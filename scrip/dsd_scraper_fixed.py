import requests # type: ignore
import re
import time
import logging
import random
from datetime import datetime, timezone
from html import unescape
from urllib.parse import urljoin

from bs4 import BeautifulSoup # pyright: ignore[reportMissingImports]
from pymongo import MongoClient, UpdateOne # type: ignore
from pymongo.errors import BulkWriteError # type: ignore

# ─────────────────────────────────────────────
# CONFIG
# ─────────────────────────────────────────────
MONGO_URI        = "mongodb://127.0.0.1:27017"
MONGO_DB         = "dsd_e_prod"
MONGO_COLLECTION = "news"

# UPLOAD_API_URL   = f"https://khubdeedlt.we-builds.com/khubdeedlt-document/upload"
UPLOAD_API_URL   = f"https://localhost:5101/upload"   #server


CATEGORIES = [
    {"index": 3, "url": "https://www.dsd.go.th/DSD/Activity/index/3", "name": "ข่าวฝึกอบรม"},
    {"index": 6, "url": "https://www.dsd.go.th/DSD/Activity/index/6", "name": "ข่าวมาตรฐานฝีมือแรงงาน"},
    {"index": 8, "url": "https://www.dsd.go.th/DSD/Activity/index/8", "name": "ข่าวการรับรองความรู้ความสามารถ"},
]

MAX_PAGES        = 1    #งแค่หน้าแรกของแต่ละ category
UPDATE_INTERVAL  = 3600 # รันซ้ำทุก 3600 วินาที (1 ชั่วโมง)
BASE_URL         = "https://www.dsd.go.th"
UPLOAD_TIMEOUT   = 15
UPLOAD_API_UP    = True  # สถานะ API — update โดย check_upload_api()

# ─────────────────────────────────────────────
# LOGGING
# ─────────────────────────────────────────────
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S",
    handlers=[
        logging.StreamHandler(),
        logging.FileHandler("dsd_scraper.log", encoding="utf-8"),
    ]
)
log = logging.getLogger(__name__)

HEADERS = {
    "User-Agent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36",
    "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
    "Accept-Language": "th-TH,th;q=0.9,en;q=0.8",
    "Referer": BASE_URL,
}


# ─────────────────────────────────────────────
# UPLOAD API HEALTH CHECK
# ─────────────────────────────────────────────
def check_upload_api() -> bool:
    """
    ทดสอบ connectivity ไปที่ upload API
    คืน True ถ้าเชื่อมต่อได้, False ถ้าไม่ได้
    """
    global UPLOAD_API_UP  # global ตัวแปรที่กำลังจะใช้งานภายในฟังก์ชัน
    try:
        # POST เบา ๆ ทดสอบ connection (endpoint รับแค่ POST)
        r = requests.post(UPLOAD_API_URL, data={}, timeout=5)
        UPLOAD_API_UP = r.status_code < 500
        if UPLOAD_API_UP:
            log.info(f"✓ Upload API เชื่อมต่อได้ ({r.status_code})")
        else:
            log.warning(f"⚠️  Upload API ตอบ {r.status_code} — จะข้ามการ upload รูป")
    except Exception as e:
        UPLOAD_API_UP = False
        log.warning(f"⚠️  Upload API เข้าไม่ได้: {str(e)[:80]}")
        log.warning("   → จะบันทึกข่าวโดยไม่มีรูป (imageUrl = '')")
    return UPLOAD_API_UP


# ─────────────────────────────────────────────
# HELPERS
# ─────────────────────────────────────────────
def generate_code() -> str:
    now  = datetime.now()
    ts   = now.strftime("%Y%m%d%H%M%S")
    r1   = random.randint(100, 999)
    r2   = random.randint(100, 999)
    return f"{ts}-{r1}-{r2}"


def parse_thai_date(date_str: str) -> datetime | None:
    thai_months = {
        "มกราคม": 1, "กุมภาพันธ์": 2, "มีนาคม": 3, "เมษายน": 4,
        "พฤษภาคม": 5, "มิถุนายน": 6, "กรกฎาคม": 7, "สิงหาคม": 8,
        "กันยายน": 9, "ตุลาคม": 10, "พฤศจิกายน": 11, "ธันวาคม": 12,
        "ม.ค.": 1, "ก.พ.": 2, "มี.ค.": 3, "เม.ย.": 4,
        "พ.ค.": 5, "มิ.ย.": 6, "ก.ค.": 7, "ส.ค.": 8,
        "ก.ย.": 9, "ต.ค.": 10, "พ.ย.": 11, "ธ.ค.": 12,
    }
    if not date_str:
        return None
    try:
        parts = date_str.strip().split()
        if len(parts) == 3:
            day   = int(parts[0])
            month = thai_months.get(parts[1], 0)
            year  = int(parts[2]) - 543
            if month:
                return datetime(year, month, day, tzinfo=timezone.utc)
    except Exception:
        pass
    return None


