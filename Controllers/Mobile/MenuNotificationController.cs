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
    [Route("m/[controller]")]
    public class MenuNotificationController : Controller
    {
        public MenuNotificationController() { }

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] MenuNotification value)
        {
            value.code = value.code.toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "menuNotification");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "username", value.username },
                    { "title", value.title },
                    { "createBy", value.createBy },
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

                var col = new Database().MongoClient<MenuNotification>( "menuNotification");

                var filter = (Builders<MenuNotification>.Filter.Eq(x => x.isActive, true) | Builders<MenuNotification>.Filter.Eq(x => x.isActive, false));

                if (!string.IsNullOrEmpty(value.username)) { filter = filter & Builders<MenuNotification>.Filter.Regex("username", value.username); }

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).SortByDescending(o => o.createDate).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.title, c.username, c.updateBy, c.updateDate, c.createTime, c.updateTime, c.docDate, c.docTime }).ToList();

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
        public ActionResult<Response> Update([FromBody] MenuNotification value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "menuNotification");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                doc["isActive"] = value.isActive;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success", jsonData = model.ToJson(), objectData = model };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] MenuNotification value)
        {
            try
            {
                var col = new Database().MongoClient( "menuNotification");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                col.UpdateOne(filter, update);
                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

    }
}