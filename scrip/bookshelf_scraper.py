#!/usr/bin/env python3
"""DSD bookshelf -> MongoDB knowledgeCategory and knowledge."""
import argparse
import random
import fcntl
import hashlib
import tempfile
from pathlib import Path
from logging.handlers import RotatingFileHandler
from contextlib import contextmanager
import json
import logging
import os
import re
import time
from datetime import datetime, timedelta
from zoneinfo import ZoneInfo
from urllib.parse import quote, urljoin, urlsplit

import requests
from bs4 import BeautifulSoup
from requests.adapters import HTTPAdapter
from urllib3.util.retry import Retry

UPDATE_INTERVAL = 86400  # วินาที: 3600 = ทุก 1 ชั่วโมง

BASE = 'https://gcloud.dsd.go.th/~dsd-e-bookshelf/'
LOG = logging.getLogger('bookshelf')
from pymongo import MongoClient


def soup(content):
    # Source declares TIS-620; using response.text defaults to Latin-1 incorrectly.
    return BeautifulSoup(content, 'html.parser', from_encoding='tis-620')


def generate_code():
    # Extension.toCode(): yyyyMMddHHmmss-millisecond-Random.Next(100, 999)
    now = datetime.now()
    return f"{now:%Y%m%d%H%M%S}-{now.microsecond // 1000}-{random.randrange(100, 999)}"


def parse_categories(content):
    result = []
    for card in soup(content).select('.bookcard'):
        name_node = card.select_one('[id="catname"]')
        if name_node is None:
            continue
        name = name_node.get_text(' ', strip=True)
        count = re.search(r'\((\d+)\)\s*$', card.get_text(' ', strip=True))
        if not name or count is None:
            raise ValueError('Category markup changed: missing name/count')
        result.append({'code': name, 'title': name,
                       'expected_count': int(count.group(1))})
    if not result:
        raise ValueError('No categories found; check website markup')
    return result


def parse_books(content):
    result = {}
    for card in soup(content).select('.bookcard'):
        def value(selector):
            node = card.select_one(selector)
            return node.get('value', '').strip() if node else ''
        bid = value('.bid')
        if not bid:
            continue
        title = card.select_one('.card-title')
        if not bid.isdigit() or title is None or not title.get_text(strip=True):
            raise ValueError('Invalid book ID/title')
        img = card.select_one('img')
        src = img.get('src', '') if img else ''
        result[bid] = {
            'source_id': bid, 'title': title.get_text(' ', strip=True),
            'author': value('.cauthor'), 'description': value('.ctitle'),
            'imageUrl': src if src.startswith('data:') else urljoin(BASE, src) if src else '',
            'source_view': int(value('.rcount') or 0),
            'source_downloads': int(value('.lcount') or 0),
            'linkUrl': urljoin(BASE, 'read.php?rbid=' + bid),
            'downloadUrl': urljoin(BASE, 'download.php?bid=' + bid),
        }
    return result


class Source:
    def __init__(self, delay):
        self.delay = delay
        self.session = requests.Session()
        retry = Retry(total=3, backoff_factor=1, status_forcelist=[429, 500, 502, 503, 504],
                      allowed_methods=['GET'], respect_retry_after_header=True)
        self.session.mount('https://', HTTPAdapter(max_retries=retry))
        self.session.headers['User-Agent'] = 'DSD-Bookshelf-Importer/1.0'

    def get(self, url):
        if not url.startswith(BASE):
            raise ValueError('Unexpected source URL: ' + url)
        time.sleep(self.delay)
        response = self.session.get(url, timeout=(15, 60))
        response.raise_for_status()
        return response.content

    def search(self, name):
        return self.listing(BASE + 'search.php?search=' + quote(name, encoding='tis-620'))

    def listing(self, url):
        pending, visited, books = [url], set(), {}
        while pending:
            url = pending.pop(0)
            if url in visited:
                continue
            visited.add(url)
            if len(visited) > 1000:
                raise ValueError('Pagination exceeded 1000 pages')
            content = self.get(url)
            books.update(parse_books(content))
            for a in soup(content).select('a[href]'):
                href = a['href']
                # The current page has no pagination; follow it if supplied later.
                if re.search(r'(?:[?&])page=\d+', href):
                    nxt = urljoin(url, href)
                    if urlsplit(nxt).path == urlsplit(url).path and nxt.startswith(BASE):
                        pending.append(nxt)
        return books


