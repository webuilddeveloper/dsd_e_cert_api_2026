using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class BannerController : Controller
    {
        public BannerController() { }

        // POST /read
        [HttpPost("gallery/read")]
        public ActionResult<Response> GalleryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Gallery>("bannerGallery");

                var filter = Builders<Gallery>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Gallery>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).Project(c => new { c.imageUrl, c.code }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("main/read")]
        public ActionResult<Response> MainRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Banner>("banner");

                var filter = Builders<Banner>.Filter.Eq(x => x.status, "A");
                filter &= Builders<Banner>.Filter.Eq("mainPage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Banner>.Filter.Regex("code", value.code); }
                if (!string.IsNullOrEmpty(value.app)) { filter = filter & Builders<Banner>.Filter.Eq("app", value.app); }
                else { filter = filter & Builders<Banner>.Filter.Nin("app", new[] { "marine", "massage", "security", "guide" }); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.imageUrlCreateBy, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.titleEN, c.imageUrl, c.linkUrl, c.description, c.descriptionEN, c.action, c.mainPage, c.contactPage, c.suggestionPage, c.note }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("contact/read")]
        public ActionResult<Response> ContactRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Banner>("banner");

                var filter = Builders<Banner>.Filter.Eq("isActive", true);
                filter &= Builders<Banner>.Filter.Eq("contactPage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Banner>.Filter.Regex("code", value.code); }
                if (!string.IsNullOrEmpty(value.app)) { filter = filter & Builders<Banner>.Filter.Eq("app", value.app); }
                else { filter = filter & Builders<Banner>.Filter.Nin("app", new[] { "marine", "massage", "security", "guide" }); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.titleEN, c.imageUrl, c.linkUrl, c.description, c.descriptionEN, c.action, c.mainPage, c.contactPage, c.suggestionPage, c.note }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("reporter/read")]
        public ActionResult<Response> ReporterRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Banner>("banner");

                var filter = Builders<Banner>.Filter.Eq("isActive", true);
                filter &= Builders<Banner>.Filter.Eq("reporterPage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Banner>.Filter.Regex("code", value.code); }
                if (!string.IsNullOrEmpty(value.app)) { filter = filter & Builders<Banner>.Filter.Eq("app", value.app); }
                else { filter = filter & Builders<Banner>.Filter.Nin("app", new[] { "marine", "massage", "security", "guide" }); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.titleEN, c.imageUrl, c.linkUrl, c.description, c.descriptionEN, c.action, c.mainPage, c.contactPage, c.note }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }
    }
}