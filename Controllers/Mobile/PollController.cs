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
    public class PollController : Controller
    {
        public PollController() { }

        // POST /read
        [HttpPost("all/read")]
        public ActionResult<Response> AllRead([FromBody] Criteria value)
        {

            try
            {
                value.statisticsCreate("poll");
                var col = new Database().MongoClient<Poll>( "poll");
                var colQ = new Database().MongoClient<PollQuestion>( "pollQuestion");
                var colA = new Database().MongoClient<PollAnswer>( "pollAnswer");
                var filter = (Builders<Poll>.Filter.Eq("status", "A") & value.filterOrganization<Poll>());
                if (value.isPublic) { filter = filter & Builders<Poll>.Filter.Eq("isPublic", value.isPublic); }
                var status2 = false;

                var model = new Poll();

                if (!string.IsNullOrEmpty(value.code))
                {

                    filter = filter & Builders<Poll>.Filter.Eq("code", value.code);
                    var doc = col.Find(filter).FirstOrDefault();
                    model = doc;
                    
                    //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        var view = model.view;
                        var colUpdateView = new Database().MongoClient("poll");

                        var filterView = Builders<BsonDocument>.Filter.Eq("code", value.code);
                        var updateView = Builders<BsonDocument>.Update.Set("view", view + 1);
                        colUpdateView.UpdateOne(filterView, updateView);

                        //docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();
                    }
                    //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                    var filterQuestion = Builders<PollQuestion>.Filter.Eq("isActive", true) & Builders<PollQuestion>.Filter.Regex("reference", model.code);
                    var questionList = colQ.Find(filterQuestion).Project(c => new { c.code, c.title, c.language, c.category, c.isRequired, c.isActive }).ToList();
                    var newQuestions = new List<Questions>();

                    questionList.ForEach(c =>
                    {
                        var filterAnswer = Builders<PollAnswer>.Filter.Eq("isActive", true) & Builders<PollAnswer>.Filter.Regex("reference", c.code);
                        var answerList = colA.Find(filterAnswer).Project(c => new Answer { code = c.code, title = c.title, value = false, isActive = c.isActive }).ToList();

                        var modelQ = new Questions
                        {
                            code = c.code,
                            title = c.title,
                            reference = model.code,
                            value = false,
                            category = c.category,
                            isRequired = c.isRequired,
                            isActive = c.isActive,
                        };
                        modelQ.answers = answerList;

                        newQuestions.Add(modelQ);
                    });

                    model.questions = newQuestions;

                    // check username duplicate

                    if (!string.IsNullOrEmpty(value.username))
                    {
                        var colReply = new Database().MongoClient<PollReply>("pollReply");
                        var filterReply = Builders<PollReply>.Filter.Eq("username", value.username) & Builders<PollReply>.Filter.Eq("reference", value.code);
                        var docPollReply = colReply.Find(filterReply).FirstOrDefault();
                        if (docPollReply != null)
                            status2 = true;
                    }

                    // read createBY

                    var colUser = new Database().MongoClient<Register1>("register");
                    var filterUser = Builders<Register1>.Filter.Eq("username", model.createBy);
                    Register1 docUser = colUser.Find(filterUser).FirstOrDefault();

                    model.userList.Add(docUser);

                    // end read createBY

                    return new Response { status = "S", message = "success", jsonData = model.ToJson(), objectData = model, status2 = status2 };
                }
                else
                {
                    return new Response { status = "E", message = "code not found" };
                }
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                value.statisticsCreate("poll");
                var col = new Database().MongoClient<Poll>( "poll");
                var filter = (Builders<Poll>.Filter.Eq("status", "A") & value.filterOrganization<Poll>());

                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Poll>.Filter.Regex("code", value.code); }
                if (!string.IsNullOrEmpty(value.keySearch)) { filter = filter & Builders<Poll>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Poll>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<Poll>.Filter.Eq("status", value.status); }
                if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Poll>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Poll>.Filter.Regex("language", value.language); }
                if (value.isHighlight) { filter = filter & Builders<Poll>.Filter.Eq("isHighlight", value.isHighlight); }
                if (value.isPublic) { filter = filter & Builders<Poll>.Filter.Eq("isPublic", value.isPublic); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Poll>.Filter.Gt("docDate", ds.start) & Builders<Poll>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Poll>.Filter.Gt("docDate", ds.start) & Builders<Poll>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Poll>.Filter.Gt("docDate", de.start) & Builders<Poll>.Filter.Lt("docDate", de.end); }

                List<Poll> docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("pollCategory", "category", "code", "categoryList")
                                          .Lookup("register", "createBy", "username", "userList")
                                      .As<Poll>()
                                      .ToList();

                docs.ForEach(c => {
                    // check username duplicate

                    if (!string.IsNullOrEmpty(value.username))
                    {
                        var colReply = new Database().MongoClient<PollReply>("pollReply");
                        var filterReply = Builders<PollReply>.Filter.Eq("username", value.username) & Builders<PollReply>.Filter.Eq("reference", c.code);
                        var docPollReply = colReply.Find(filterReply).FirstOrDefault();
                        if (docPollReply != null)
                            c.status2 = true;
                    }
                });

                //var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.imageUrlCreateBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.updateBy, c.updateDate, c.sequence, c.category, c.view }).ToList();

                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient("poll");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit)
                                      .Lookup("pollCategory", "category", "code", "categoryList")
                                          .Lookup("register", "createBy", "username", "userList")
                                      .As<Poll>()
                                      .ToList();

                    //docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();
                }
                //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("question/read")]
        public ActionResult<Response> QuestionRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<PollQuestion>( "pollQuestion");

                var filter = Builders<PollQuestion>.Filter.Eq("isActive", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<PollQuestion>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.title, c.category, c.reference, c.createBy, c.createDate }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("answer/read")]
        public ActionResult<Response> AnswerRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<PollAnswer>( "pollAnswer");

                var filter = Builders<PollAnswer>.Filter.Eq("isActive", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<PollAnswer>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.title, c.reference, c.createBy, c.createDate }).ToList();

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
                var col = new Database().MongoClient<PollReply>( "replyQuestion");

                var filter = Builders<PollReply>.Filter.Eq("isActive", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<PollReply>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.title, c.answer, c.username, c.firstName, c.lastName, c.reference, c.createBy, c.createDate }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("reply/create")]
        public ActionResult<Response> PollReplyCreate([FromBody] PollReply value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "pollReply");

                if(value.answer != "false")
                {
                    //check duplicate
                    var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    if (col.Find(filter).Any())
                    {
                        return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                    }

                    doc = new BsonDocument
                {
                    { "code", value.code },
                    { "title", value.title },
                    { "username", value.username },
                    { "firstName", value.firstName },
                    { "lastName", value.lastName },
                    { "answer", value.answer },
                    { "reference", value.reference },
                    { "createBy", value.createBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                };
                    col.InsertOne(doc);
                }

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /read
        [HttpPost("category/read")]
        public ActionResult<Response> pollCategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Category>( "pollCategory");

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
                    //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<pollCategory>.Filter.Regex("dateStart", value.startDate); }
                    //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<pollCategory>.Filter.Regex("dateEnd", value.endDate); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.imageUrl, c.createBy, c.createDate, c.isActive }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }
    }
}