# ─────────────────────────────────────────────
# IMAGE UPLOAD
# ─────────────────────────────────────────────
def upload_image(dsd_image_url: str) -> str:
    """
    Upload รูปไป API — ถ้า API down ให้คืน '' ทันที (ไม่รอ timeout)
    """
    if not dsd_image_url:
        return ""

    # API down → ข้ามเลย
    if not UPLOAD_API_UP:
        return ""

    # download รูป
    try:
        r = requests.get(dsd_image_url, headers=HEADERS, timeout=UPLOAD_TIMEOUT, stream=True)
        r.raise_for_status()
        content_type = r.headers.get("Content-Type", "image/jpeg")
        if "image" not in content_type and "octet-stream" not in content_type:
            log.warning(f"   ⚠️  ไม่มีรูปภาพ ({content_type})")
            return ""
        image_data = r.content
    except Exception as e:
        log.error(f"   ❌ download: {str(e)[:60]}")
        return ""

    # upload
    now      = datetime.now()
    ext      = next((e for e in [".jpg", ".jpeg", ".png", ".gif", ".webp"]
                     if e in dsd_image_url.lower()), ".jpg")
    # filename = f"news_{now.strftime('%Y%m%d')}_{random.randint(1000, 9999)}{ext}"
    rand_id  = random.randint(1000, 9999)
    filename = f"news_{now.strftime('%Y%m%d')}_{rand_id}{ext}"  # folder = news_20260617.jpg
                              # ชื่อไฟล์ = 1542_xxx.jpg

    try:
        up = requests.post(
            UPLOAD_API_URL,
            files={"Image": (filename, image_data, content_type or "image/jpeg")},
            data={"ImageCaption": "dsd-news"},
            timeout=UPLOAD_TIMEOUT
        )
        up.raise_for_status()
        result     = up.json()
        public_url = re.sub(r'([^:])//+', r'\1/', result.get("imageUrl", "").strip())
        if not public_url:
            log.error(f"   ❌ API ไม่ส่ง imageUrl: {result}")
            return ""
        log.info(f"   📤 uploaded: {filename} ({len(image_data)//1024}KB)")
        return public_url
    except Exception as e:
        log.error(f"   ❌ upload: {str(e)[:60]}")
        return ""


# ─────────────────────────────────────────────
# MONGODB
# ─────────────────────────────────────────────
def get_collection():
    client = MongoClient(MONGO_URI, serverSelectionTimeoutMS=5000)
    col    = client[MONGO_DB][MONGO_COLLECTION]

  
    existing = {i["name"] for i in col.list_indexes()}  
  # linkdsd = unique key หลัก
    if "linkdsd_1" not in existing:
        col.create_index("linkdsd", unique=True, sparse=True)
        log.info("✓ สร้าง index: linkdsd (unique)")

    if "code_1" not in existing:
        col.create_index("code", unique=True, sparse=True)
        log.info("✓ สร้าง index: code (unique, sparse)")

    col.create_index([("category", 1), ("isActive", 1)])
    col.create_index("docDate")
    return col


def save_to_mongo(col, news_list: list) -> dict:
    if not news_list:
        return {"upserted": 0, "modified": 0}

    ops = [
        UpdateOne(
            {"linkdsd": n["linkdsd"]},          # match ด้วย linkdsd (unique identifier)
            {"$set": n},                         # set ทั้งหมด (code จะมีค่า reuse หรือ gen ใหม่)
            upsert=True
        )
        for n in news_list
    ]
    try:
        result = col.bulk_write(ops, ordered=False)
        return {"upserted": result.upserted_count, "modified": result.modified_count}
    except BulkWriteError as bwe:
        # กรณี duplicate key error บาง doc → นับ success
        details = bwe.details
        ok = details.get("nInserted", 0) + details.get("nUpserted", 0) + details.get("nModified", 0)
        log.warning(f"BulkWrite partial: {ok} ok, errors: {len(details.get('writeErrors', []))}")
        return {"upserted": details.get("nUpserted", 0), "modified": details.get("nModified", 0)}


