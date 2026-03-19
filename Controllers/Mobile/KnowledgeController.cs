using System;
using System.Collections.Generic;
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
    public class KnowledgeController : Controller
    {
        public KnowledgeController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                value.statisticsCreate("knowledge");
                var col = new Database().MongoClient<Knowledge>( "knowledge");
                var filter = (Builders<Knowledge>.Filter.Eq("status", "A") & value.filterOrganization<Knowledge>());

                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Knowledge>.Filter.Regex("code", value.code); }
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = filter & Builders<Knowledge>.Filter.Regex("title", value.keySearch);

                    //BEGIN : Statistic
                    try
                    {
                        var value1 = new Criteria();

                        value1.title = value.keySearch;
                        value1.updateBy = value.updateBy;

                        if (!string.IsNullOrEmpty(value.code))
                            value1.reference = value.code;

                        value1.statisticsCreate("knowledgeKeySearch");
                    }
                    catch { }
                    //END : Statistic
                }
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Knowledge>.Filter.Regex("code", value.code); }
                if (!string.IsNullOrEmpty(value.keySearch)) { filter = filter & Builders<Knowledge>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Knowledge>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<Knowledge>.Filter.Eq("status", value.status); }
                if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Knowledge>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Knowledge>.Filter.Regex("language", value.language); }
                if (value.isHighlight) { filter = filter & Builders<Knowledge>.Filter.Eq("isHighlight", value.isHighlight); }
                if (value.isPublic) { filter = filter & Builders<Knowledge>.Filter.Eq("isPublic", value.isPublic); }
                //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<POI>.Filter.Regex("dateStart", value.startDate); }
                //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<POI>.Filter.Regex("dateEnd", value.endDate); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Knowledge>.Filter.Gt("docDate", ds.start) & Builders<Knowledge>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Knowledge>.Filter.Gt("docDate", ds.start) & Builders<Knowledge>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Knowledge>.Filter.Gt("docDate", de.start) & Builders<Knowledge>.Filter.Lt("docDate", de.end); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                    .Lookup("knowledgeCategory", "category", "code", "categoryList")
                                          .Lookup("register", "createBy", "username", "userList")
                                    .As<Knowledge>()
                                    .ToList();
                //var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.sequence, c.description, c.imageUrl, c.fileUrl, c.author, c.publisher, c.bookType, c.numberOfPages, c.size, c.category, c.publishDate, c.createBy, c.createDate, c.isActive, c.updateBy, c.updateDate}).ToList();
                /*c.sequence, c.linkUrl, c.imageUrlPopup, c.action, c.note, c.position, c.newsPage, c.eventPage, c.loginPage, c.mainPage*/

                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient("knowledge");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                          .Lookup("knowledgeCategory", "category", "code", "categoryList")
                                          .Lookup("register", "createBy", "username", "userList")
                                          .As<Knowledge>()
                                          .ToList();

                    //BEGIN : Statistic
                    try
                    {
                        if (!string.IsNullOrEmpty(value.code))
                        {
                            value.reference = value.code;
                            value.title = docs.Count > 0 ? docs[0].title : "";
                            value.category = docs.Count > 0 ? docs[0].categoryList[0].title : "";
                        }

                        value.statisticsCreate("knowledge");
                    }
                    catch { }
                    //END : Statistic

                    //docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();
                }
                //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
            
        }

        // POST /read
        [HttpPost("category/read")]
        public ActionResult<Response> CategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Category>( "knowledgeCategory");

                var filter = Builders<Category>.Filter.Eq(x => x.status, "A");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Category>.Filter.Regex("title", value.keySearch)) | (filter & Builders<Category>.Filter.Regex("description", value.keySearch));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Category>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Category>.Filter.Regex("title", value.title); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Category>.Filter.Regex("description", value.description); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Category>.Filter.Regex("language", value.language); }
                    //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<knowledgeCategory>.Filter.Regex("dateStart", value.startDate); }
                    //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<knowledgeCategory>.Filter.Regex("dateEnd", value.endDate); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive }).ToList();

                //BEGIN : Statistic
                try
                {
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        value.reference = value.code;
                        value.title = docs.Count > 0 ? docs[0].title : "";
                    }

                    value.statisticsCreate("knowledgeCategory");
                }
                catch { }
                //END : Statistic

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

    }
}
