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
    public class ComingSoonController : Controller
    {
        public ComingSoonController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] News value)
        {
            
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "comingSoon");

                //check duplicate
                value.code = "".toCode();
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                if (value.organizationMode == "auto")
                {
                    var og = value.organization.filterQrganizationAuto();
                    value.lv0 = og.lv0;
                    value.lv1 = og.lv1;
                    value.lv2 = og.lv2;
                    value.lv3 = og.lv3;
                    value.lv4 = og.lv4;
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "sequence", value.sequence },
                    { "codeShort", value.codeShort },
                    { "title", value.title },
                    { "imageUrl", value.imageUrl },
                    { "category", value.category },
                    { "language", value.language },
                    { "description", value.description },
                    { "fileUrl", value.fileUrl},
                    { "linkUrl", value.linkUrl},
                    { "textButton", value.textButton},
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
                    { "isPublic", value.isPublic },
                    { "isHighlight", value.isHighlight },
                    { "status", value.isActive ? "A" : "N" },
                    { "lv0", value.lv0 },
                    { "lv1", value.lv1 },
                    { "lv2", value.lv2 },
                    { "lv3", value.lv3 },
                    { "lv4", value.lv4 }
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

                var col = new Database().MongoClient<News>( "comingSoon");
                var filter = (Builders<News>.Filter.Ne("status", "D"));

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<News>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<News>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                    if (value.permission != "all")
                        filter &= (value.permission.filterPermission<News>("category"));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<News>.Filter.Eq("category", value.category);
                    else
                        if (value.permission != "all")
                        filter &= (value.permission.filterPermission<News>("category"));

                    if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<News>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.codeShort)) { filter &= Builders<News>.Filter.Eq("codeShort", value.codeShort ); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<News>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<News>.Filter.Eq("createBy", value.createBy); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<News>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<News>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<News>.Filter.Regex("language", value.language); }
                    if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<News>.Filter.Eq("sequence", sequence); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<News>.Filter.Gt("docDate", ds.start) & Builders<News>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<News>.Filter.Gt("docDate", ds.start) & Builders<News>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<News>.Filter.Gt("docDate", de.start) & Builders<News>.Filter.Lt("docDate", de.end); }

                }

                //var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();

                List<News> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("comingSoonCategory", "category", "code", "categoryList")
                                      .As<News>()
                                      .ToList();

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
        public ActionResult<Response> Update([FromBody] News value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "comingSoon");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                if (value.organizationMode == "auto")
                {
                    var og = value.organization.filterQrganizationAuto();
                    value.lv0 = og.lv0;
                    value.lv1 = og.lv1;
                    value.lv2 = og.lv2;
                    value.lv3 = og.lv3;
                    value.lv4 = og.lv4;
                }

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.category)) { doc["category"] = value.category; }
                if (!string.IsNullOrEmpty(value.language)) { doc["language"] = value.language; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                doc["description"] = value.description;

                doc["fileUrl"] = value.fileUrl;
                doc["codeShort"] = value.codeShort;
                doc["linkUrl"] = value.linkUrl;
                doc["textButton"] = value.textButton;
                doc["sequence"] = value.sequence;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                doc["isPublic"] = value.isPublic;
                doc["isHighlight"] = value.isHighlight;
                doc["status"] = value.isActive ? "A" : "N";
                doc["lv0"] = value.lv0;
                doc["lv1"] = value.lv1;
                doc["lv2"] = value.lv2;
                doc["lv3"] = value.lv3;
                doc["lv4"] = value.lv4;

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
        public ActionResult<Response> Delete([FromBody] News value)
        {
            try
            {
                var col = new Database().MongoClient( "comingSoon");

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

        #region gallery

        // POST /create
        [HttpPost("gallery/create")]
        public ActionResult<Response> GalleryCreate([FromBody] Gallery value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "comingSoonGallery");


                value.code = "".toCode();

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "imageUrl", value.imageUrl },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "reference", value.reference },
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

        // POST /create
        [HttpPost("gallery/delete")]
        public ActionResult<Response> GalleryDelete([FromBody] Gallery value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "comingSoonGallery");

                {
                    //disable all
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("reference", value.code);
                        var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                        col.UpdateMany(filter, update);
                    }
                }

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        #endregion

        #region comment

        // POST /read
        [HttpPost("comment/read")]
        public ActionResult<Response> CommentRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Comment>("comingSoonComment");

                var filter = Builders<Comment>.Filter.Eq(x => x.isActive, true) | Builders<Comment>.Filter.Eq(x => x.isActive, false);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Comment>.Filter.Regex("reference", value.code); }

                //var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.docTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.description, c.createBy, c.createDate, c.imageUrlCreateBy, c.isActive }).ToList();

                List<Comment> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.docTime).Skip(value.skip).Limit(value.limit).ToList();

                docs.ForEach(c =>
                {
                    if (!string.IsNullOrEmpty(c.profileCode))
                    {
                        //Get Profile
                        var colRegister = new Database().MongoClient<Register>("register");
                        var filterRegister = Builders<Register>.Filter.Ne(x => x.status, "D") & Builders<Register>.Filter.Eq("code", c.profileCode);
                        var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                        c.createBy = docRegister.firstName + " " + docRegister.lastName;
                        c.imageUrlCreateBy = docRegister.imageUrl;
                    }
                });

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("comment/update")]
        public ActionResult<Response> Update([FromBody] Comment value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("comingSoonComment");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                doc["description"] = value.description;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /update
        [HttpPost("comment/verify")]
        public ActionResult<Response> Verify([FromBody] Comment value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("comingSoonComment");
                var descriptionVerify = value.description;

                try
                {
                    descriptionVerify = value.description.verifyRude();
                }
                catch { }

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                doc["description"] = descriptionVerify;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        #endregion

        #region category

        // POST /create
        [HttpPost("category/create")]
        public ActionResult<Response> CategoryCreate([FromBody] Category value)
        {
            
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("comingSoonCategory");

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
        public ActionResult<Response> CategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Category>("comingSoonCategory");

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

                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Category>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Category>.Filter.Regex("language", value.language); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }

                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive, c.updateBy, c.updateDate, c.sequence }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /update
        [HttpPost("category/update")]
        public ActionResult<Response> CategoryUpdate([FromBody] Category value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "comingSoonCategory");

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

                // ------- update content ------
                if (!value.isActive)
                {
                    var collectionContent = new Database().MongoClient("comingSoon");
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
                    var updatePermission = Builders<BsonDocument>.Update.Set("comingSoonPage", false).Set("isActive", false);
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
        [HttpPost("category/delete")]
        public ActionResult<Response> CategoryDelete([FromBody] Category value)
        {
            try
            {
                var col = new Database().MongoClient( "comingSoonCategory");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
                col.UpdateOne(filter, update);

                // ------- update content ------
                if (!value.isActive)
                {
                    var collectionContent = new Database().MongoClient("comingSoon");
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
                    var updatePermission = Builders<BsonDocument>.Update.Set("comingSoonPage", false).Set("isActive", false);
                    collectionPermission.UpdateMany(filterPermission, updatePermission);
                }
                // ------- end ------

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