def crawl(source, include_shelves=True):
    categories = parse_categories(source.get(BASE + 'category.php'))
    by_name = {c['title']: c for c in categories}
    books, memberships = {}, set()
    for cat in categories:
        found = source.search(cat['title'])
        matched = set()
        for bid, book in found.items():
            if bid not in books:
                raw = json.loads(source.get(BASE + 'getcat.php?bid=' + bid))
                if not isinstance(raw, list) or any(not isinstance(x, dict) or 'CATNAME' not in x for x in raw):
                    raise ValueError('Invalid category response for book ' + bid)
                book['category_names'] = [x['CATNAME'].strip() for x in raw]
                read_html = source.get(book['linkUrl']).decode('tis-620')
                pdf = re.search(r"\b(?:const|let|var)\s+pdfPath\s*=\s*['\"]([^'\"]+)['\"]", read_html)
                if not pdf:
                    raise ValueError('PDF link missing for book ' + bid)
                book['fileUrl'] = urljoin(BASE, pdf.group(1))
                books[bid] = book
            for name in books[bid]['category_names']:
                if name not in by_name:
                    raise ValueError('Unknown category: ' + name)
                memberships.add((by_name[name]['code'], bid))
            if cat['title'] in books[bid]['category_names']:
                matched.add(bid)
        LOG.info('หมวด %s: พบ %d เล่ม จากที่เว็บระบุ %d เล่ม', cat['title'], len(matched), cat['expected_count'])
        if len(matched) != cat['expected_count']:
            raise ValueError('Category count mismatch for ' + cat['title'] + '; MongoDB not updated')
    # Discover shelf destinations from the homepage rather than assuming categories.
    home = soup(source.get(BASE + 'index.php'))
    for label in ('หนังสือใหม่', 'หนังสือยอดนิยม'):
        heading = next((h for h in home.select('h4') if h.get_text(strip=True) == label), None)
        if heading is None:
            raise ValueError('Missing homepage section: ' + label)
        anchor = heading.parent.select_one('a[href]')
        if anchor is None:
            raise ValueError('Missing shelf listing link: ' + label)
        found = source.listing(urljoin(BASE, anchor['href']))
        if not found:
            raise ValueError('Empty shelf listing: ' + label)
        for bid, book in found.items():
            if bid not in books:
                raw = json.loads(source.get(BASE + 'getcat.php?bid=' + bid))
                if not isinstance(raw, list) or any(not isinstance(x, dict) or not isinstance(x.get('CATNAME'), str) for x in raw):
                    raise ValueError('Invalid category response for book ' + bid)
                book['category_names'] = [x['CATNAME'].strip() for x in raw]
                read_html = source.get(book['linkUrl']).decode('tis-620')
                pdf = re.search(r"\b(?:const|let|var)\s+pdfPath\s*=\s*['\"]([^'\"]+)['\"]", read_html)
                if not pdf:
                    raise ValueError('PDF link missing for book ' + bid)
                book['fileUrl'] = urljoin(BASE, pdf.group(1))
                books[bid] = book
                for name in book['category_names']:
                    if name not in by_name:
                        raise ValueError('Unknown category: ' + name)
                    memberships.add((name, bid))
            if include_shelves:
                memberships.add((label, bid))
        if include_shelves:
            categories.append({'code': label, 'title': label, 'expected_count': len(found),
                               'book_order': list(found)})
        LOG.info('%s: พบ %d เล่ม', label, len(found))
    return categories, books, memberships


def normalized_title(value):
    return ' '.join((value or '').split())


def same_source_url(left, right):
    if not left or not right:
        return False
    # Legacy http/https links may point to the same DSD resource.
    a, b = urlsplit(left.strip()), urlsplit(right.strip())
    return (a.netloc.lower(), a.path, a.query) == (b.netloc.lower(), b.path, b.query)


def select_existing(documents, title, book=None):
    matches = []
    if book:
        matches = [d for d in documents if same_source_url(d.get('fileUrl'), book['fileUrl'])
                   or same_source_url(d.get('linkUrl'), book['linkUrl'])]
    if not matches:
        matches = [d for d in documents if normalized_title(d.get('title')) == normalized_title(title)]
    if len(matches) > 1:
        raise ValueError('Ambiguous existing records for ' + title + '; resolve duplicates before import')
    return matches[0] if matches else None


