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
    public class AboutUsController : Controller
    {
        public AboutUsController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] AboutUs value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("aboutUs");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", "1" },
                    { "title", value.title },
                    { "imageBgUrl", value.imageBgUrl },
                    { "imageLogoUrl", value.imageLogoUrl },
                    { "description", value.description },
                    { "latitude", value.latitude},
                    { "longitude", value.longitude},
                    { "address", value.address},
                    { "telephone", value.telephone},
                    { "email", value.email},
                    { "site", value.site},
                    { "facebook", value.facebook},
                    { "youtube", value.youtube},
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                    { "lineOfficial", value.lineOfficial }
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
                var col = new Database().MongoClient<AboutUs>("aboutUs");
                var filter = Builders<AboutUs>.Filter.Eq("code", "1");
                var docs = col.Find(filter).Project(c => new { c.code, c.isActive, c.title, c.imageLogoUrl, c.imageBgUrl, c.description, c.latitude, c.email, c.site, c.longitude, c.address, c.facebook, c.youtube, c.telephone, c.createBy, c.createDate, c.updateBy, c.updateDate, c.lineOfficial }).ToList();
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] AboutUs value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("aboutUs");

                if (!string.IsNullOrEmpty(value.code))
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("code", "1");
                    doc = col.Find(filter).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                    if (!string.IsNullOrEmpty(value.imageBgUrl)) { doc["imageBgUrl"] = value.imageBgUrl; }
                    if (!string.IsNullOrEmpty(value.imageLogoUrl)) { doc["imageLogoUrl"] = value.imageLogoUrl; }
                    if (!string.IsNullOrEmpty(value.description)) { doc["description"] = value.description; }
                    if (!string.IsNullOrEmpty(value.latitude)) { doc["latitude"] = value.latitude; }
                    if (!string.IsNullOrEmpty(value.longitude)) { doc["longitude"] = value.longitude; }
                    if (!string.IsNullOrEmpty(value.address)) { doc["address"] = value.address; }
                    if (!string.IsNullOrEmpty(value.telephone)) { doc["telephone"] = value.telephone; }
                    if (!string.IsNullOrEmpty(value.email)) { doc["email"] = value.email; }
                    if (!string.IsNullOrEmpty(value.facebook)) { doc["facebook"] = value.facebook; }
                    if (!string.IsNullOrEmpty(value.youtube)) { doc["youtube"] = value.youtube; }
                    if (!string.IsNullOrEmpty(value.site)) { doc["site"] = value.site; }
                    if (!string.IsNullOrEmpty(value.updateBy)) { doc["updateBy"] = value.updateBy; }
                    if (!string.IsNullOrEmpty(value.lineOfficial)) { doc["lineOfficial"] = value.lineOfficial; }
                    doc["updateDate"] = DateTime.Now.toStringFromDate();
                    doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                    col.ReplaceOne(filter, doc);
                }
                else
                {
                    doc = new BsonDocument
                    {
                        { "code", "1" },
                        { "title", value.title },
                        { "imageLogoUrl", value.imageLogoUrl },
                        { "imageBgUrl", value.imageBgUrl },
                        { "description", value.description },
                        { "latitude", value.latitude},
                        { "longitude", value.longitude},
                        { "address", value.address},
                        { "telephone", value.telephone},
                        { "email", value.email },
                        { "site", value.site },
                        { "facebook", value.facebook },
                        { "youtube", value.youtube },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "docDate", DateTime.Now },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true },
                        { "lineOfficial", value.lineOfficial }
                    };
                    col.InsertOne(doc);
                }

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] AboutUs value)
        {
            try
            {
                var col = new Database().MongoClient("aboutUs");
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