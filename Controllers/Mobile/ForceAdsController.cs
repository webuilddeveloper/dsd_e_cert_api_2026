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
    public class ForceAdsController : Controller
    {
        public ForceAdsController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<ForceAds>( "forceAds");

                var filter = Builders<ForceAds>.Filter.Eq(x => x.status, "A");
               
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<ForceAds>.Filter.Regex("code", value.code); }
                if (!string.IsNullOrEmpty(value.keySearch)) { filter = filter & Builders<ForceAds>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<ForceAds>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<ForceAds>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<ForceAds>.Filter.Regex("dateStart", value.startDate); }
                //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<ForceAds>.Filter.Regex("dateEnd", value.endDate); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<ForceAds>.Filter.Gt("docDate", ds.start) & Builders<ForceAds>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<ForceAds>.Filter.Gt("docDate", ds.start) & Builders<ForceAds>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<ForceAds>.Filter.Gt("docDate", de.start) & Builders<ForceAds>.Filter.Lt("docDate", de.end); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                
                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.action, c.note, c.imageUrl, c.linkUrl, c.description, c.mainPage, c.privilegePage }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("gallery/read")]
        public ActionResult<Response> GalleryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Gallery>( "forceAdsGallery");

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
    }
}