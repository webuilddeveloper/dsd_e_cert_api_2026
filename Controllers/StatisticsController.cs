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
    public class StatisticsController : Controller
    {
        public StatisticsController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Statistics value)
        {

            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("statistics");

                doc = new BsonDocument
                {
                    { "code", "".toCode() },
                    { "category", value.category },

                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
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

                var col = new Database().MongoClient<Statistics>("statistics");
                var filter = Builders<Statistics>.Filter.Eq("isActive", true);

                if (!string.IsNullOrEmpty(value.category)) { filter &= Builders<Statistics>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Statistics>.Filter.Eq("code", value.code); }
                if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Statistics>.Filter.Eq("createBy", value.createBy); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Statistics>.Filter.Gt("docDate", ds.start) & Builders<Statistics>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Statistics>.Filter.Gt("docDate", ds.start) & Builders<Statistics>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Statistics>.Filter.Gt("docDate", de.start) & Builders<Statistics>.Filter.Lt("docDate", de.end); }

                List<Statistics> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

    }
}