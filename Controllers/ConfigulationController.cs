using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class ConfigulationController : Controller
    {
        public ConfigulationController() { }

        #region main

        // POST /create
        [HttpPost("shared/read")]
        public ActionResult<Response> SharedRead([FromBody] Register value)
        {
            try
            {

                var doc = new BsonDocument();
                var newPassword = "".getRandom();
                var filterConfig = Builders<BsonDocument>.Filter.Eq("title", "shared");
                var colConfig = new Database().MongoClient("configulation");
                var docConfig = colConfig.Find(filterConfig).FirstOrDefault();

                return new Response { status = "S", message = "success", jsonData = "", objectData = BsonSerializer.Deserialize<object>(docConfig) };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("email/read")]
        public ActionResult<Response> MailRead([FromBody] Register value)
        {
            try
            {

                var doc = new BsonDocument();
                var newPassword = "".getRandom();
                var col = new Database().MongoClient("register");
                var filterConfig = Builders<BsonDocument>.Filter.Eq("title", "email");

                var colConfig = new Database().MongoClient("configulation");

                var docConfig = colConfig.Find(filterConfig).FirstOrDefault();

                return new Response { status = "s", message = "success", jsonData = docConfig.ToJson(), objectData = BsonSerializer.Deserialize<object>(docConfig) };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("email/create")]
        public ActionResult<Response> ForgetReadAsync([FromBody] Register value)
        {
            try
            {

                var doc = new BsonDocument();
                var col = new Database().MongoClient("configulation");

                var filter = Builders<BsonDocument>.Filter.Eq("title", "email");
                if (col.Find(filter).Any())
                {
                    doc = col.Find(filter).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                    if (!string.IsNullOrEmpty(value.username)) { doc["username"] = value.username; }
                    if (!string.IsNullOrEmpty(value.email)) { doc["email"] = value.email; }
                    if (!string.IsNullOrEmpty(value.password)) { doc["password"] = value.password; }
                    doc["updateBy"] = value.updateBy;
                    doc["updateDate"] = DateTime.Now.toStringFromDate();
                    doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                    doc["isActive"] = value.isActive;
                    col.ReplaceOne(filter, doc);
                }
                else
                {
                    doc = new BsonDocument
                    {
                    { "code", "".toCode() },
                    { "title", value.title },
                    { "username", value.username },
                    { "email", value.email },
                    { "password", value.password },
                    { "createBy", value.createBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", value.isActive }
                    };
                    col.InsertOne(doc);
                }

                return new Response { status = "s", message = "success", jsonData = value.ToJson(), objectData = value };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpGet("initialEmail")]
        public ActionResult<Response> InitialEmail()
        {
            try
            {
                {
                    var doc = new BsonDocument();
                    var col = new Database().MongoClient("configulation");
                    doc = new BsonDocument
                        {
                        { "code", "".toCode() },
                        { "title", "email" },
                        { "username", "webuild" },
                        { "email", "ext18979@gmail.com" },
                        { "password", "EX74108520" },
                        { "description", "" },
                        { "createBy", "system" },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", "system" },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true }
                        };
                    col.InsertOne(doc);
                }

                {
                    var doc = new BsonDocument();
                    var col = new Database().MongoClient("configulation");
                    doc = new BsonDocument
                        {
                        { "code", "".toCode() },
                        { "title", "shared" },
                        { "username", "" },
                        { "email", "" },
                        { "password", "" },
                        { "description", "http://shared.we-builds.com/opec/" },
                        { "createBy", "system" },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", "system" },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true }
                        };
                    col.InsertOne(doc);
                }



                return new Response { status = "s", message = "success", jsonData = "", objectData = new { } };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

    }

    #endregion
}
