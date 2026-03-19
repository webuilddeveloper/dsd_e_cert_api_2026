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
    public class ReporterController : Controller
    {

        public ReporterController() { }

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Suggestion value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("reporter");

                //check duplicate
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
                    { "title", value.title },
                    { "category", value.category },
                    { "latitude", value.latitude },
                    { "longitude", value.longitude },
                    { "language", value.language },
                    { "firstName", value.firstName },
                    { "lastName", value.lastName },
                    { "description", value.description},
                    { "imageUrl", value.imageUrl },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.createBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                    { "status", "N" },
                    { "lv0", value.lv0 },
                    { "lv1", value.lv1 },
                    { "lv2", value.lv2 },
                    { "lv3", value.lv3 },
                    { "lv4", value.lv4 },
                    { "status2", "N"},
                    { "province", value.province }
                };
                col.InsertOne(doc);

                //BEGIN :history >>>>>>>>>>>>>>>>>>>>>>>>>>>>

                var docHistory = new BsonDocument();
                var colHistory = new Database().MongoClient("reporterHistory");

                docHistory = new BsonDocument
                    {
                        { "code", "".toCode() },
                        { "category", value.category },
                        { "reference", value.code },
                        { "imageUrlCreateBy", value.imageUrlCreateBy },
                        { "createBy", value.createBy },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", value.updateBy },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true },
                        { "status", value.status2 }
                    };
                colHistory.InsertOne(docHistory);

                //END :history <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                //BEGIN :create gallery >>>>>>>>>>>>>>>>>>>>>>>>>>>>

                value.gallery.ForEach(c =>
                {
                    var colGallery = new Database().MongoClient("reporterGallery");

                    doc = new BsonDocument
                    {
                        { "code", "".toCode() },
                        { "imageUrl", c.imageUrl },
                        { "createBy", value.createBy },
                        { "createDate", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true },
                        { "reference", value.code }
                    };
                    colGallery.InsertOne(doc);
                });

                //END :create gallery <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


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
                value.statisticsCreate("reporter");
                var col = new Database().MongoClient<Suggestion>("reporter");
                var filter = value.filterOrganizationOld<Suggestion>();

                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Suggestion>.Filter.Eq("code", value.code); }
                if (!string.IsNullOrEmpty(value.keySearch)) { filter &= Builders<Suggestion>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter &= Builders<Suggestion>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.description)) { filter &= Builders<Suggestion>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                if (!string.IsNullOrEmpty(value.createBy)) { filter &= Builders<Suggestion>.Filter.Eq("createBy", value.createBy); }
                if (!string.IsNullOrEmpty(value.province)) { filter &= Builders<Suggestion>.Filter.Eq("province", value.province); }
                if (!string.IsNullOrEmpty(value.language)) { filter &= Builders<Suggestion>.Filter.Regex("language", value.language); }
                if (!string.IsNullOrEmpty(value.firstName)) { filter &= Builders<Suggestion>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.firstName), "i")); }
                if (!string.IsNullOrEmpty(value.lastName)) { filter &= Builders<Suggestion>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.lastName), "i")); }
                //if (value.isPublic) { filter = filter & Builders<Suggestion>.Filter.Eq("isPublic", value.isPublic); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Suggestion>.Filter.Gt("docDate", ds.start) & Builders<Suggestion>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Suggestion>.Filter.Gt("docDate", ds.start) & Builders<Suggestion>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Suggestion>.Filter.Gt("docDate", de.start) & Builders<Suggestion>.Filter.Lt("docDate", de.end); }

                List<Suggestion> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("reporterCategory", "category", "code", "categoryList")
                                          .Lookup("register", "createBy", "username", "userList")
                                      .As<Suggestion>()
                                      .ToList();

                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient("reporter");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit)
                                      .Lookup("reporterCategory", "category", "code", "categoryList")
                                          .Lookup("register", "createBy", "username", "userList")
                                      .As<Suggestion>()
                                      .ToList();

                    //docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();
                }
                //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                //var listModel = new List<Suggestion>();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("gallery/read")]
        public ActionResult<Response> GalleryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Gallery>("reporterGallery");

                var filter = Builders<Gallery>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Gallery>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).Project(c => new { c.imageUrl }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("category/read")]
        public ActionResult<Response> SuggestionCategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Category>("reporterCategory");

                var filter = Builders<Category>.Filter.Eq(x => x.status, "A");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Category>.Filter.Regex("title", value.keySearch)) | (filter & Builders<Category>.Filter.Regex("description", value.keySearch));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Category>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Category>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Category>.Filter.Regex("language", value.language); }
                    //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Suggestion>.Filter.Regex("dateStart", value.startDate); }
                    //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Suggestion>.Filter.Regex("dateEnd", value.endDate); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.createBy, c.createDate, c.isActive, c.imageUrl }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("reply/read")]
        public ActionResult<Response> ReplyRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Comment>("reporterReply");

                var filter = Builders<Comment>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Comment>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.description, c.createBy, c.createDate, c.imageUrlCreateBy }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("count")]
        public ActionResult<Response> Count([FromBody] Criteria value)
        {
            //username, status = "N"

            try
            {
                var col = new Database().MongoClient<Suggestion>("reporter");
                var filter = value.filterOrganization<Suggestion>();

                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Suggestion>.Filter.Eq("code", value.code); }
                if (!string.IsNullOrEmpty(value.keySearch)) { filter &= Builders<Suggestion>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter &= Builders<Suggestion>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.description)) { filter &= Builders<Suggestion>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                if (!string.IsNullOrEmpty(value.language)) { filter &= Builders<Suggestion>.Filter.Eq("language", value.language); }
                if (!string.IsNullOrEmpty(value.createBy)) { filter &= Builders<Suggestion>.Filter.Eq("createBy", value.createBy); }
                //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Suggestion>.Filter.Regex("dateStart", value.startDate); }
                //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Suggestion>.Filter.Regex("dateEnd", value.endDate); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Suggestion>.Filter.Gt("docDate", ds.start) & Builders<Suggestion>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Suggestion>.Filter.Gt("docDate", ds.start) & Builders<Suggestion>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Suggestion>.Filter.Gt("docDate", de.start) & Builders<Suggestion>.Filter.Lt("docDate", de.end); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).SortByDescending(o => o.docDate).Project(c => new { c.code, c.title, c.titleEN, c.status, c.category, c.description, c.descriptionEN, c.latitude, c.longitude, c.imageUrlCreateBy, c.createBy, c.createDate, c.isActive, c.status2 }).ToList();

                var doc = new
                {
                    reporter = docs.Count().ToString()
                };

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = doc };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }


    }
}