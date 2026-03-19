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
    public class UserController : Controller
    {
        public UserController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] User value)
        {
            
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient( "user");

                //check duplicate
                value.code = "".toCode();
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "username", value.username },
                    { "password", value.password },
                    { "prefixName", value.prefixName },
                    { "firstName", value.firstName },
                    { "lastName", value.lastName },
                    { "imageUrl", value.imageUrl},
                    { "position", value.position },
                    { "level", value.level },
                    { "expirationDate", value.expirationDate },
                    { "birthDay", value.birthDay },
                    { "phone", value.phone },
                    { "email", value.email },
                    //{ "facebookID", value.facebookID },
                    //{ "googleID", value.googleID },
                    //{ "lineID", value.lineID },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "docDate", DateTime.Now },
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
                var col = new Database().MongoClient<User>( "user");

                var filter = Builders<User>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<User>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).SortByDescending(o => o.docDate).Project(c => new {
                    c.code,
                    c.imageUrl,
                    c.username,
                    c.password,
                    c.prefixName,
                    c.firstName,
                    c.lastName,
                    c.position,
                    c.level,
                    c.expirationDate,
                    c.phone,
                    c.email,
                    c.birthDay,
                    c.createBy,
                    c.createDate,
                    c.updateBy,
                    c.updateDate,
                    c.docDate,
                    c.docTime,
                    c.isActive
                }).ToList();

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
        public ActionResult<Response> Update([FromBody] User value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "user");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                //if (!string.IsNullOrEmpty(value.username)) { doc["username"] = value.username; }
                if (!string.IsNullOrEmpty(value.username)) { doc["password"] = value.password; }
                if (!string.IsNullOrEmpty(value.username)) { doc["prefixName"] = value.prefixName; }
                if (!string.IsNullOrEmpty(value.firstName)) { doc["firstName"] = value.firstName; }
                if (!string.IsNullOrEmpty(value.lastName)) { doc["lastName"] = value.lastName; }
                if (!string.IsNullOrEmpty(value.phone)) { doc["phone"] = value.phone; }
                if (!string.IsNullOrEmpty(value.email)) { doc["email"] = value.email; }
                if (!string.IsNullOrEmpty(value.birthDay)) { doc["birthDay"] = value.birthDay; }
                if (!string.IsNullOrEmpty(value.firstName)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.position)) { doc["position"] = value.position; }
                if (!string.IsNullOrEmpty(value.level)) { doc["level"] = value.level; }
                if (!string.IsNullOrEmpty(value.expirationDate)) { doc["expirationDate"] = value.expirationDate; }
                if (!string.IsNullOrEmpty(value.updateBy)) { doc["updateBy"] = value.updateBy; }
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] User value)
        {
            try
            {
                var col = new Database().MongoClient( "user");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toTimeStringFromDate());
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