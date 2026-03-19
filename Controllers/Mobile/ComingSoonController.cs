using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Models;
using cms_api.Extension;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class ComingSoonController : Controller
    {

        public ComingSoonController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {

                //value.statisticsCreate("comingSoon");
                var col = new Database().MongoClient<News>( "comingSoon");
                var filter = (Builders<News>.Filter.Eq("status", "A") & Builders<News>.Filter.Regex("codeShort", value.codeShort));

                List<News> docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("comingSoonCategory", "category", "code", "categoryList")
                                      .As<News>()
                                      .ToList();

                //var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();


                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient( "comingSoon");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit)
                                          .Lookup("comingSoonCategory", "category", "code", "categoryList")
                                          .As<News>()
                                          .ToList();

                    //docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();
                }
                //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                docs.ForEach(c =>
                {
                    try
                    {
                        //Get Profile
                        var colRegister = new Database().MongoClient<Register>("register");
                        var filterRegister = Builders<Register>.Filter.Eq("username", c.updateBy);
                        var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                        c.imageUrlCreateBy = docRegister.imageUrl;
                        c.createBy = docRegister.firstName + " " + docRegister.lastName;
                    }
                    catch
                    {

                    }
                });

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
                var col = new Database().MongoClient<Gallery>( "comingSoonGallery");

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

                var col = new Database().MongoClient<Comment>( "comingSoonComment");

                var filter = Builders<Comment>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Comment>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

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
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {

                //Get Profile
                var colRegister = new Database().MongoClient<Register>("register");
                var filterRegister = Builders<Register>.Filter.Ne(x => x.status, "D") & Builders<Register>.Filter.Eq("code", value.profileCode);
                var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                var word = value.description.verifyRude();

                var col = new Database().MongoClient( "comingSoonComment");

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
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.createBy },
                    { "profileCode", value.profileCode },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
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
                var col = new Database().MongoClient<Category>( "comingSoonCategory");

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

                ////BEGIN : Statistic
                //try
                //{
                //    if (!string.IsNullOrEmpty(value.code))
                //    {
                //        value.reference = value.code;
                //        value.title = docs.Count > 0 ? docs[0].title : "";
                //    }

                //    value.statisticsCreateAsync("newCategory");
                //}
                //catch { }
                ////END : Statistic

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

    }
}