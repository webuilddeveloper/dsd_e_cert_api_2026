using System;
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
    public class MenuController : Controller
    {
        public MenuController() { }

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
                    { "category", value.category },
                    { "title", value.title },
                    { "titleEN", value.titleEN },
                    { "direction", value.direction },
                    { "imageUrl", value.imageUrl },
                    { "createBy", value.createBy },
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
                var col = new Database().MongoClient<Menu>( "menu");

                var filter = Builders<Menu>.Filter.Eq("isActive", true);

                if (!string.IsNullOrEmpty(value.code))
                {
                    filter &= Builders<Menu>.Filter.Eq("code", value.code);
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).Project(c => new { c.code, c.category, c.title, c.titleEN, c.direction, c.imageUrl, c.createBy, c.createDate, c.updateBy, c.updateDate, c.isActive, c.action }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }
    }
}