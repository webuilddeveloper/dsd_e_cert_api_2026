using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace master_api.Controllers
{
    [Route("[controller]")]
    public class ZoneController : Controller
    {
        public ZoneController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] News value)
        {
            
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("news");

                //check duplicate
                value.code = "".toCode();
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                if (value.organizationMode == "auto")
                {
                    var og = value.organization.filterQrganizationAuto();
                    value.lv0 = og.lv0;
                    value.lv1 = og.lv1;
                    value.lv2 = og.lv2;
                    value.lv3 = og.lv3;
                    value.lv4 = og.lv4;
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "sequence", value.sequence },
                    { "title", value.title },
                    { "titleEN", value.titleEN },
                    { "imageUrl", value.imageUrl },
                    { "category", value.category },
                    { "language", value.language },
                    { "description", value.description },
                    { "descriptionEN", value.descriptionEN },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.createBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", value.isActive },
                    { "status", value.isActive ? "A" : "N" },
                    { "lv0", value.lv0 },
                    { "lv1", value.lv1 },
                    { "lv2", value.lv2 },
                    { "lv3", value.lv3 }
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {

                var col = new Database().MongoClient<News>( "news");
                var filter = (Builders<News>.Filter.Ne("status", "D") & value.filterOrganization<News>());

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<News>.Filter.Regex("title", value.keySearch)) | (filter & Builders<News>.Filter.Regex("description", value.keySearch));

                    if (value.permission != "all")
                        filter &= (value.permission.filterPermission<News>("category"));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<News>.Filter.Eq("category", value.category);
                    else
                        if (value.permission != "all")
                            filter &= (value.permission.filterPermission<News>("category"));

                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<News>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<News>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<News>.Filter.Regex("title", value.title); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<News>.Filter.Regex("description", value.description); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<News>.Filter.Regex("language", value.language); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<News>.Filter.Gt("docDate", ds.start) & Builders<News>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<News>.Filter.Gt("docDate", ds.start) & Builders<News>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<News>.Filter.Gt("docDate", de.start) & Builders<News>.Filter.Lt("docDate", de.end); }
                    
                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] News value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "news");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                if (value.organizationMode == "auto")
                {
                    var og = value.organization.filterQrganizationAuto();
                    value.lv0 = og.lv0;
                    value.lv1 = og.lv1;
                    value.lv2 = og.lv2;
                    value.lv3 = og.lv3;
                    value.lv4 = og.lv4;
                }

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.category)) { doc["category"] = value.category; }
                if (!string.IsNullOrEmpty(value.language)) { doc["language"] = value.language; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.description)) { doc["description"] = value.description; }

                doc["sequence"] = value.sequence;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                doc["status"] = value.isActive ? "A" : "N";
                doc["lv0"] = value.lv0;
                doc["lv1"] = value.lv1;
                doc["lv2"] = value.lv2;
                doc["lv3"] = value.lv3;

                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] News value)
        {
            try
            {
                var col = new Database().MongoClient( "news");
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                col.UpdateOne(filter, update);
                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

    }
}