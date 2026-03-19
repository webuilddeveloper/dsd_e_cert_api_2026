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

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class NotificationController : Controller
    {
        public NotificationController() { }

        #region main

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            //username, skip, limit

            try
            {
                value.statisticsCreate("notification");
                var docs = new List<Notification>();
                var col = new Database().MongoClient<NotificationSend>("notificationSend");
                var filter = Builders<NotificationSend>.Filter.Ne("status", "D");

                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<NotificationSend>.Filter.Eq("code", value.code); }
                if (!string.IsNullOrEmpty(value.username)) { filter &= Builders<NotificationSend>.Filter.Eq("username", value.username); }
                if (!string.IsNullOrEmpty(value.reference)) { filter &= Builders<NotificationSend>.Filter.Eq("reference", value.reference); }

                var rawDocs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("notification", "reference", "code", "notificationList")
                                          .Lookup("register", "createBy", "username", "userList")
                                      .As<NotificationSend>()
                                      .ToList();

                rawDocs.ForEach( c => {
                    docs.Add(new Notification
                    {
                        code = c.code,
                        category = c.notificationList[0].category,
                        page = c.notificationList[0].page,
                        title = c.notificationList[0].title,
                        description = c.notificationList[0].description,
                        reference = c.notificationList[0].reference,
                        username = c.username,

                        lv0 = c.notificationList[0].lv0,
                        lv1 = c.notificationList[0].lv1,
                        lv2 = c.notificationList[0].lv2,
                        lv3 = c.notificationList[0].lv3,
                        lv4 = c.notificationList[0].lv4,

                        createBy = c.notificationList[0].createBy,
                        createDate = c.notificationList[0].createDate,
                        status = c.status,
                        imageUrlCreateBy = c.notificationList[0].imageUrlCreateBy,
                        docDate = c.notificationList[0].docDate,
                        docTime = c.notificationList[0].docTime
                    });
                });
                //docs.Add(new { code = "code-01", title = "title 1", description = "description 1", page = "mainPage", reference = "", username = "test", createBy = "adminpo", createDate = "20200426", status = "N", imageUrlCreateBy = "http://122.155.223.63/td-doc/images/news/news_203356098.jpg" });
                
                //var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.page, c.title, c.description, c.reference, c.username, c.titleEN, c.descriptionEN, c.createDate, c.createBy, c.imageUrlCreateBy,c.status }).ToList();

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
                var col = new Database().MongoClient<Gallery>("notificationGallery");

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

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] Notification value)
        {
            //ส่ง code เข้ามา update status = "A";

            var col = new Database().MongoClient("notificationSend");

            var filter = Builders<BsonDocument>.Filter.Eq("username", value.username) & Builders<BsonDocument>.Filter.Ne("status", "D");
            if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<BsonDocument>.Filter.Regex("code", value.code);}
            else if (!string.IsNullOrEmpty(value.page)) { filter = filter & Builders<BsonDocument>.Filter.Regex("page", value.page); }

            var doc = col.Find(filter).ToList();
            doc.ForEach(c =>
            {
                var update = Builders<BsonDocument>.Update.Set("status", "A").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
                col.UpdateMany(filter, update);
            });

            //username, code 
            //username
            //username, page

            return new Response { status = "S", message = "success" };
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] Notification value)
        {
            //ส่ง code เข้ามา update status = "D";

            var col = new Database().MongoClient("notificationSend");

            var filter = Builders<BsonDocument>.Filter.Eq("username", value.username);
            if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<BsonDocument>.Filter.Regex("code", value.code); }
            else if (!string.IsNullOrEmpty(value.page)) { filter &= Builders<BsonDocument>.Filter.Regex("page", value.page); }

            var doc = col.Find(filter).ToList();
            doc.ForEach(c =>
            {
                var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
                col.UpdateMany(filter, update);
            });

            return new Response { status = "S", message = "success" };
        }

        // POST /read
        [HttpPost("count")]
        public ActionResult<Response> Count([FromBody] Criteria value)
        {
            //username, status = "N"

            //var docs = new List<Notification>();
            var col = new Database().MongoClient<NotificationSend>("notificationSend");
            var filter = Builders<NotificationSend>.Filter.Ne("status", "D");
            if (!string.IsNullOrEmpty(value.username)) { filter = filter & Builders<NotificationSend>.Filter.Eq("username", value.username); }

            var docs = col.Find(filter).Project(c => new { c.code, c.category, c.status }).ToList();

            var doc = new
            {
                mainPage = docs.Where(c => c.category == "mainPage" && c.status != "A").Count(),
                newsPage = docs.Where(c => c.category == "newsPage" && c.status != "A").Count(),
                eventPage = docs.Where(c => c.category == "eventPage" && c.status != "A").Count(),
                contactPage = docs.Where(c => c.category == "contactPage" && c.status != "A").Count(),
                knowledgePage = docs.Where(c => c.category == "knowledgePage" && c.status != "A").Count(),
                privilegePage = docs.Where(c => c.category == "privilegePage" && c.status != "A").Count(),
                poiPage = docs.Where(c => c.category == "poiPage" && c.status != "A").Count(),
                pollPage = docs.Where(c => c.category == "pollPage" && c.status != "A").Count(),
                suggestionPage = docs.Where(c => c.category == "suggestionPage" && c.status != "A").Count(),
                reporterPage = docs.Where(c => c.category == "reporterPage" && c.status != "A").Count(),
                trainingPage = docs.Where(c => c.category == "trainingPage" && c.status != "A").Count(),
                welfarePage = docs.Where(c => c.category == "welfarePage" && c.status != "A").Count(),
                total = docs.Where(c => c.status != "A").Count()
            };

            //ถ้า page = "mainPage" ใช้ description ถ้าไม่ใช่ ใช้ reference
            return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = doc };
        }

        // POST /read
        [HttpPost("detail")]
        public ActionResult<Response> Detail([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Notification>("notification");
                var filter = (Builders<Notification>.Filter.Ne("status", "D"));

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Notification>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                    var permissionFilter = Builders<Notification>.Filter.Ne("status", "D");
                }
                else
                {

                    if (!string.IsNullOrEmpty(value.category))
                    {
                        filter = filter & Builders<Notification>.Filter.Regex("page", value.category);
                    }
                    else
                    {
                        //var permissionFilter = Builders<Notification>.Filter.Ne("status", "D");
                        //var permission = value.permission.Split(",");
                        //for (int i = 0; i < permission.Length; i++)
                        //{
                        //    if (i == 0)
                        //        permissionFilter = Builders<Notification>.Filter.Eq("category", permission[i]);
                        //    else
                        //        permissionFilter |= Builders<Notification>.Filter.Eq("category", permission[i]);
                        //}

                        //filter &= (permissionFilter);

                    }


                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Notification>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<Notification>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Notification>.Filter.Eq("createBy", value.createBy); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Notification>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Notification>.Filter.Regex("language", value.language); }
                    if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<Notification>.Filter.Eq("sequence", sequence); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Notification>.Filter.Gt("docDate", ds.start) & Builders<Notification>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Notification>.Filter.Gt("docDate", ds.start) & Builders<Notification>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Notification>.Filter.Gt("docDate", de.start) & Builders<Notification>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.sequence, c.title, c.language, c.description, c.category, c.reference, c.updateBy, c.updateDate, c.createTime, c.updateTime, c.docDate, c.docTime, c.to, c.sound, c.priority, c._displayInForeground, c.channelId,c.fileUrl, c.body, c.page, c.total }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
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