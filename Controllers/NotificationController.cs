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
    public class NotificationController : Controller
    {
        public NotificationController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Notification value)
        {

            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "notification");

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
                
                if (value.category == "mainPage")
                {
                    value.reference = value.code;
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "sequence", value.sequence },
                    { "title", value.title },
                    { "category", value.category },
                    { "total", value.items.Count() },
                    //{ "page", value.page },
                    { "language", value.language },
                    { "fileUrl", value.fileUrl },
                    { "description", value.description },
                    { "reference", value.reference },
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
                    { "lv3", value.lv3 }
                };
                col.InsertOne(doc);


                value.items.Where(c => c.isSelected).ToList().ForEach(c =>
                {
                    this.CreateSend(new NotificationSend { username = c.username, category = c.category, reference = value.code });
                });

                //this.GalleryCreate(new Gallery { imageUrl = "https://www.cheatsheet.com/wp-content/uploads/2012/04/apple-logo.jpg", isActive = true, reference = value.code, createBy = "adminPO", createDate = "20200310150000" });
                //this.GalleryCreate(new Gallery { imageUrl = "https://www.cheatsheet.com/wp-content/uploads/2012/04/apple-logo.jpg", isActive = true, reference = value.code, createBy = "adminPO", createDate = "20200310150000" });
                //this.CommentCreate(new Comment { description = "comment 1", isActive = true, reference = value.code, createBy = "adminPO", createDate = "20200310150000" });
                //this.CommentCreate(new Comment { description = "comment 2", isActive = true, reference = value.code, createBy = "adminPO", createDate = "20200310150000" });

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
                var col = new Database().MongoClient<Notification>( "notification");
                var filter = (Builders<Notification>.Filter.Ne("status", "D") & value.filterOrganization<Notification>());

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Notification>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                    var permissionFilter = Builders<Notification>.Filter.Ne("status", "D");
                    //var permission = value.permission.Split(",");
                    //for (int i = 0; i < permission.Length; i++)
                    //{
                    //    if (i == 0)
                    //        permissionFilter = Builders<Notification>.Filter.Eq("page", permission[i]);
                    //    else
                    //        permissionFilter |= Builders<Notification>.Filter.Eq("page", permission[i]);
                    //}

                    //filter &= (permissionFilter);

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

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.sequence, c.title, c.language, c.description, c.category, c.reference, c.updateBy, c.updateDate, c.createTime, c.updateTime, c.docDate, c.docTime, c.to, c.sound, c.priority, c._displayInForeground, c.channelId, c.body, c.page, c.total }).ToList();

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
        public ActionResult<Response> Update([FromBody] Notification value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "notification");

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
                if (!string.IsNullOrEmpty(value.page)) { doc["page"] = value.page; }
                if (!string.IsNullOrEmpty(value.language)) { doc["language"] = value.language; }
                if (!string.IsNullOrEmpty(value.reference)) { doc["reference"] = value.reference; }
                if (!string.IsNullOrEmpty(value.description)) { doc["description"] = value.description; }

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
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] Notification value)
        {
            try
            {
                var col = new Database().MongoClient( "notification");

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
                var col = new Database().MongoClient("notificationGallery");


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
                var col = new Database().MongoClient("notificationGallery");

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


        #region Member

        // POST /read
        [HttpPost("member/read")]
        public ActionResult<Response> MemberRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne("status", "D");

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))
                        | (filter & Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")))
                        | (filter & Builders<Register>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")))
                        | (filter & Builders<Register>.Filter.Regex("email", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {

                    if (!string.IsNullOrEmpty(value.username)) { filter &= Builders<Register>.Filter.Eq("username", value.username); }
                    if (!string.IsNullOrEmpty(value.password)) { filter &= Builders<Register>.Filter.Eq("password", value.password); }
                    if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Register>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter &= Builders<Register>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.category)) { filter &= Builders<Register>.Filter.Regex("category", value.category); }
                    if (!string.IsNullOrEmpty(value.sex)) { filter &= Builders<Register>.Filter.Eq("sex", value.sex); }
                    if (!string.IsNullOrEmpty(value.lineID)) { filter &= Builders<Register>.Filter.Eq("lineID", value.lineID); }
                    if (!string.IsNullOrEmpty(value.firstName)) { filter &= Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.firstName), "i")); }
                    if (!string.IsNullOrEmpty(value.lastName)) { filter &= Builders<Register>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.lastName), "i")); }
                    if (!string.IsNullOrEmpty(value.status)) { filter &= Builders<Register>.Filter.Eq("status", value.status); }
                    //if (!string.IsNullOrEmpty(value.lv0)) { filter &= Builders<Register>.Filter.Eq("lv0", value.lv0); }
                    //if (!string.IsNullOrEmpty(value.lv1)) { filter &= Builders<Register>.Filter.Eq("lv1", value.lv1); }
                    //if (!string.IsNullOrEmpty(value.lv2)) { filter &= Builders<Register>.Filter.Eq("lv2", value.lv2); }
                    //if (!string.IsNullOrEmpty(value.lv3)) { filter &= Builders<Register>.Filter.Eq("lv3", value.lv3); }
                    //if (!string.IsNullOrEmpty(value.lv4)) { filter &= Builders<Register>.Filter.Eq("lv4", value.lv4); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", de.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new
                {
                    c.code,
                    c.username,
                    c.password,
                    c.isActive,
                    c.createBy,
                    c.createDate,
                    c.imageUrl,
                    c.updateBy,
                    c.updateDate,
                    c.createTime,
                    c.updateTime,
                    c.docDate,
                    c.docTime,
                    c.category,
                    c.prefixName,
                    c.firstName,
                    c.lastName,
                    c.birthDay,
                    c.phone,
                    c.email,
                    c.facebookID,
                    c.googleID,
                    c.lineID,
                    c.sex,
                    c.soi,
                    c.address,
                    c.moo,
                    c.road,
                    c.tambonCode,
                    c.tambon,
                    c.amphoeCode,
                    c.amphoe,
                    c.provinceCode,
                    c.province,
                    c.postnoCode,
                    c.postno,
                    c.job,
                    c.idcard,
                    c.officerCode,
                    c.countUnit,
                    c.status,
                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,
                    c.token,
                    c.isOnline
                }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("member2/read")]
        public ActionResult<Response> Member2Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne("status", "D");

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))
                        | (filter & Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")))
                        | (filter & Builders<Register>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")))
                        | (filter & Builders<Register>.Filter.Regex("email", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {

                    if (!string.IsNullOrEmpty(value.username)) { filter &= Builders<Register>.Filter.Eq("username", value.username); }
                    if (!string.IsNullOrEmpty(value.password)) { filter &= Builders<Register>.Filter.Eq("password", value.password); }
                    if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Register>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter &= Builders<Register>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.category)) { filter &= Builders<Register>.Filter.Regex("category", value.category); }
                    if (!string.IsNullOrEmpty(value.sex)) { filter &= Builders<Register>.Filter.Regex("sex", value.sex); }
                    if (!string.IsNullOrEmpty(value.lineID)) { filter &= Builders<Register>.Filter.Regex("lineID", value.lineID); }
                    if (!string.IsNullOrEmpty(value.firstName)) { filter &= Builders<Register>.Filter.Regex("firstName", value.firstName); }
                    if (!string.IsNullOrEmpty(value.lastName)) { filter &= Builders<Register>.Filter.Regex("lastName", value.lastName); }
                    if (!string.IsNullOrEmpty(value.lv0)) { filter &= Builders<Register>.Filter.Regex("lv0", new BsonRegularExpression(string.Format(".*{0}.*", value.lv0), "i")); }
                    if (!string.IsNullOrEmpty(value.lv1)) { filter &= Builders<Register>.Filter.Regex("lv1", new BsonRegularExpression(string.Format(".*{0}.*", value.lv1), "i")); }
                    if (!string.IsNullOrEmpty(value.lv2)) { filter &= Builders<Register>.Filter.Regex("lv2", new BsonRegularExpression(string.Format(".*{0}.*", value.lv2), "i")); }
                    if (!string.IsNullOrEmpty(value.lv3)) { filter &= Builders<Register>.Filter.Regex("lv3", new BsonRegularExpression(string.Format(".*{0}.*", value.lv3), "i")); }
                    if (!string.IsNullOrEmpty(value.lv4)) { filter &= Builders<Register>.Filter.Regex("lv4", new BsonRegularExpression(string.Format(".*{0}.*", value.lv4), "i")); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", de.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        #region send

        // POST /read
        [HttpPost("push")]
        public async Task<ActionResult<Response>> Push([FromBody] Notification value)
        {
            try
            {
                //----------------- Start Firebase -----------------

                var body = new FirebaseModel();
                var tokenArray = value.token.Split(',').ToList();
                var tokenList = tokenArray.GroupBy(c => c).Select(c => c.Key).ToList();

                body = new FirebaseModel
                {
                    //registration_ids = new List<string> { "eNiYBdB0S3KcaskMj5OkDe:APA91bE-mf6c5xOog2xIGC3yu7oI8zNrFtNB-MiuwZ83oGeJ9bhffePMx46YC7vKL5rDZ3bl8p0DP0rzgFwKdv--ocRK0J6GMSBs9LFI32xDQpKl2MAuHVpRQKL0p-_MGEzJHd32KfEU" },
                    registration_ids = tokenList,
                    notification = new NotificationModel { title = value.title, body = value.description },
                    data = new DataModel { title = value.title, body = value.description, page = value.data.page, code = value.data.code }
                };

                HttpRequestMessage httpRequest = null;
                HttpClient httpClient = null;

                var authorizationKey = string.Format("key={0}", "AAAA3yUfsSY:APA91bEBuY3wyOsrsXGLpuzPuYfT3xuCHl53VkaUqSs3zPwXCD3Ud7VZXje08hm2gleVJdBHTi7sZxMLoKOPKYhcAiagUsOkcECugDO67RYYViasLF_ZJIYkkBemSA81nT-LibFFLgVn");
                var jsonBody = JsonConvert.SerializeObject(body);

                try
                {
                    httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");

                    httpRequest.Headers.TryAddWithoutValidation("Authorization", authorizationKey);
                    httpRequest.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

                    httpClient = new HttpClient();
                    using (await httpClient.SendAsync(httpRequest))
                    {
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    httpRequest.Dispose();
                    httpClient.Dispose();
                }

                //HttpClient client = new HttpClient();
                //client.DefaultRequestHeaders.Authorization();
                //client.DefaultRequestHeaders.Add("Authorization", "key=AAAADQZeWAM:APA91bE6SqzWxZpuXh52CkBLgkhxnkTDdGCY2EBpyWehYjyqmyp9BHsZ2sH88pVDMSC6iY33Dd-ZE8X9Z4ZznuaAS-cNM8KSz163iTfPaEYvR0tmxsO40h0ESM7tTse0s9mjP39gZMLh");
                //client.DefaultRequestHeaders.Add("Content-Type", "application/json");

                //var json = JsonConvert.SerializeObject(body);
                //HttpContent httpContent = new StringContent(json);
                ////httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                //var response = await client.PostAsync("https://fcm.googleapis.com/fcm/send", httpContent);
                //var responseString = await response.Content.ReadAsStringAsync();
                //----------------- End Firebase -----------------

                return new Response { status = "S", message = "success", jsonData = "", objectData = new { } };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    status = "E",
                    message = ex.Message
                };
            }
        }

        // POST /create
        [HttpPost("send/create")]
        public ActionResult<Response> CreateSend([FromBody] NotificationSend value)
        {

            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("notificationSend");

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
                    { "code", "".toCode() },
                    { "username", value.username },
                    { "category", value.category },
                    { "reference", value.reference },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.updateBy },
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
                    { "lv4", value.lv4 }
                };
                col.InsertOne(doc);

                //this.GalleryCreate(new Gallery { imageUrl = "https://www.cheatsheet.com/wp-content/uploads/2012/04/apple-logo.jpg", isActive = true, reference = value.code, createBy = "adminPO", createDate = "20200310150000" });
                //this.GalleryCreate(new Gallery { imageUrl = "https://www.cheatsheet.com/wp-content/uploads/2012/04/apple-logo.jpg", isActive = true, reference = value.code, createBy = "adminPO", createDate = "20200310150000" });
                //this.CommentCreate(new Comment { description = "comment 1", isActive = true, reference = value.code, createBy = "adminPO", createDate = "20200310150000" });
                //this.CommentCreate(new Comment { description = "comment 2", isActive = true, reference = value.code, createBy = "adminPO", createDate = "20200310150000" });

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        #endregion
    }
}