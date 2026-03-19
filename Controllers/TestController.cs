using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class TestController : Controller
    {
        public TestController() { }

        #region main

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var db = new Database();
                var col = new Database().MongoClient<News>("news");
                var filter = (Builders<Notification>.Filter.Ne("status", "D") & value.filterOrganization<Notification>());

                List<News> result = col.Aggregate()
                                       .Lookup("newsCategory", "category", "title", "categoryList")
                                       .As<News>()
                                       .ToList();

                //result.ForEach(c => new News
                //{
                //    code = c[0].ToString(),
                //    categoryList = BsonSerializer.Deserialize<List<Category>>(c["categoryList"].ToJson())
                //}); ;

                // https://stackoverflow.com/questions/51759997/mongodb-join-using-linq 

                //var news = db.MongoClient<News>("news");
                //var category = db.MongoClient<Category>("newsCategory");

                //List<NewsWithCategory> result = news.Aggregate()
                //                                       .Lookup<News, Category, NewsWithCategory>(
                //                                            category,
                //                                            x => x.category,
                //                                            x => x.title,
                //                                            x => x.Categories).ToList();

                return new Response { status = "S", message = "success", objectData = result };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        public class NewsWithCategory : News
        {
            public List<Category> Categories { get; set; }
        }

        // POST /create
        [HttpPost("configulation/email")]
        public ActionResult<Response> ForgetReadAsync([FromBody] Register value)
        {
            try
            {

                var doc = new BsonDocument();
                var col = new Database().MongoClient("configulation");

                var filter = Builders<BsonDocument>.Filter.Eq("title", "email");
                if (col.Find(filter).Any())
                {
                    doc = col.Find(filter).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                    if (!string.IsNullOrEmpty(value.username)) { doc["username"] = value.username; }
                    if (!string.IsNullOrEmpty(value.email)) { doc["email"] = value.email; }
                    if (!string.IsNullOrEmpty(value.password)) { doc["password"] = value.password; }
                    doc["updateBy"] = value.updateBy;
                    doc["updateDate"] = DateTime.Now.toStringFromDate();
                    doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                    doc["isActive"] = value.isActive;
                    col.ReplaceOne(filter, doc);
                }
                else
                {
                    doc = new BsonDocument
                    {
                    { "code", "".toCode() },
                    { "title", value.title },
                    { "username", value.username },
                    { "email", value.email },
                    { "password", value.password },
                    { "createBy", value.createBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", value.isActive }
                    };
                    col.InsertOne(doc);
                }

                return new Response { status = "s", message = "success", jsonData = value.ToJson(), objectData = value };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

    }

    #endregion
}