def save_mongo(db, categories, books, memberships, dry_run=False):
    now = datetime.now(ZoneInfo('Asia/Bangkok'))
    stamp, clock = now.strftime('%Y%m%d%H%M%S'), now.strftime('%H:%M:%S')
    base = {'isActive': True, 'status': 'A', 'createBy': 'dsd-bookshelf',
            'createDate': stamp, 'createTime': clock,
            'docDate': now.replace(hour=7, minute=0, second=0, microsecond=0),
            'docTime': clock, 'sequence': 0, 'titleEN': '', 'language': 'th', 'imageUrl': ''}
    knowledge_defaults = dict(base, isHighlight=False, isPublic=False, view=0,
                              author='', authorEN='', description='', descriptionEN='',
                              publisher='', publisherEN='', numberOfPages=0, size='', publishDate='',
                              imageUrlCreateBy='', textButton='อ่านหนังสือ', textButtonEN='Read',
                              bookType='PDF', **{f'lv{i}': '' for i in range(6)})
    # Read existing API documents, regardless of creator/code prefix.
    existing_categories = list(db.knowledgeCategory.find({}))
    existing_books = list(db.knowledge.find({}))
    used_codes = {d.get('code') for d in existing_categories + existing_books}
    plans, category_codes, sequences = [], {}, {}

    def plan(collection, old, fields, defaults):
        if old:
            code = old['code']
        else:
            for _ in range(100):
                code = generate_code()
                if code not in used_codes:
                    break
            else:
                raise ValueError('Unable to generate an unused code')
            used_codes.add(code)
        new = dict(defaults, **fields, code=code)
        if old:
            # Preserve CMS translations, dates, visibility, view counts and uploaded images.
            changes = {k: v for k, v in defaults.items() if k not in old}
            for k, v in fields.items():
                if k == 'imageUrl' and old.get(k) and not old[k].startswith('data:'):
                    continue
                if v not in ('', None) and old.get(k) != v:
                    changes[k] = v
            if changes:
                changes.update(updateBy='dsd-bookshelf', updateDate=stamp, updateTime=clock)
                plans.append((collection, old, changes))
        else:
            new.update(updateBy='dsd-bookshelf', updateDate=stamp, updateTime=clock)
            plans.append((collection, None, new))
        return code

    priority = {'หนังสือใหม่': 0, 'หนังสือยอดนิยม': 1}
    ordered_categories = sorted(categories, key=lambda cat: priority.get(cat['title'], 2))
    for category_sequence, cat in enumerate(ordered_categories):
        old = select_existing(existing_categories, cat['title'])
        category_codes[cat['code']] = plan('knowledgeCategory', old, {'title': cat['title'], 'sequence': category_sequence}, base)
        for index, bid in enumerate(cat.get('book_order', [])):
            sequences[(cat['code'], bid)] = index
    for category_key, bid in sorted(memberships):
        book, category = books[bid], category_codes[category_key]
        candidates = [d for d in existing_books if d.get('category') == category]
        old = select_existing(candidates, book['title'], book)
        fields = {k: book[k] for k in ('title', 'author', 'description', 'imageUrl', 'fileUrl', 'linkUrl')}
        fields['category'] = category
        if (category_key, bid) in sequences:
            fields['sequence'] = sequences[(category_key, bid)]
        plan('knowledge', old, fields, knowledge_defaults)
    result = {'insert': sum(old is None for _, old, _ in plans),
              'update': sum(old is not None for _, old, _ in plans),
              'unchanged': len(categories) + len(memberships) - len(plans)}
    if not dry_run:
        for collection, old, document in plans:
            if old:
                db[collection].update_one({'_id': old['_id']}, {'$set': document})
            else:
                db[collection].insert_one(document)
    LOG.info('%s ฐานข้อมูล %s | เพิ่มใหม่ %d | อัปเดต %d | ข้อมูลเดิมไม่เปลี่ยน %d รายการ',
             'ตรวจสอบโดยไม่บันทึก' if dry_run else 'บันทึกสำเร็จ', db.name,
             result['insert'], result['update'], result['unchanged'])
    return result


@contextmanager
def run_lock(uri, database):
    # The same script/database must not run concurrently on this host.
    key = hashlib.sha256((uri + '/' + database).encode()).hexdigest()
    with open(os.path.join(tempfile.gettempdir(), 'dsd-bookshelf-' + key + '.lock'), 'a') as handle:
        try:
            fcntl.flock(handle, fcntl.LOCK_EX | fcntl.LOCK_NB)
        except BlockingIOError as exc:
            raise ValueError('มีสคริปต์อีกตัวกำลังทำงานกับฐานข้อมูลนี้อยู่ กรุณาใช้เพียงตัวเดียว') from exc
        try:
            yield
        finally:
            fcntl.flock(handle, fcntl.LOCK_UN)


