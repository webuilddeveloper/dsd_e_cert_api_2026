using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class CooperativeFormController : Controller
    {
        public CooperativeFormController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] CooperativeForm value)
        {

            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("cooperativeForm");

                //check duplicate
                value.code = "".toCode();
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                if (value.organizationMode == "auto")
                {
                    Task.Run(() => { value.organization.createQrganizationAuto(value.code, "cooperativeForm"); });

                    //var og = value.organization.filterQrganizationAuto();
                    //value.lv0 = og.lv0;
                    //value.lv1 = og.lv1;
                    //value.lv2 = og.lv2;
                    //value.lv3 = og.lv3;
                    //value.lv4 = og.lv4;
                    //value.lv5 = og.lv5;
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "title", value.title },
                    { "sequence", value.sequence },
                    { "imageUrl", value.imageUrl },
                    { "category", value.category },
                    { "language", value.language },
                    { "description", value.description},
                    { "author", value.author },
                    { "publisher", value.publisher },
                    { "bookType", value.bookType },
                    { "numberOfPages", value.numberOfPages },
                    { "size", value.size },
                    { "fileUrl", value.fileUrl },
                    { "linkUrl", value.linkUrl},
                    { "textButton", value.textButton},
                    { "publishDate", value.publishDate == "Invalid date" ? "" : value.publishDate },
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
                    { "status", "P" },
                    { "lv0", value.lv0 },
                    { "lv1", value.lv1 },
                    { "lv2", value.lv2 },
                    { "lv3", value.lv3 },
                    { "lv4", value.lv4 },
                    { "lv5", value.lv5 },
                    { "isPublic", value.isPublic },
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
                var col = new Database().MongoClient<CooperativeForm>( "cooperativeForm");
                var filter = (Builders<CooperativeForm>.Filter.Ne("status", "D") & value.filterOrganization<CooperativeForm>());

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<CooperativeForm>.Filter.Regex("title", value.keySearch)) | (filter & Builders<CooperativeForm>.Filter.Regex("description", value.keySearch));

                    if (value.permission != "all")
                        filter &= value.permission.filterPermission<CooperativeForm>("category");

                }
                else
                {
                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<CooperativeForm>.Filter.Eq("category", value.category);
                    else
                        if (value.permission != "all")
                        filter &= value.permission.filterPermission<CooperativeForm>("category");

                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<CooperativeForm>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<CooperativeForm>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<CooperativeForm>.Filter.Eq("createBy", value.createBy); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<CooperativeForm>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<CooperativeForm>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<CooperativeForm>.Filter.Regex("language", value.language); }
                    if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<CooperativeForm>.Filter.Eq("sequence", sequence); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<CooperativeForm>.Filter.Gt("docDate", ds.start) & Builders<CooperativeForm>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<CooperativeForm>.Filter.Gt("docDate", ds.start) & Builders<CooperativeForm>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<CooperativeForm>.Filter.Gt("docDate", de.start) & Builders<CooperativeForm>.Filter.Lt("docDate", de.end); }

                }
                //var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.sequence, c.titleEN, c.description, c.descriptionEN, c.imageUrl, c.fileUrl, c.author, c.publisher, c.bookType, c.numberOfPages, c.size, c.category, c.publishDate, c.createBy, c.createDate, c.isActive, c.updateBy, c.updateDate, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();
                /*c.sequence, c.linkUrl, c.imageUrlPopup, c.action, c.note, c.position, c.newsPage, c.eventPage, c.loginPage, c.mainPage*/
                
                List<CooperativeForm> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                     .Lookup("cooperativeFormCategory", "category", "code", "categoryList")
                                     .As<CooperativeForm>()
                                     .ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] CooperativeForm value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "cooperativeForm");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                if (value.organizationMode == "auto")
                {
                    Task.Run(() => { value.organization.createQrganizationAuto(value.code, "cooperativeForm"); });
                    
                    //var og = value.organization.filterQrganizationAuto();
                    //value.lv0 = og.lv0;
                    //value.lv1 = og.lv1;
                    //value.lv2 = og.lv2;
                    //value.lv3 = og.lv3;
                    //value.lv4 = og.lv4;
                    //value.lv5 = og.lv5;
                }

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.category)) { doc["category"] = value.category; }
                if (!string.IsNullOrEmpty(value.language)) { doc["language"] = value.language; }
                if (!string.IsNullOrEmpty(value.description)) { doc["description"] = value.description; }
                if (!string.IsNullOrEmpty(value.size)) { doc["size"] = value.size; }
                if (!string.IsNullOrEmpty(value.author)) { doc["author"] = value.author; }
                if (!string.IsNullOrEmpty(value.fileUrl)) { doc["fileUrl"] = value.fileUrl; }
                if (!string.IsNullOrEmpty(value.bookType)) { doc["bookType"] = value.bookType; }
                if (!string.IsNullOrEmpty(value.publisher)) { doc["publisher"] = value.publisher; }
                if (!string.IsNullOrEmpty(value.publishDate)) { doc["publishDate"] = value.publishDate; }

                doc["fileUrl"] = value.fileUrl;
                doc["linkUrl"] = value.linkUrl;
                doc["textButton"] = value.textButton;
                doc["numberOfPages"] = value.numberOfPages;
                doc["sequence"] = value.sequence;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["isActive"] = value.isActive;
                doc["status"] = value.organizationMode == "auto" ? "P" : value.isActive ? "A" : "N";
                doc["lv0"] = value.lv0;
                doc["lv1"] = value.lv1;
                doc["lv2"] = value.lv2;
                doc["lv3"] = value.lv3;
                doc["lv4"] = value.lv4;
                doc["lv5"] = value.lv5;
                doc["isPublic"] = value.isPublic;
                col.ReplaceOne(filter, doc);

                // ------- update content ------
                if (!value.isActive)
                {
                    var collectionContent = new Database().MongoClient("cooperativeForm");
                    var filterContent = Builders<BsonDocument>.Filter.Eq("category", value.code);
                    var updateContent = Builders<BsonDocument>.Update.Set("isActive", false).Set("status", "N");
                    collectionContent.UpdateMany(filterContent, updateContent);
                }
                // ------- end ------

                // ------- update register permission ------
                if (!value.isActive)
                {
                    var collectionPermission = new Database().MongoClient("registerPermission");
                    var filterPermission = Builders<BsonDocument>.Filter.Eq("category", value.code);
                    var updatePermission = Builders<BsonDocument>.Update.Set("cooperativeFormPage", false).Set("isActive", false);
                    collectionPermission.UpdateMany(filterPermission, updatePermission);
                }
                // ------- end ------

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] CooperativeForm value)
        {
            try
            {
                var col = new Database().MongoClient( "cooperativeForm");

                var codeList = value.code.Split(",");

                foreach (var code in codeList)
                {

                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                    col.UpdateOne(filter, update);

                    // ------- update content ------
                    if (!value.isActive)
                    {
                        var collectionContent = new Database().MongoClient("cooperativeForm");
                        var filterContent = Builders<BsonDocument>.Filter.Eq("category", value.code);
                        var updateContent = Builders<BsonDocument>.Update.Set("isActive", false).Set("status", "D");
                        collectionContent.UpdateMany(filterContent, updateContent);
                    }
                    // ------- end ------

                    // ------- update register permission ------
                    if (!value.isActive)
                    {
                        var collectionPermission = new Database().MongoClient("registerPermission");
                        var filterPermission = Builders<BsonDocument>.Filter.Eq("category", value.code);
                        var updatePermission = Builders<BsonDocument>.Update.Set("cooperativeFormPage", false).Set("isActive", false);
                        collectionPermission.UpdateMany(filterPermission, updatePermission);
                    }
                    // ------- end ------

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

        // POST /create
        [HttpPost("category/create")]
        public ActionResult<Response> cooperativeFormCategoryCreate([FromBody] Category value)
        {

            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "cooperativeFormCategory");

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
                    { "sequence", value.sequence },
                    { "language", value.language },
                    { "title", value.title },
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
                    { "status", value.isActive ? "A" : "N" }
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
        [HttpPost("category/read")]
        public ActionResult<Response> cooperativeFormCategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Category>("cooperativeFormCategory");

                var filter = Builders<Category>.Filter.Ne("status", "D");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Category>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                    if (value.permission != "all")
                        filter &= value.permission.filterPermission<Category>("code");

                }
                else
                {
                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<Category>.Filter.Eq("title", value.category);
                    else
                        if (value.permission != "all")
                            filter &= value.permission.filterPermission<Category>("code");

                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Category>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Category>.Filter.Regex("language", value.language); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive, c.sequence, c.updateBy, c.updateDate }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /update
        [HttpPost("category/update")]
        public ActionResult<Response> cooperativeFormCategoryUpdate([FromBody] Category value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "cooperativeFormCategory");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.language)) { doc["language"] = value.language; }
                doc["sequence"] = value.sequence;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                doc["status"] = value.isActive ? "A" : "N";
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /delete
        [HttpPost("category/delete")]
        public ActionResult<Response> cooperativeFormCategoryDelete([FromBody] Category value)
        {
            try
            {
                var col = new Database().MongoClient( "cooperativeFormCategory");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
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
