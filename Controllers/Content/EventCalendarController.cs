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
    public class EventCalendarController : Controller
    {
        public EventCalendarController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] EventCalendar value)
        {
          
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("eventCalendar");

                //check duplicate
                value.code = "".toCode();
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                if (value.organizationMode == "auto")
                {
                    Task.Run(() => { value.organization.createQrganizationAuto(value.code, "eventCalendar"); });

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
                    { "fileUrl", value.fileUrl},
                    { "linkUrl", value.linkUrl},
                    { "textButton", value.textButton},
                    { "view", value.view},
                    { "linkFacebook", value.linkFacebook},
                    { "linkYoutube", value.linkYoutube},
                    { "numberOfDayNotification", value.numberOfDayNotification},
                    { "dateStart", value.dateStart == "Invalid date" ? "" : value.dateStart},
                    { "dateEnd", value.dateEnd == "Invalid date" ? "" : value.dateEnd},
                    { "confirmStatus", value.confirmStatus},
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
                    { "isHighlight", value.isHighlight },
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
                var col = new Database().MongoClient<EventCalendar>( "eventCalendar");
                var filter = (Builders<EventCalendar>.Filter.Ne("status", "D") & value.filterOrganization<EventCalendar>());

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<EventCalendar>.Filter.Regex("title", value.keySearch)) | (filter & Builders<EventCalendar>.Filter.Regex("description", value.keySearch));

                    if (value.permission != "all")
                        filter &= value.permission.filterPermission<EventCalendar>("category");
                }
                else
                {

                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<EventCalendar>.Filter.Eq("category", value.category);
                    else
                        if (value.permission != "all")
                        filter &= value.permission.filterPermission<EventCalendar>("category");


                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<EventCalendar>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<EventCalendar>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<EventCalendar>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<EventCalendar>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<EventCalendar>.Filter.Regex("language", value.language); }
                    if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<EventCalendar>.Filter.Eq("sequence", sequence); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", de.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }

                    // filter event date.
                    var dse = value.startDateEvent.toDateFromString().AddHours(7);
                    var dee = value.endDateEvent.toDateFromString().AddHours(7);
                    if (value.startDateEvent != "Invalid date" && value.endDateEvent != "Invalid date" && !string.IsNullOrEmpty(value.startDateEvent) && !string.IsNullOrEmpty(value.endDateEvent)) { filter = filter & Builders<EventCalendar>.Filter.Lte("docDateStartEvent", dse) & Builders<EventCalendar>.Filter.Gte("docDateEndEvent", dee); }
                    else if (value.startDateEvent != "Invalid date" && !string.IsNullOrEmpty(value.startDateEvent)) { filter = filter & Builders<EventCalendar>.Filter.Lte("docDateStartEvent", dse) & Builders<EventCalendar>.Filter.Gte("docDateEndEvent", dse); }
                    else if (value.endDateEvent != "Invalid date" && !string.IsNullOrEmpty(value.endDateEvent)) { filter = filter & Builders<EventCalendar>.Filter.Lte("docDateStartEvent", dee) & Builders<EventCalendar>.Filter.Gte("docDateEndEvent", dee); }
                }

                //var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.imageUrlCreateBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.sequence, c.category, c.textButton, c.confirmStatus, c.linkUrl, c.linkFacebook, c.linkYoutube, c.dateStart, c.dateEnd, c.view, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();

                List<EventCalendar> docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("eventCalendarCategory", "category", "code", "categoryList")
                                      .As<EventCalendar>()
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
        public ActionResult<Response> Update([FromBody] EventCalendar value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "eventCalendar");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                if (value.organizationMode == "auto")
                {
                    Task.Run(() => { value.organization.createQrganizationAuto(value.code, "eventCalendar"); });

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
                if (!string.IsNullOrEmpty(value.linkYoutube)) { doc["linkYoutube"] = value.linkYoutube; }
                if (!string.IsNullOrEmpty(value.linkFacebook)) { doc["linkFacebook"] = value.linkFacebook; }
                if (!string.IsNullOrEmpty(value.confirmStatus)) { doc["confirmStatus"] = value.confirmStatus; }
                if (!string.IsNullOrEmpty(value.dateEnd)) { doc["dateEnd"] = value.dateEnd; }
                if (!string.IsNullOrEmpty(value.dateStart)) { doc["dateStart"] = value.dateStart; }
                if (!string.IsNullOrEmpty(value.updateBy)) { doc["updateBy"] = value.updateBy; }


                doc["fileUrl"] = value.fileUrl;
                doc["numberOfDayNotification"] = value.numberOfDayNotification;
                doc["linkUrl"] = value.linkUrl;
                doc["textButton"] = value.textButton;
                doc["sequence"] = value.sequence;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                doc["isHighlight"] = value.isHighlight;
                doc["status"] = value.organizationMode == "auto" ? "P" : value.isActive ? "A" : "N";
                doc["lv0"] = value.lv0;
                doc["lv1"] = value.lv1;
                doc["lv2"] = value.lv2;
                doc["lv3"] = value.lv3;
                doc["lv4"] = value.lv4;
                doc["lv5"] = value.lv5;
                doc["isPublic"] = value.isPublic;
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
        public ActionResult<Response> Delete([FromBody] EventCalendar value)
        {
            try
            {
                var col = new Database().MongoClient( "eventCalendar");

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
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "eventCalendarGallery");

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
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "reference", value.reference },
                    { "isActive", true }
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /delete
        [HttpPost("gallery/delete")]
        public ActionResult<Response> GalleryDelete([FromBody] Gallery value)
        {
            try
            {
                var doc = new BsonDocument();
                var col = new Database().MongoClient( "eventCalendarGallery");

                var filter = Builders<BsonDocument>.Filter.Eq("reference", value.code);
                var update = Builders<BsonDocument>.Update.Set("isActive", false);
                col.UpdateMany(filter, update);
                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        #region comment

        // POST /create
        [HttpPost("comment/create")]
        public ActionResult<Response> CommentCreate([FromBody] Comment value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "eventCalendarComment");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "description", value.description },
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
        [HttpPost("comment/read")]
        public ActionResult<Response> CommentRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Comment>( "eventCalendarComment");

                var filter = Builders<Comment>.Filter.Eq(x => x.isActive, true) | Builders<Comment>.Filter.Eq(x => x.isActive, false);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Comment>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                List<Comment> rawDocs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.docTime).Skip(value.skip).Limit(value.limit)
                                          .Lookup("register", "createBy", "username", "userList")
                                          .As<Comment>()
                                          .ToList();

                var docs = new List<Comment>();

                rawDocs.ForEach(c =>
                {
                    var createBy = "";
                    var imageUrlCreateBy = "";
                    if (c.userList.Count > 0)
                    {
                        createBy = c.userList[0].firstName + " " + c.userList[0].lastName;
                        imageUrlCreateBy = c.userList[0].imageUrl;
                    }
                    else
                    {
                        createBy = c.createBy;
                        imageUrlCreateBy = c.imageUrlCreateBy;
                    }
                    docs.Add(new Comment
                    {
                        code = c.code,
                        description = c.description,
                        createBy = createBy,
                        imageUrlCreateBy = imageUrlCreateBy,
                        createDate = c.createDate,
                        isActive = c.isActive
                    });
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
                var col = new Database().MongoClient("eventCalendarComment");

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
                var col = new Database().MongoClient("eventCalendarComment");
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
                var col = new Database().MongoClient( "eventCalendarCategory");

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
                var col = new Database().MongoClient<Category>( "eventCalendarCategory");

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
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<Category>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Category>.Filter.Eq("createBy", value.createBy); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Category>.Filter.Regex("language", value.language); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    
                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive, c.updateDate, c.updateBy, c.sequence }).ToList();

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
                var col = new Database().MongoClient( "eventCalendarCategory");

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
                    var collectionContent = new Database().MongoClient("eventCalendar");
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
                    var updatePermission = Builders<BsonDocument>.Update.Set("eventPage", false).Set("isActive", false);
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
                var col = new Database().MongoClient( "eventCalendarCategory");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
                col.UpdateOne(filter, update);

                // ------- update content ------
                if (!value.isActive)
                {
                    var collectionContent = new Database().MongoClient("eventCalendar");
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
                    var updatePermission = Builders<BsonDocument>.Update.Set("eventPage", false).Set("isActive", false);
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

        #region re order

        // POST /create
        [HttpPost("reorder")]
        public ActionResult<Response> ReOrder([FromBody] EventCalendar value)
        {

            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient<BsonDocument>("eventCalendar");
                var arrayFilter = Builders<BsonDocument>.Filter.Ne("sequence", 9999) & Builders<BsonDocument>.Filter.Ne("status", "D");
                var arrayUpdate = Builders<BsonDocument>.Update.Set("sequence", 9999).Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());

                col.UpdateMany(arrayFilter, arrayUpdate);

                var codeList = value.code.Split(",");

                int sequence = 1;
                foreach (var code in codeList)
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("sequence", sequence).Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                    col.UpdateOne(filter, update);
                    sequence++;
                }

                // if data type list.
                //value.ForEach(o => {
                //    var filter = Builders<BsonDocument>.Filter.Ne("code", o.code);
                //    doc["sequence"] = sequence;
                //    col.ReplaceOne(filter, doc);
                //    sequence++;
                //});


                return new Response { status = "S", message = "success", jsonData = value.ToJson(), objectData = value };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        #endregion
    }
}