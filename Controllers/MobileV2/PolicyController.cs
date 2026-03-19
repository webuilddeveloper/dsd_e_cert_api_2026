using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Models;
using cms_api.Extension;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace mobilev2_api.Controllers
{
    [Route("m/[controller]")]
    public class PolicyController : Controller
    {

        public PolicyController() { }

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> PolicyCreate([FromBody] Register value)
        {
            try
            {
                var doc = new BsonDocument();
                var col = new Database().MongoClient("registerPolicy");

                var filter = Builders<BsonDocument>.Filter.Eq("username", value.username);
                filter &= Builders<BsonDocument>.Filter.Eq("reference", value.reference);
                filter &= Builders<BsonDocument>.Filter.Eq("isActive", true);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "S", message = "You have accepted this policy." };
                }

                doc = new BsonDocument
                    {
                        { "code", "".toCode() },
                        { "username", value.username },
                        { "reference", value.reference },
                        { "createBy", value.createBy },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", value.updateBy },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", value.isActive },
                        { "status", value.status }
                };

                col.InsertOne(doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                //Get Profile
                var colRegister = new Database().MongoClient<Register>("register");
                var filterRegister = Builders<Register>.Filter.Ne(x => x.status, "D") & Builders<Register>.Filter.Eq("code", value.profileCode);
                var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                value.statisticsCreate("policy");
                var docs = new List<Policy>();
                var col = new Database().MongoClient<Policy>("policy");
                var filter = Builders<Policy>.Filter.Eq("status", "A");
                filter = filter & Builders<Policy>.Filter.Eq("category", "application");

                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Policy>.Filter.Regex("code", value.code); }
                if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Policy>.Filter.Regex("title", value.title); }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Policy>.Filter.Eq("category", value.category); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Policy>.Filter.Gt("docDate", ds.start) & Builders<Policy>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Policy>.Filter.Gt("docDate", ds.start) & Builders<Policy>.Filter.Lt("docDate", ds.end); }

                var docsPolicy = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new Policy
                {
                    code = c.code,
                    imageUrl = c.imageUrl,
                    category = c.category,
                    description = c.description,
                    descriptionEN = c.descriptionEN,
                    title = c.title,
                    createDate = c.createDate,
                    createBy = c.createBy,
                    imageUrlCreateBy = c.imageUrlCreateBy,
                    isRequired = c.isRequired
                }).ToList();
                var codePolicy = new List<PolicyRegister>();

                docsPolicy.ForEach(c =>
                {
                    var colR = new Database().MongoClient<PolicyRegister>("registerPolicy");
                    var filterR = Builders<PolicyRegister>.Filter.Eq("username", value.username);
                    filterR &= Builders<PolicyRegister>.Filter.Eq("reference", c.code);

                    var code = colR.Find(filterR).FirstOrDefault();
                    if (code == null)
                        docs.Add(c);
                });


                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

    }
}