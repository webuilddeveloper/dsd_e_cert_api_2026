using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class EventCalendarController : Controller
    {
        public EventCalendarController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                value.statisticsCreate("eventCalendar");
                var col = new Database().MongoClient<EventCalendar>("eventCalendar");
                var filter = (Builders<EventCalendar>.Filter.Eq("status", "A") & value.filterOrganization<EventCalendar>());

                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<EventCalendar>.Filter.Regex("code", value.code); }
                if (!string.IsNullOrEmpty(value.keySearch)) { filter = filter & Builders<EventCalendar>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<EventCalendar>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<EventCalendar>.Filter.Eq("status", value.status); }
                if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<EventCalendar>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<EventCalendar>.Filter.Regex("language", value.language); }
                if (value.isHighlight) { filter = filter & Builders<EventCalendar>.Filter.Eq("isHighlight", value.isHighlight); }
                if (value.isPublic) { filter = filter & Builders<EventCalendar>.Filter.Eq("isPublic", value.isPublic); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", de.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                List<EventCalendar> docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("eventCalendarCategory", "category", "code", "categoryList")
                                          .Lookup("register", "createBy", "username", "userList")
                                      .As<EventCalendar>()
                                      .ToList();

                //var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.imageUrlCreateBy, c.createDate, c.description, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.sequence, c.category, c.confirmStatus, c.linkFacebook, c.linkYoutube, c.dateStart, c.dateEnd, c.view, c.linkUrl, c.textButton, c.fileUrl }).ToList();
                
                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient("eventCalendar");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                     .Lookup("newsCategory", "category", "code", "categoryList")
                                         .Lookup("register", "createBy", "username", "userList")
                                     .As<EventCalendar>()
                                     .ToList();
                }
                //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                // where by date
                var filterData = new List<object>();

                if (!string.IsNullOrEmpty(value.date))
                {
                    string dateSub = value.date.Substring(0, 10);
                    string[] dateArray = dateSub.Split("-");
                    string date = dateArray[0] + dateArray[1] + dateArray[2];
                    bool dateBetween = false;

                    foreach (var c in docs)
                    {
                        dateBetween = Between(date.toDateFromString(), c.dateStart.toDateFromString(), c.dateEnd.toDateFromString());
                        if (dateBetween)
                        {
                            filterData.Add(c);
                        }
                    }

                    return new Response { status = "S", message = "success", jsonData = filterData.ToJson(), objectData = filterData };
                }


                //end where by date

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("read2")]
        public ActionResult<Response> Read2([FromBody] Criteria value)
        {
            try
            {
                value.statisticsCreate("eventCalendar");
                var col = new Database().MongoClient<EventCalendar>("eventCalendar");
                var filter = (Builders<EventCalendar>.Filter.Eq("status", "A") & value.filterOrganization<EventCalendar>());

                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<EventCalendar>.Filter.Eq("code", value.code); }
                if (!string.IsNullOrEmpty(value.keySearch)) { filter &= Builders<EventCalendar>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<EventCalendar>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<EventCalendar>.Filter.Eq("status", value.status); }
                if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<EventCalendar>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<EventCalendar>.Filter.Regex("language", value.language); }
                if (value.isHighlight) { filter &= Builders<EventCalendar>.Filter.Eq("isHighlight", value.isHighlight); }
                if (value.isPublic) { filter &= Builders<EventCalendar>.Filter.Eq("isPublic", value.isPublic); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", de.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.imageUrlCreateBy, c.createDate, c.description, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.sequence, c.category, c.confirmStatus, c.linkFacebook, c.linkYoutube, c.dateStart, c.dateEnd, c.view, c.linkUrl, c.textButton, c.fileUrl }).ToList();

                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient("eventCalendar");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.imageUrlCreateBy, c.createDate, c.description, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.sequence, c.category, c.confirmStatus, c.linkFacebook, c.linkYoutube, c.dateStart, c.dateEnd, c.view, c.linkUrl, c.textButton, c.fileUrl }).ToList();
                }
                //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                var result = new List<EventCalendar2>();
                docs.OrderBy(o => o.dateStart).ToList().ForEach(c =>
                {
                    var items = new List<EventCalendar2Item>();
                    var monthYear = "";
                    try { monthYear = c.dateStart.Substring(0, 6); } catch { }

                    if (!result.Any(an => an.monthYear == monthYear))
                    {
                        items.Add(new EventCalendar2Item { code = c.code, dateStart = c.dateStart, dateEnd = c.dateEnd, title = c.title });
                        result.Add(new EventCalendar2 { monthYear = monthYear, dateStart = c.dateStart, dateEnd = c.dateEnd, items = items });
                    }
                    else
                    {
                        var existModel = result.Where(an => an.monthYear == monthYear).FirstOrDefault();
                        existModel.items.Add(new EventCalendar2Item { code = c.code, dateStart = c.dateStart, dateEnd = c.dateEnd, title = c.title });
                    }
                       
                });

                //end where by date

                return new Response { status = "S", message = "success", objectData = result };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }


        // POST /read
        [HttpPost("mark/read")]
        public ActionResult<Response> MarkRead([FromBody] EventCalendar value)
        {
            try
            {
                if (value.year == 0) {
                    return new Response { status = "E", message = "Error input year." };
                }
                var col = new Database().MongoClient<EventCalendar>( "eventCalendar");
                var filter = (Builders<EventCalendar>.Filter.Eq("status", "A") & value.filterOrganization<EventCalendar>());
                if (!string.IsNullOrEmpty(value.language)) { filter &= Builders<EventCalendar>.Filter.Eq("language", value.language); }

                var docs = col.Find(filter).Project(c => new { c.dateStart, c.dateEnd }).ToList();
                    
                var json = "";
                int year = value.year;
                string mount = "";
                string day = "";
                string doc = "";
                int dayPerMount = 0;
                int yearStart = year - 1;
                int yearEnd = year + 1;
                int[] YearDaysPerMonth = new int[] { };

                for (int j = yearStart; j <= yearEnd; j++)
                {
                    year = j;
                    bool leap = IsLeapYear(year);
                    if (leap)
                        YearDaysPerMonth = new int[] { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                    else
                        YearDaysPerMonth = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

                    for (int k = 1; k <= 12; k++)
                    {
                        dayPerMount = YearDaysPerMonth[k - 1];
                        mount = k.ToString();
                        if (mount.Length < 2)
                            mount = "0" + k;

                        for (int l = 1; l <= dayPerMount; l++)
                        {
                            day = l.ToString();
                            if (day.Length < 2)
                                day = "0" + l;

                            bool dateBetween = false;
                            string date = year + mount + day;

                            foreach (var c in docs)
                            {
                                dateBetween = Between(date.toDateFromString(), c.dateStart.toDateFromString(), c.dateEnd.toDateFromString());
                                if (dateBetween)
                                {
                                    break;
                                }
                                //do something
                            }

                            if (dateBetween)
                                json = "\"" + year + "-" + mount + "-" + day + "\":{\"marked\":true,\"selected\":false}";
                            else
                                json = "\"" + year + "-" + mount + "-" + day + "\":{\"marked\":false,\"selected\":false}";

                            if (doc == "")
                                doc = "{" + json;
                            else
                                doc = doc + "," + json;

                        }
                    }
                }

                doc += "}";

                return new Response { status = "S", message = "Success", jsonData = doc };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("mark/read2")]
        public ActionResult<Response> MarkRead2([FromBody] EventCalendar value)
        {
            try
            {
                if (value.year == 0)
                {
                    return new Response { status = "E", message = "Error input year." };
                }
                var col = new Database().MongoClient<EventCalendar>("eventCalendar");
                var filter = (Builders<EventCalendar>.Filter.Eq("status", "A"));
                if (!string.IsNullOrEmpty(value.language)) { filter &= Builders<EventCalendar>.Filter.Eq("language", value.language); }

                var docs = col.Find(filter).Project(c => new { c.dateStart, c.dateEnd, c.code, c.title, c.imageUrl }).ToList();

                //var json = "";
                var model = new List<EventCalendarMark2>();
                int year = value.year;
                string mount = "";
                string day = "";
                string doc = "";
                int dayPerMount = 0;
                int yearStart = year - 1;
                int yearEnd = year + 1;
                int[] YearDaysPerMonth = new int[] { };

                //for (int j = yearStart; j <= yearEnd; j++)
                //{
                //year = j;
                bool leap = IsLeapYear(year);
                if (leap)
                    YearDaysPerMonth = new int[] { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                else
                    YearDaysPerMonth = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

                for (int k = 1; k <= 12; k++)
                {
                    dayPerMount = YearDaysPerMonth[k - 1];
                    mount = k.ToString();
                    if (mount.Length < 2)
                        mount = "0" + k;

                    for (int l = 1; l <= dayPerMount; l++)
                    {
                        day = l.ToString();
                        if (day.Length < 2)
                            day = "0" + l;

                        bool dateBetween = false;
                        string curDate = year + mount + day;

                        model.Add(new EventCalendarMark2
                        {
                            date = curDate
                        });

                        foreach (var c in docs)
                        {
                            dateBetween = Between(curDate.toDateFromString(), c.dateStart.toDateFromString(), c.dateEnd.toDateFromString());
                            if (dateBetween)
                            {
                                model.ForEach(o =>
                                {
                                    if (o.date == curDate)
                                    {
                                        o.items.Add(new EventDataMark2
                                        {
                                            code = c.code,
                                            title = c.title,
                                            dateStart = c.dateStart,
                                            dateEnd = c.dateEnd,
                                            imageUrl = c.imageUrl
                                        });
                                    }
                                });
                            }
                            //do something
                        }

                        //if (dateBetween)
                        //    json = "\"" + year + "-" + mount + "-" + day + "\":{\"marked\":true,\"selected\":false}";
                        //else
                        //    json = "\"" + year + "-" + mount + "-" + day + "\":{\"marked\":false,\"selected\":false}";

                        //if (doc == "")
                        //    doc = "{" + json;
                        //else
                        //    doc = doc + "," + json;

                    }
                }
                //}

                //doc += "}";

                return new Response { status = "S", message = "Success", jsonData = doc, objectData = model };
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
                var col = new Database().MongoClient<Gallery>( "eventCalendarGallery");

                var filter = Builders<Gallery>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Gallery>.Filter.Regex("reference", value.code); }
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
        [HttpPost("comment/read")]
        public ActionResult<Response> CommentRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Comment>("eventCalendarComment");

                var filter = Builders<Comment>.Filter.Eq(x => x.isActive, true);
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
                        description = c.description,
                        createBy = createBy,
                        imageUrlCreateBy = imageUrlCreateBy,
                        createDate = c.createDate,
                    });
                });
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
                var word = value.description.verifyRude();

                var col = new Database().MongoClient("eventCalendarComment");

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

        #region category

        // POST /read
        [HttpPost("category/read")]
        public ActionResult<Response> CategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Category>( "eventCalendarCategory");

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
                    //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<eventCalendarCategory>.Filter.Regex("dateStart", value.startDate); }
                    //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<eventCalendarCategory>.Filter.Regex("dateEnd", value.endDate); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        #endregion

        public static bool IsLeapYear(int year)
        {
            if (year % 400 == 0)
            {
                return true;
            }
            if (year % 100 == 0)
            {
                return false;
            }
            //otherwise
            return (year % 4) == 0;
        }
        public static bool Between(DateTime input, DateTime date1, DateTime date2)
        {
            return (input >= date1 && input <= date2);
        }
    }

}