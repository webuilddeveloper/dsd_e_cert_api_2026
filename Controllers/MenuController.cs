using System;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class MenuController : Controller
    {
        public MenuController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Menu value)
        {
            value.code = value.code.toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "menu");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "sequence", value.sequence },
                    { "category", value.category },
                    { "title", value.title },
                    { "titleEN", value.titleEN },
                    { "direction", value.direction },
                    { "imageUrl", value.imageUrl },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                    { "action", value.action }
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
                var col = new Database().MongoClient<Menu>("menu");

                var filter = Builders<Menu>.Filter.Ne("status", "D");

                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Menu>.Filter.Eq("code", value.code); }

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Menu>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Menu>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));


                    //var permissionFilter = Builders<Menu>.Filter.Ne("status", "D");
                    //var permission = value.permission.Split(",");
                    //for (int i = 0; i < permission.Length; i++)
                    //{
                    //    if (i == 0)
                    //        permissionFilter = Builders<Menu>.Filter.Eq("category", permission[i]);
                    //    else
                    //        permissionFilter |= Builders<Menu>.Filter.Eq("category", permission[i]);
                    //}

                    //filter &= (permissionFilter);

                }
                else
                {

                    if (!string.IsNullOrEmpty(value.category))
                    {
                        filter = filter & Builders<Menu>.Filter.Regex("category", value.category);
                    }
                    else
                    {
                        //var permissionFilter = Builders<Menu>.Filter.Ne("status", "D");
                        //var permission = value.permission.Split(",");
                        //for (int i = 0; i < permission.Length; i++)
                        //{
                        //    if (i == 0)
                        //        permissionFilter = Builders<Menu>.Filter.Eq("category", permission[i]);
                        //    else
                        //        permissionFilter |= Builders<Menu>.Filter.Eq("category", permission[i]);
                        //}

                        //filter &= (permissionFilter);

                    }


                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Menu>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<Menu>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Menu>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Menu>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i"));
                        if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<Menu>.Filter.Eq("sequence", sequence); }
                    }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Menu>.Filter.Gt("docDate", ds.start) & Builders<Menu>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Menu>.Filter.Gt("docDate", ds.start) & Builders<Menu>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Menu>.Filter.Gt("docDate", de.start) & Builders<Menu>.Filter.Lt("docDate", de.end); }

                }

                if (!string.IsNullOrEmpty(value.code))
                {
                    filter &= Builders<Menu>.Filter.Eq("code", value.code);
                }

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.sequence, c.category, c.title, c.titleEN, c.direction, c.imageUrl, c.createBy, c.createDate, c.updateBy, c.updateDate, c.isActive, c.action }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] Menu value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "menu");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.titleEN)) { doc["titleEN"] = value.titleEN; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.category)) { doc["category"] = value.category; }
                if (!string.IsNullOrEmpty(value.direction)) { doc["direction"] = value.direction; }
                if (!string.IsNullOrEmpty(value.action)) { doc["action"] = value.action; }

                doc["sequence"] = value.sequence;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] Menu value)
        {
            try
            {
                var col = new Database().MongoClient( "menu");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
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