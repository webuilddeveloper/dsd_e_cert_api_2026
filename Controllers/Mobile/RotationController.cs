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
    public class RotationController : Controller
    {
        public RotationController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Rotation>( "rotation");

                var filter = Builders<Rotation>.Filter.Eq(x => x.status, "A");
                
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Rotation>.Filter.Regex("code", value.code); }
                if (!string.IsNullOrEmpty(value.keySearch)) { filter = filter & Builders<Rotation>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Rotation>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Rotation>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Rotation>.Filter.Regex("dateStart", value.startDate); }
                //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Rotation>.Filter.Regex("dateEnd", value.endDate); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Rotation>.Filter.Gt("docDate", ds.start) & Builders<Rotation>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Rotation>.Filter.Gt("docDate", ds.start) & Builders<Rotation>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Rotation>.Filter.Gt("docDate", de.start) & Builders<Rotation>.Filter.Lt("docDate", de.end); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                
                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.imageUrl, c.linkUrl, c.description, c.action , c.note, c.newsPage, c.eventPage, c.privilegePage, c.mainPage, c.poiPage, c.notificationPage }).ToList();

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
                var col = new Database().MongoClient<Gallery>( "rotationGallery");

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
                var col = new Database().MongoClient<Rotation>( "rotation");

                var filter = Builders<Rotation>.Filter.Eq("status", "A");
                filter &= Builders<Rotation>.Filter.Eq("mainPage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Rotation>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.imageUrl, c.linkUrl, c.description, c.action, c.note, c.newsPage, c.eventPage, c.privilegePage, c.mainPage, c.poiPage, c.notificationPage }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("news/read")]
        public ActionResult<Response> NewsRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Rotation>( "rotation");

                var filter = Builders<Rotation>.Filter.Eq("status", "A");
                filter &= Builders<Rotation>.Filter.Eq("newsPage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Rotation>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.imageUrl, c.linkUrl, c.description, c.action, c.note, c.newsPage, c.eventPage, c.privilegePage, c.mainPage, c.poiPage, c.notificationPage }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("fund/read")]
        public ActionResult<Response> FundRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Rotation>("rotation");

                var filter = Builders<Rotation>.Filter.Eq("status", "A");
                filter &= Builders<Rotation>.Filter.Eq("fundPage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Rotation>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.imageUrl, c.linkUrl, c.description, c.action, c.note, c.newsPage, c.eventPage, c.privilegePage, c.mainPage, c.poiPage, c.notificationPage }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("privilege/read")]
        public ActionResult<Response> PrivilegeRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Rotation>( "rotation");

                var filter = Builders<Rotation>.Filter.Eq("status", "A");
                filter &= Builders<Rotation>.Filter.Eq("privilegePage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Rotation>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.imageUrl, c.linkUrl, c.description, c.action, c.note, c.newsPage, c.eventPage, c.privilegePage, c.mainPage, c.poiPage, c.notificationPage }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("notification/read")]
        public ActionResult<Response> NotificationRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Rotation>( "rotation");

                var filter = Builders<Rotation>.Filter.Eq("status", "A");
                filter &= Builders<Rotation>.Filter.Eq("notificationPage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Rotation>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.imageUrl, c.linkUrl, c.description, c.action, c.note, c.newsPage, c.eventPage, c.privilegePage, c.mainPage, c.poiPage, c.notificationPage }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("event/read")]
        public ActionResult<Response> EventRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Rotation>( "rotation");

                var filter = Builders<Rotation>.Filter.Eq("status", "A");
                filter &= Builders<Rotation>.Filter.Eq("eventPage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Rotation>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.imageUrl, c.linkUrl, c.description, c.action, c.note, c.newsPage, c.eventPage, c.privilegePage, c.mainPage, c.poiPage, c.notificationPage }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("poi/read")]
        public ActionResult<Response> POIRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Rotation>( "rotation");

                var filter = Builders<Rotation>.Filter.Eq("status", "A");
                filter &= Builders<Rotation>.Filter.Eq("poiPage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Rotation>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.imageUrl, c.linkUrl, c.description, c.action, c.note, c.newsPage, c.eventPage, c.privilegePage, c.mainPage, c.poiPage, c.notificationPage }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("warning/read")]
        public ActionResult<Response> WarningRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Rotation>("rotation");

                var filter = Builders<Rotation>.Filter.Eq("status", "A");
                filter &= Builders<Rotation>.Filter.Eq("warningPage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Rotation>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.imageUrl, c.linkUrl, c.description, c.action, c.note, c.newsPage, c.eventPage, c.privilegePage, c.mainPage, c.poiPage, c.notificationPage }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("training/read")]
        public ActionResult<Response> TrainingRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Rotation>("rotation");

                var filter = Builders<Rotation>.Filter.Eq("status", "A");
                filter &= Builders<Rotation>.Filter.Eq("trainingPage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Rotation>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.imageUrl, c.linkUrl, c.description, c.action, c.note, c.newsPage, c.eventPage, c.privilegePage, c.mainPage, c.poiPage, c.notificationPage }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("welfare/read")]
        public ActionResult<Response> WelfareRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Rotation>("rotation");

                var filter = Builders<Rotation>.Filter.Eq("status", "A");
                filter &= Builders<Rotation>.Filter.Eq("welfarPage", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Rotation>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.imageUrl, c.linkUrl, c.description, c.action, c.note, c.newsPage, c.eventPage, c.privilegePage, c.mainPage, c.poiPage, c.notificationPage }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }
    }
}
