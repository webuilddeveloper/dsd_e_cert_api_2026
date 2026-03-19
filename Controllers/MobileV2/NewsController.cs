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
    [Route("m/v2/[controller]")]
    public class NewsController : Controller
    {

        public NewsController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                //value.statisticsCreate("news");
                var col = new Database().MongoClient<News>("news");
                var filter = Builders<News>.Filter.Eq("status", "A");
                //var filter = (Builders<News>.Filter.Eq("status", "A") & value.filterOrganization<News>());

                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<News>.Filter.Eq("code", value.code); }
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter &= Builders<News>.Filter.Regex("title", value.keySearch);

                    //BEGIN : Statistic
                    //try
                    //{
                    //    var value1 = new Criteria();

                    //    value1.title = value.keySearch;
                    //    value1.updateBy = value.updateBy;

                    //    if (!string.IsNullOrEmpty(value.code))
                    //        value1.reference = value.code;

                    //    value1.statisticsCreate("newsKeySearch");
                    //}
                    //catch { }
                    //END : Statistic
                }
                if (!string.IsNullOrEmpty(value.category)) { filter &= Builders<News>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.language)) { filter &= Builders<News>.Filter.Regex("language", value.language); }
                if (value.isHighlight) { filter &= Builders<News>.Filter.Eq("isHighlight", value.isHighlight); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<News>.Filter.Gt("docDate", ds.start) & Builders<News>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<News>.Filter.Gt("docDate", ds.start) & Builders<News>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<News>.Filter.Gt("docDate", de.start) & Builders<News>.Filter.Lt("docDate", de.end); }

                List<News> docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("newsCategory", "category", "code", "categoryList")
                                          .Lookup("register", "createBy", "username", "userList")
                                      .As<News>()
                                      .ToList();

                //var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();

                //BEGIN : Statistic
                //try
                //{
                //    if (!string.IsNullOrEmpty(value.code))
                //    {
                //        value.reference = value.code;
                //        value.title = docs.Count > 0 ? docs[0].title : "";
                //        value.category = docs.Count > 0 ? docs[0].categoryList[0].title : "";
                //    }

                //    value.statisticsCreate("news");
                //}
                //catch { }
                //END : Statistic

                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient( "news");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit)
                                          .Lookup("newsCategory", "category", "code", "categoryList")
                                          .Lookup("register", "createBy", "username", "userList")
                                          .As<News>()
                                          .ToList();

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
        [HttpPost("gallery/read")]
        public ActionResult<Response> GalleryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Gallery>( "newsGallery");

                var filter = Builders<Gallery>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Gallery>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).Project(c => new { c.imageUrl, c.code }).ToList();

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
        [HttpPost("comment/read")]
        public ActionResult<Response> CommentRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Comment>( "newsComment");

                var filter = Builders<Comment>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Comment>.Filter.Regex("reference", value.code); }

                List<Comment> rawDocs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.docTime).Skip(value.skip).Limit(value.limit)
                                          .Lookup("register", "createBy", "username", "userList")
                                          .As<Comment>()
                                          .ToList();

                var docs = new List<Comment>();

                rawDocs.ForEach(c =>
                {
                    var createBy = "";
                    var imageUrlCreateBy = "";
                    if (c.userList.Count > 0) {
                        createBy = c.userList[0].firstName + " " + c.userList[0].lastName;
                        imageUrlCreateBy = c.userList[0].imageUrl;
                    }
                    else { 
                        createBy = c.createBy;
                        imageUrlCreateBy = c.imageUrlCreateBy;
                    }
                    docs.Add(new Comment
                    {
                        description = c.description,
                        createBy = createBy,
                        imageUrlCreateBy = imageUrlCreateBy,
                        createDate = c.createDate,
                    });
                });
                
                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("comment/create")]
        public ActionResult<Response> CommentCreate([FromBody] Comment value)
        {
            //Get Profile
            var colRegister = new Database().MongoClient<Register>("register");
            var filterRegister = Builders<Register>.Filter.Ne(x => x.status, "D") & Builders<Register>.Filter.Eq("code", value.profileCode) ;
            var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var word = value.description.verifyRude();

                var col = new Database().MongoClient("newsComment");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "description", word },
                    { "original", value.description },
                    { "imageUrlCreateBy", docRegister.imageUrl },
                    { "createBy", docRegister.firstName + ' ' + docRegister.lastName },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", docRegister.firstName + ' ' + docRegister.lastName },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                    { "reference", value.reference }
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
                var col = new Database().MongoClient<Category>( "newsCategory");

                var filter = Builders<Category>.Filter.Eq(x => x.status, "A");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = Builders<Category>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")) | (filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Category>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Category>.Filter.Regex("title", value.title); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Category>.Filter.Regex("language", value.language); }
                    //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<NewsCategory>.Filter.Regex("dateStart", value.startDate); }
                    //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<NewsCategory>.Filter.Regex("dateEnd", value.endDate); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive }).ToList();

                //BEGIN : Statistic
                try
                {
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        value.reference = value.code;
                        value.title = docs.Count > 0 ? docs[0].title : "";
                    }

                    value.statisticsCreate("newCategory");
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