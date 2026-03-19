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
    public class PolicyController : Controller
    {
        public PolicyController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Policy value)
        {

            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("policy");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("status", "D");
                    col.UpdateOne(filter, update);

                    var colP = new Database().MongoClient<PolicyRegister>("registerPolicy");
                    var filterRegisterPolicy = Builders<PolicyRegister>.Filter.Eq("reference", value.code);
                    var updateRegisterPolicy = Builders<PolicyRegister>.Update.Set("isActive", false).Set("status", "D");
                    colP.UpdateMany(filterRegisterPolicy, updateRegisterPolicy);
                }

                if (value.byPass) { value.lv0 = ""; value.lv1 = ""; value.lv2 = ""; value.lv3 = ""; }

                doc = new BsonDocument
                {
                    { "code", "".toCode() },
                    { "sequence", value.sequence },
                    { "title", value.title },
                    { "imageUrl", value.imageUrl },
                    { "category", value.category },
                    { "language", value.language },
                    { "isRequired", value.isRequired },
                    { "description", value.description },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.updateBy },
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
                    { "lv3", value.lv3 },
                    { "lv4", value.lv4 },
                    { "lv5", value.lv5 },
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

                var col = new Database().MongoClient<Policy>("policy");
                //var filter = (Builders<Policy>.Filter.Ne("status", "D") & value.filterOrganization<Policy>());
                var filter = (Builders<Policy>.Filter.Ne("status", "D"));

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Policy>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Policy>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                    //if (value.permission != "all")
                    //    filter &= (value.permission.filterPermission<Policy>("category"));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<Policy>.Filter.Eq("category", value.category);
                    //else
                    //    if (value.permission != "all")
                    //    filter &= (value.permission.filterPermission<Policy>("category"));

                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Policy>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<Policy>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Policy>.Filter.Eq("category", value.category); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Policy>.Filter.Eq("createBy", value.createBy); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Policy>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Policy>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Policy>.Filter.Regex("language", value.language); }
                    if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<Policy>.Filter.Eq("sequence", sequence); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Policy>.Filter.Gt("docDate", ds.start) & Builders<Policy>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Policy>.Filter.Gt("docDate", ds.start) & Builders<Policy>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Policy>.Filter.Gt("docDate", de.start) & Builders<Policy>.Filter.Lt("docDate", de.end); }

                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.isRequired }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] Policy value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("policy");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                if (value.byPass) { value.lv0 = ""; value.lv1 = ""; value.lv2 = ""; value.lv3 = ""; }

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.category)) { doc["category"] = value.category; }
                if (!string.IsNullOrEmpty(value.language)) { doc["language"] = value.language; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.description)) { doc["description"] = value.description; }

                doc["isRequired"] = value.isRequired;
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
                doc["lv4"] = value.lv4;
                doc["lv5"] = value.lv5;

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
        public ActionResult<Response> Delete([FromBody] Policy value)
        {
            try
            {
                var col = new Database().MongoClient("policy");

                var codeList = value.code.Split(",");

                foreach (var code in codeList)
                {

                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                    col.UpdateOne(filter, update);

                }

                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        // POST /read
        [HttpPost("register/read")]
        public ActionResult<Response> RegisterRead([FromBody] Criteria value)
        {
            try
            {
                var docs = new List<PolicyRegister>();
                var col = new Database().MongoClient<PolicyRegister>("registerPolicy");
                var filter = (Builders<PolicyRegister>.Filter.Eq("isActive", true));
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<PolicyRegister>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                    if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<PolicyRegister>.Filter.Eq("category", value.category); }
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<PolicyRegister>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.username)) { filter = filter & Builders<PolicyRegister>.Filter.Eq("username", value.username); }
                    if (!string.IsNullOrEmpty(value.reference)) { filter = filter & Builders<PolicyRegister>.Filter.Eq("reference", value.reference); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<PolicyRegister>.Filter.Eq("createBy", value.createBy); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<PolicyRegister>.Filter.Eq("status", value.status); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<PolicyRegister>.Filter.Gt("docDate", ds.start) & Builders<PolicyRegister>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<PolicyRegister>.Filter.Gt("docDate", ds.start) & Builders<PolicyRegister>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<PolicyRegister>.Filter.Gt("docDate", de.start) & Builders<PolicyRegister>.Filter.Lt("docDate", de.end); }

                }

                //var docsss = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.updateBy, c.updateDate, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status }).ToList();

                var rawDocs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime)
                                      .Lookup("register", "username", "username", "registerList")
                                      .Lookup("policy", "reference", "code", "policyList")
                                      .As<PolicyRegister>()
                                      .ToList();

                rawDocs.ForEach(c =>
                {
                    if(c.policyList.Count != 0)
                        if (c.policyList[0].category == value.category)
                        {
                            docs.Add(c);
                        }
                });

                var total = docs.Count();

                docs = docs.Skip(value.skip).Take(value.limit).ToList();

                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = total };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

    }
}