# ─────────────────────────────────────────────
# SCRAPER
# ─────────────────────────────────────────────
def fix_src_url(src: str, page_url: str) -> str:
    if not src:
        return ""
    return urljoin(page_url, src.strip())


def get_news_links(category_url: str, page: int = 1) -> list:
    try:
        url  = f"{category_url}?page={page}" if page > 1 else category_url
        resp = requests.get(url, headers=HEADERS, timeout=15)
        resp.raise_for_status()
        soup  = BeautifulSoup(resp.text, "html.parser")
        links = []
        for a in soup.find_all('a', href=True):
            if 'Activity/ShowDetails' in a['href']:
                full = urljoin(BASE_URL, a['href'])
                if full not in links:
                    links.append(full)
        for m in re.finditer(r'["\']([^"\']*Activity/ShowDetails/\d+[^"\']*)["\']', resp.text):
            full = urljoin(BASE_URL, unescape(m.group(1)))
            if full not in links:
                links.append(full)
        return list(dict.fromkeys(links))
    except Exception as e:
        log.error(f"get_links error: {e}")
        return []


def extract_news(news_url: str, cat: dict, col) -> dict | None:
    """
    Extract ข่าว + smart code assignment:
    - ถ้าข่าว linkdsd นี้มีอยู่แล้ว → reuse code เก่า (_is_new = False)
    - ถ้าข่าวใหม่ → gen code ใหม่ (_is_new = True)
    """
    try:
        resp = requests.get(news_url, headers=HEADERS, timeout=15)
        resp.raise_for_status()
        soup = BeautifulSoup(resp.text, "html.parser")

        # TITLE 
        title = ""
        for h1 in soup.find_all('h1'):
            cls = ' '.join(h1.get('class', []))
            if 'txt-bold' in cls and 'c-gray' in cls:
                continue
            if h1.find_parent('footer'):
                continue
            text = h1.get_text(" ", strip=True)
            if text in ["กรมพัฒนาฝีมือแรงงาน", "Department of skill Development"]:
                continue
            if text and len(text) > 5:
                title = text
                break
        if not title:
            return None

        # DATE 
        date_str = ""
        date_tag = soup.find('p', class_=lambda c: c and 'date' in c)
        if date_tag:
            for content in date_tag.contents:
                if isinstance(content, str):
                    t = content.strip()
                    if t:
                        date_str = t
                        break
        if not date_str:
            m2 = re.search(r'\d{1,2}\s+[ก-๙\.]{2,10}\s+25\d\d', soup.get_text())
            if m2:
                date_str = m2.group(0)

        doc_date = parse_thai_date(date_str) or datetime.now(timezone.utc)

        # DESCRIPTION
        description = ""
        span = soup.find('span', style=lambda s: s and 'word-wrap' in s and 'padding' in s)
        if span:
            paras = [p.get_text(" ", strip=True).replace('\xa0', '').strip()
                     for p in span.find_all('p')]
            description = "\n".join(p for p in paras if p)

        # IMAGE
        dsd_image_url = ""
        img_thumb = soup.find('div', class_='image_thumb')
        if img_thumb:
            img = img_thumb.find('img')
            if img and img.get('src'):
                dsd_image_url = fix_src_url(img['src'], news_url)
        if not dsd_image_url:
            img = soup.find('img', class_=lambda c: c and 'img-detail-activity' in (
                c if isinstance(c, str) else ' '.join(c)))
            if img and img.get('src'):
                dsd_image_url = fix_src_url(img['src'], news_url)
        if not dsd_image_url:
            m3 = re.search(r'src=["\'](\~/filedownload_edoc\.ashx\?[^"\']+)["\']', resp.text)
            if m3:
                dsd_image_url = fix_src_url(m3.group(1), news_url)

        image_url = upload_image(dsd_image_url)

        now         = datetime.now()
        create_date = now.strftime("%Y%m%d%H%M%S")
        create_time = now.strftime("%H:%M:%S")

        # ✨ V15: URL นี้ผ่านการ filter จาก scrape_all() แล้ว → ใหม่แน่นอน
        code   = generate_code()
        is_new = True

        return {
            "code":             code,               # gen ใหม่ (ถ้าข่าวใหม่) หรือ reuse (ถ้าข้อมูลเดิม)
            "sequence":         10,
            "title":            title.strip(),
            "description":      description.strip()[:1000],
            "titleEN":          "",
            "descriptionEN":    "",
            "imageUrl":         image_url,
            "category":         str(cat["index"]), 
            "language":         "th",
            "fileUrl":          "",
            "linkUrl":          "",           
            "textButton":       "",
            "textButtonEN":     "",
            "imageUrlCreateBy": "",
            "createBy":         "dsd_scraper",
            "createDate":       str(create_date),
            "createTime":       str(create_time),
            "updateBy":         "dsd_scraper",
            "updateDate":       str(create_date),
            "updateTime":       str(create_time),
            "docDate":          str(doc_date),
            "docTime":          str(create_time),
            "isActive":         True,
            "isHighlight":      False,
            "status":           "A",
            "lv0":              "",
            "lv1":              "",
            "lv2":              "",
            "lv3":              "",
            "lv4":              "",
            "lv5":              "",
            "isPublic":         True,
    
            "_is_new":          is_new,    
            "linkdsd":          news_url,      # internal flag: True = ข่าวใหม่, False = เดิม
        }

    except Exception as e:
        log.error(f"extract error ({news_url[-30:]}): {e}")
        return None


