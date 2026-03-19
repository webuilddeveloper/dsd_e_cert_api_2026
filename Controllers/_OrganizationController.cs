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
    public class OrganizationController : Controller
    {
        public OrganizationController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] News value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("organization");

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
                    { "title", value.title },
                    { "titleEN", value.titleEN },
                    { "category", value.category },
                    { "sequence", value.sequence },
                    { "titleShort", value.titleShort },
                    { "codeShort", value.codeShort },
                    { "imageUrl", value.imageUrl },
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
                    { "lv5", value.lv5 }
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
                var col = new Database().MongoClient<News>("organization");

                var filter = Builders<News>.Filter.Ne("status", "D");

                if (!string.IsNullOrEmpty(value.title)) { filter &= (Builders<News>.Filter.Regex("title", value.title) | Builders<News>.Filter.Regex("titleEN", value.title)); }
                if (!string.IsNullOrEmpty(value.category)) { filter &= Builders<News>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<News>.Filter.Eq("code", value.code); }

                if (value.category == "lv1")
                {
                    List<News> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                          .Lookup("organization", "lv0", "code", "lv0List")
                                          .As<News>()
                                          .ToList();

                    return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
                }
                else if (value.category == "lv2")
                {
                    List<News> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                          .Lookup("organization", "lv0", "code", "lv0List")
                                          .Lookup("organization", "lv1", "code", "lv1List")
                                          .As<News>()
                                          .ToList();

                    return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
                }
                else if (value.category == "lv3")
                {
                    List<News> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                          .Lookup("organization", "lv0", "code", "lv0List")
                                          .Lookup("organization", "lv1", "code", "lv1List")
                                          .Lookup("organization", "lv2", "code", "lv2List")
                                          .As<News>()
                                          .ToList();

                    return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
                }
                else if (value.category == "lv4")
                {
                    List<News> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                          .Lookup("organization", "lv0", "code", "lv0List")
                                          .Lookup("organization", "lv1", "code", "lv1List")
                                          .Lookup("organization", "lv2", "code", "lv2List")
                                          .Lookup("organization", "lv3", "code", "lv3List")
                                          .As<News>()
                                          .ToList();

                    return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
                }
                else if (value.category == "lv5")
                {
                    List<News> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                          .Lookup("organization", "lv0", "code", "lv0List")
                                          .Lookup("organization", "lv1", "code", "lv1List")
                                          .Lookup("organization", "lv2", "code", "lv2List")
                                          .Lookup("organization", "lv3", "code", "lv3List")
                                          .Lookup("organization", "lv4", "code", "lv4List")
                                          .As<News>()
                                          .ToList();

                    return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
                }
                else
                {
                    //var docs = col.Find(filter).Project(c => new { c.code, c.title, c.titleEN, c.category, c.createBy, c.createDate, c.updateBy, c.updateDate, c.isActive }).ToList();
                    List<News> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                          .As<News>()
                                          .ToList();

                    return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
                }
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] News value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("organization");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.titleEN)) { doc["titleEN"] = value.titleEN; }

                doc["sequence"] = value.sequence;
                doc["titleShort"] = value.titleShort;
                doc["codeShort"] = value.codeShort;

                doc["imageUrl"] = value.imageUrl;
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
        public ActionResult<Response> Delete([FromBody] Identity value)
        {
            try
            {
                var col = new Database().MongoClient("organization");

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

        #region category

        // POST /read
        [HttpPost("category/read")]
        public ActionResult<Response> CategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<News>("organization");

                var filter = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("isActive", true);

                if (!string.IsNullOrEmpty(value.keySearch)) { filter &= Builders<News>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter &= Builders<News>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<News>.Filter.Eq("code", value.code); }
                if (!string.IsNullOrEmpty(value.lv0)) { filter &= Builders<News>.Filter.Eq("lv0", value.lv0); }
                if (!string.IsNullOrEmpty(value.lv1)) { filter &= Builders<News>.Filter.Eq("lv1", value.lv1); }
                if (!string.IsNullOrEmpty(value.lv2)) { filter &= Builders<News>.Filter.Eq("lv2", value.lv2); }
                if (!string.IsNullOrEmpty(value.lv3)) { filter &= Builders<News>.Filter.Eq("lv3", value.lv3); }
                if (!string.IsNullOrEmpty(value.lv4)) { filter &= Builders<News>.Filter.Eq("lv4", value.lv4); }
                if (!string.IsNullOrEmpty(value.lv5)) { filter &= Builders<News>.Filter.Eq("lv5", value.lv5); }

                //var docs = col.Find(filter).Project(c => new { c.code, c.title, c.titleEN, c.category, c.createBy, c.createDate, c.updateBy, c.updateDate, c.isActive }).ToList();
                List<News> docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenBy(o => o.title).Skip(value.skip).Limit(value.limit)
                                      //.Lookup("newsCategory", "category", "code", "categoryList")
                                      .As<News>()
                                      .ToList();

                if (value.category == "lv0")
                {
                    docs.ForEach(c =>
                    {
                        if (col.Find(Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("category", "lv5") & Builders<News>.Filter.Eq("lv0", c.code)).Any())
                            c.totalLv = 5;
                        else if (col.Find(Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("category", "lv4") & Builders<News>.Filter.Eq("lv0", c.code)).Any())
                            c.totalLv = 4;
                        else if (col.Find(Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("category", "lv3") & Builders<News>.Filter.Eq("lv0", c.code)).Any())
                            c.totalLv = 3;
                        else if (col.Find(Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("category", "lv2") & Builders<News>.Filter.Eq("lv0", c.code)).Any())
                            c.totalLv = 2;
                        else if (col.Find(Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("category", "lv1") & Builders<News>.Filter.Eq("lv0", c.code)).Any())
                            c.totalLv = 1;
                    });
                }

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