def show_mongo(db, category=None, limit=20):
    categories = list(db.knowledgeCategory.find(
        {}, {'_id': 0, 'code': 1, 'title': 1, 'sequence': 1}).sort([('sequence', 1), ('title', 1)]))
    names = {c['code']: c['title'] for c in categories}
    counts = {row['_id']: row['count'] for row in db.knowledge.aggregate([
        {'$group': {'_id': '$category', 'count': {'$sum': 1}}}])}
    print('MongoDB:', db.name)
    for c in categories:
        print(f"{counts.get(c['code'], 0):4}  {c['title']}")
    query = {}
    if category:
        query['category'] = {'$in': [c['code'] for c in categories if c['title'] == category]}
    if not categories:
        print('ยังไม่มีหมวดหมู่ที่นำเข้าด้วยสคริปต์นี้')
    if limit == 0:
        return
    for b in db.knowledge.find(query, {'_id': 0, 'code': 1, 'title': 1, 'category': 1,
                                       'fileUrl': 1}).sort([('sequence', 1), ('code', 1)]).limit(limit):
        print(f"[{b['code']}] {b['title']}\n  หมวด: {names.get(b['category'], b['category'])}\n  PDF: {b.get('fileUrl', '')}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--show', action='store_true', help='ดูข้อมูล MongoDB แล้วจบการทำงาน')
    parser.add_argument('--delay', type=float, default=0.3)
    parser.add_argument('--dry-run', action='store_true', help='Read source and MongoDB; report changes without writing')
    parser.add_argument('--no-shelf-categories', action='store_true', help='Fetch shelves but keep only original categories')
    parser.add_argument('--category', help='Exact Thai category name for show')
    parser.add_argument('--limit', type=int, default=20)
    parser.add_argument('--mongo-uri', default=os.environ.get('MONGO_URI', 'mongodb://127.0.0.1:27017'))
    parser.add_argument('--mongo-db', default=os.environ.get('MONGO_DB', 'dsd_e_prod'))
    args = parser.parse_args()
    log_path = Path(__file__).resolve().with_suffix('.log')
    logging.basicConfig(level=logging.INFO, format='%(asctime)s | %(message)s',
                        datefmt='%d/%m/%Y %H:%M:%S', handlers=[
                            logging.StreamHandler(),
                            RotatingFileHandler(log_path, maxBytes=5_000_000, backupCount=3, encoding='utf-8')])
    if args.delay < 0 or args.limit < 0 or UPDATE_INTERVAL <= 0:
        parser.error('--delay/--limit ต้องไม่ติดลบ และ UPDATE_INTERVAL ต้องมากกว่า 0')
    if args.show and args.dry_run:
        parser.error('เลือก --show หรือ --dry-run อย่างใดอย่างหนึ่ง')

    def run():
        with MongoClient(args.mongo_uri, serverSelectionTimeoutMS=5000) as client:
            client.admin.command('ping')
            db = client[args.mongo_db]
            LOG.info('เชื่อมต่อ MongoDB สำเร็จ | ฐานข้อมูล %s', args.mongo_db)
            if args.show:
                show_mongo(db, args.category, args.limit)
                return
            LOG.info('เริ่มดึงหมวดหมู่ หนังสือใหม่ และหนังสือยอดนิยมจากเว็บ DSD')
            source = Source(args.delay)
            try:
                cats, books, memberships = crawl(source, not args.no_shelf_categories)
            finally:
                source.session.close()
            LOG.info('ดึงข้อมูลครบ | %d หมวด | หนังสือจริง %d เล่ม | ความสัมพันธ์หนังสือกับหมวด %d รายการ',
                     len(cats), len(books), len(memberships))
            save_mongo(db, cats, books, memberships, dry_run=args.dry_run)

    def attempt():
        try:
            run()
            return 0
        except Exception as exc:
            LOG.error('%s: %s', type(exc).__name__, str(exc) if isinstance(exc, ValueError)
                      else 'ทำรายการไม่สำเร็จ กรุณาตรวจการเชื่อมต่อเว็บ DSD หรือ MongoDB')
            return 1

    try:
        if args.show:
            return attempt()
        with run_lock(args.mongo_uri, args.mongo_db):
            if args.dry_run:
                return attempt()
            interval_hours, interval_minutes = divmod(int(UPDATE_INTERVAL // 60), 60)
            LOG.info('เริ่มทำงานอัตโนมัติ ทุก %d ชั่วโมง %d นาที | ไฟล์ log: %s | กด Ctrl+C เพื่อหยุด',
                     interval_hours, interval_minutes, log_path)
            round_number = 0
            while True:
                round_number += 1
                LOG.info('เริ่มรอบที่ %d', round_number)
                started = time.monotonic()
                status = attempt()
                elapsed = time.monotonic() - started
                LOG.info('จบรอบที่ %d | %s | ใช้เวลา %.0f วินาที', round_number,
                         'สำเร็จ' if status == 0 else 'ไม่สำเร็จ จะลองใหม่ในรอบถัดไป', elapsed)
                remaining = max(0, UPDATE_INTERVAL - elapsed)
                next_run = datetime.now(ZoneInfo('Asia/Bangkok')) + timedelta(seconds=remaining)
                LOG.info('รอบถัดไป %s (เวลาไทย) | รออีก %.1f นาที',
                         next_run.strftime('%d/%m/%Y %H:%M:%S'), remaining / 60)
                time.sleep(remaining)
    except KeyboardInterrupt:
        LOG.info('หยุดการทำงานแล้ว')
        return 0
    except ValueError as exc:
        LOG.error('%s', exc)
        return 1


if __name__ == '__main__':
    raise SystemExit(main())