# ─────────────────────────────────────────────
# MAIN LOOP
# ─────────────────────────────────────────────
def scrape_all(col) -> tuple[list, list]:
    """คืน (news_list, cat_summaries) สำหรับ log สรุป"""
    all_news     = []
    cat_summaries = []  # [{"name": ..., "new": N, "skip": M, "fail": K}]

    for cat in CATEGORIES:
        links = []
        for page in range(1, MAX_PAGES + 1):
            page_links = get_news_links(cat['url'], page)
            if not page_links:
                break
            links.extend(page_links)
            time.sleep(1)
        links = list(dict.fromkeys(links))

        known_urls = {
            doc["linkdsd"]
            for doc in col.find({"linkdsd": {"$in": links}}, {"linkdsd": 1})
        }
        new_links  = [l for l in links if l not in known_urls]
        skip_count = len(links) - len(new_links)
        fail_count = 0

        for link in new_links:
            news = extract_news(link, cat, col)
            if news:
                all_news.append(news)
            else:
                fail_count += 1
            time.sleep(0.5)
        time.sleep(2)

        cat_summaries.append({
            "name":  cat["name"],
            "new":   len(new_links) - fail_count,
            "skip":  skip_count,
            "fail":  fail_count,
        })

    return all_news, cat_summaries


def run_once(col, round_no: int):
    api_ok    = check_upload_api()
    start     = time.time()
    news_list, cat_summaries = scrape_all(col)
    elapsed   = int(time.time() - start)

    # ── header รอบ ──
    now_str = datetime.now().strftime("%d/%m/%Y %H:%M")
    log.info(f"[รอบที่ {round_no}]  {now_str}")

    # ── สรุปแต่ละ category ──
    for s in cat_summaries:
        if s["new"] > 0:
            fail_str = f"  ✗ ดึงไม่ได้ {s['fail']}" if s["fail"] else ""
            log.info(f"  {s['name']:<30}  ใหม่ {s['new']}  ข้าม {s['skip']}{fail_str}")
        else:
            log.info(f"  {s['name']:<30}  ข้าม {s['skip']}")

    log.info(f"  {'─' * 45}")

    if not news_list:
        log.info(f"  – ไม่มีข่าวใหม่")
    else:
        for n in news_list:
            n.pop("_is_new", None)
        stats    = save_to_mongo(col, news_list)
        with_img = sum(1 for n in news_list if n.get("imageUrl"))
        log.info(f"  ✓ บันทึก {len(news_list)} รายการ  รูป {with_img}/{len(news_list)}  เวลา {elapsed}s")
        if not api_ok:
            log.warning(f"  ⚠  Upload API ไม่พร้อม — รูปจะ upload รอบถัดไป")

    wait_min = UPDATE_INTERVAL // 60
    log.info(f"  รอ {wait_min} นาที...")
    log.info("")


def main():
    log.info("=" * 60)
    log.info(f"  DSD NEWS SCRAPER v15")
    log.info(f"  Upload API : {UPLOAD_API_URL}")
    log.info(f"  Interval   : {UPDATE_INTERVAL // 60} นาที")
    log.info("=" * 60)
    log.info("")

    col      = get_collection()
    log.info("  MongoDB connected")
    log.info("")

    round_no = 1
    while True:
        run_once(col, round_no)
        round_no += 1
        time.sleep(UPDATE_INTERVAL)


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        log.info("  หยุดโดยผู้ใช้")
    except Exception as e:
        log.exception(f"  ERROR: {e}")