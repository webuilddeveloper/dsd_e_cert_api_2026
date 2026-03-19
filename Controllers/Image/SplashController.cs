using System;
using System.Collections.Generic;
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
    public class SplashController : Controller
    {
        public SplashController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Splash value)
        {
            
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient( "splash");

                //check duplicate
                value.code = "".toCode();
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }




                //BEGIN :check active >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                if (value.isActive)
                {
                    var filterActive = Builders<BsonDocument>.Filter.Eq("isActive", true);
                    var update = Builders<BsonDocument>.Update.Set("isActive", false);
                    col.UpdateMany(filterActive, update);
                }

                //END :check active <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<





                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "imageUrl", value.imageUrl },
                    { "sequence", 1 },
                    { "timeOut", value.timeOut},
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
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
                var col = new Database().MongoClient<Splash>( "splash");

                var filter = Builders<Splash>.Filter.Ne("status", "D");
                //filter = filter | Builders<Splash>.Filter.Eq("isActive", false);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Splash>.Filter.Eq("code", value.code); }
                if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<Splash>.Filter.Eq("sequence", sequence); }

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Project(c => new { c.code, c.isActive, c.imageUrl, c.sequence, c.timeOut, c.createBy,c.createDate, c.updateBy, c.updateDate }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
            
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] Splash value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "splash");




                //BEGIN :check active >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                if (value.isActive)
                {
                    var filterActive = Builders<BsonDocument>.Filter.Eq("isActive", true);
                    var update = Builders<BsonDocument>.Update.Set("isActive", false);
                    col.UpdateMany(filterActive, update);
                }

                //END :check active <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<





                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                //if (!string.IsNullOrEmpty(value.sequence)) { doc["sequence"] = value.sequence; }
                if (!string.IsNullOrEmpty(value.timeOut)) { doc["timeOut"] = value.timeOut; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
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
        public ActionResult<Response> Delete([FromBody] Splash value)
        {
            try
            {
                var col = new Database().MongoClient( "splash");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
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