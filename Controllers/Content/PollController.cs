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
    public class PollController : Controller
    {
        public PollController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Poll value)
        {

            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("poll");

                //check duplicate
                value.code = "".toCode();
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                if (value.organizationMode == "auto")
                {
                    Task.Run(() => { value.organization.createQrganizationAuto(value.code, "poll"); });

                    //var og = value.organization.filterQrganizationAuto();
                    //value.lv0 = og.lv0;
                    //value.lv1 = og.lv1;
                    //value.lv2 = og.lv2;
                    //value.lv3 = og.lv3;
                    //value.lv4 = og.lv4;
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
                    { "textButton", value.textButton},
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.createBy },
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

        // POST /create
        [HttpPost("all/create")]
        public ActionResult<Response> AllCreate([FromBody] Poll value)
        {
            value.code = "".toCode();
            var codeQ = "";
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "poll");
                var colQ = new Database().MongoClient( "pollQuestion");
                var colA = new Database().MongoClient( "pollAnswer");
                var model = new Poll();

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                if (value.organizationMode == "auto")
                {
                    Task.Run(() => { value.organization.createQrganizationAuto(value.code, "poll"); });

                    //var og = value.organization.filterQrganizationAuto();
                    //value.lv0 = og.lv0;
                    //value.lv1 = og.lv1;
                    //value.lv2 = og.lv2;
                    //value.lv3 = og.lv3;
                    //value.lv4 = og.lv4;
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "title", value.title },
                    { "sequence", value.sequence },
                    { "imageUrl", value.imageUrl },
                    { "category", value.category },
                    { "language", value.language},
                    { "description", value.description},
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
                    { "isHighlight", value.isHighlight },
                    { "status", "P" },
                    { "lv0", value.lv0 },
                    { "lv1", value.lv1 },
                    { "lv2", value.lv2 },
                    { "lv3", value.lv3 },
                    { "lv4", value.lv4 },
                    { "isPublic", value.isPublic },
                };
                col.InsertOne(doc);

                value.questions.ForEach(c =>
                {
                    codeQ = "".toCode();
                    var docQ = new BsonDocument
                {
                    { "code", codeQ },
                    { "title", c.title },
                    { "category", c.category },
                    { "reference", value.code },
                    { "isRequired", c.isRequired },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                };
                    colQ.InsertOne(docQ);
                    if (c.category == "text")
                    {
                        var docA = new BsonDocument
                {
                    { "code", "".toCode() },
                    { "title", c.answers[0].title },
                    { "reference", codeQ },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                    };
                        colA.InsertOne(docA);
                    }
                    else
                    {
                        c.answers.ForEach(o =>
                        {
                            var docA = new BsonDocument
                {
                    { "code", "".toCode() },
                    { "title", o.title },
                    { "reference", codeQ },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                    };
                            colA.InsertOne(docA);
                        });
                    }
                });


                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    status = "E",
                    message = ex.Message,
                    jsonData = doc.ToJson(),
                    objectData = BsonSerializer.Deserialize<object>(doc)
                };
            }
        }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Poll>( "poll");
                var filter = (Builders<Poll>.Filter.Ne("status", "D") & value.filterOrganization<Poll>());

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Poll>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Poll>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                    if (value.permission != "all")
                        filter &= value.permission.filterPermission<Poll>("category");
                }
                else
                {

                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<Poll>.Filter.Eq("category", value.category);
                    else
                        if (value.permission != "all")
                        filter &= value.permission.filterPermission<Poll>("category");

                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Poll>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<Poll>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Poll>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Poll>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Poll>.Filter.Regex("language", value.language); }
                    if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<Poll>.Filter.Eq("sequence", sequence); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Poll>.Filter.Gt("docDate", ds.start) & Builders<Poll>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Poll>.Filter.Gt("docDate", ds.start) & Builders<Poll>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Poll>.Filter.Gt("docDate", de.start) & Builders<Poll>.Filter.Lt("docDate", de.end); }

                }

                List<Poll> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                     .Lookup("pollCategory", "category", "code", "categoryList")
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

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("all/read")]
        public ActionResult<Response> AllRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Poll>( "poll");
                var colQ = new Database().MongoClient<PollQuestion>( "pollQuestion");
                var colA = new Database().MongoClient<PollAnswer>( "pollAnswer");
                var filter = Builders<Poll>.Filter.Ne("status", "D");

                if (!string.IsNullOrEmpty(value.lv3))
                    filter &= (value.lv3.filterOrganization<Poll>("lv3"));

                if (!string.IsNullOrEmpty(value.lv2))
                    filter &= (value.lv2.filterOrganization<Poll>("lv2"));

                if (!string.IsNullOrEmpty(value.lv1))
                    filter &= (value.lv1.filterOrganization<Poll>("lv1"));

                if (!string.IsNullOrEmpty(value.lv0))
                    filter &= (value.lv0.filterOrganization<Poll>("lv0"));

                filter |= (Builders<Poll>.Filter.Eq("lv0", "") & Builders<Poll>.Filter.Eq("lv1", "") & Builders<Poll>.Filter.Eq("lv2", "") & Builders<Poll>.Filter.Eq("lv3", ""));

                var model = new Poll();

                if (!string.IsNullOrEmpty(value.code))
                {

                    filter &= Builders<Poll>.Filter.Eq("code", value.code);
                    //var doc = col.Find(filter).FirstOrDefault();
                    var doc = col.Aggregate().Match(filter)
                                         .Lookup("pollCategory", "category", "code", "categoryList")
                                         .As<Poll>()
                                         .ToList();
                    model = doc[0];

                    var filterQuestion = Builders<PollQuestion>.Filter.Eq("isActive", true) & Builders<PollQuestion>.Filter.Regex("reference", model.code);
                    var questionList = colQ.Find(filterQuestion).Project(c => new { c.code, c.title, c.category, c.isRequired, c.isActive }).ToList();
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

                    return new Response { status = "S", message = "success", jsonData = model.ToJson(), objectData = model };
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

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] Poll value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "poll");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                if (value.organizationMode == "auto")
                {
                    Task.Run(() => { value.organization.createQrganizationAuto(value.code, "poll"); });

                    //var og = value.organization.filterQrganizationAuto();
                    //value.lv0 = og.lv0;
                    //value.lv1 = og.lv1;
                    //value.lv2 = og.lv2;
                    //value.lv3 = og.lv3;
                    //value.lv4 = og.lv4;
                    //value.lv5 = og.lv5;
                }

                doc = col.Find(filter).FirstOrDefault();
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.category)) { doc["category"] = value.category; }
                if (!string.IsNullOrEmpty(value.language)) { doc["language"] = value.language; }
                if (!string.IsNullOrEmpty(value.description)) { doc["description"] = value.description; }
                if (!string.IsNullOrEmpty(value.updateBy)) { doc["updateBy"] = value.updateBy; }

                doc["fileUrl"] = value.fileUrl;
                doc["linkUrl"] = value.linkUrl;
                doc["textButton"] = value.textButton;
                doc["sequence"] = value.sequence;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
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

        // POST /update
        [HttpPost("all/update")]
        public ActionResult<Response> AllUpdate([FromBody] Poll value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "poll");
                var colQ = new Database().MongoClient( "pollQuestion");
                var colA = new Database().MongoClient( "pollAnswer");
                var model = new Poll();


                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                if (value.organizationMode == "auto")
                {
                    Task.Run(() => { value.organization.createQrganizationAuto(value.code, "poll"); });

                    //var og = value.organization.filterQrganizationAuto();
                    //value.lv0 = og.lv0;
                    //value.lv1 = og.lv1;
                    //value.lv2 = og.lv2;
                    //value.lv3 = og.lv3;
                    //value.lv4 = og.lv4;
                }

                doc = col.Find(filter).FirstOrDefault();
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.category)) { doc["category"] = value.category; }
                if (!string.IsNullOrEmpty(value.language)) { doc["language"] = value.language; }
                if (!string.IsNullOrEmpty(value.description)) { doc["description"] = value.description; }
                if (!string.IsNullOrEmpty(value.linkUrl)) { doc["linkUrl"] = value.linkUrl; }
                if (!string.IsNullOrEmpty(value.textButton)) { doc["textButton"] = value.textButton; }
                if (!string.IsNullOrEmpty(value.updateBy)) { doc["updateBy"] = value.updateBy; }

                doc["fileUrl"] = value.fileUrl;
                doc["linkUrl"] = value.linkUrl;
                doc["textButton"] = value.textButton;
                doc["sequence"] = value.sequence;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["isActive"] = value.isActive;
                doc["isHighlight"] = value.isHighlight;
                doc["status"] = value.organizationMode == "auto" ? "P" : value.isActive ? "A" : "N";
                doc["lv0"] = value.lv0;
                doc["lv1"] = value.lv1;
                doc["lv2"] = value.lv2;
                doc["lv3"] = value.lv3;
                doc["lv4"] = value.lv4;
                doc["isPublic"] = value.isPublic;

                col.ReplaceOne(filter, doc);


                // 1 . ?????????????
                //   1.1 ????????? text
                //       - ?? ????????????????
                //       - generate ????? ????
                //   1.2 ????? ???? single, etc.
                //      - ????????? = create 
                //      - ????????? = update 
                // 2. ??????????
                //      - create all



                value.questions.ForEach(c =>
                {
                    var codeC = "".toCode();
                    var docQ = new BsonDocument();
                    var docA = new BsonDocument();
                    // update Q
                    if (!string.IsNullOrEmpty(c.code))
                    {
                        codeC = c.code;
                        docQ = new BsonDocument();
                        var filterQuestion = Builders<BsonDocument>.Filter.Eq("code", c.code);
                        docQ = colQ.Find(filterQuestion).FirstOrDefault();
                        if (!string.IsNullOrEmpty(value.title)) { docQ["title"] = c.title; }
                        if (!string.IsNullOrEmpty(value.category)) { docQ["category"] = c.category; }

                        docQ["isRequired"] = c.isRequired;
                        docQ["isActive"] = c.isActive;
                        docQ["updateBy"] = value.updateBy;
                        docQ["updateDate"] = DateTime.Now.toStringFromDate();
                        colQ.ReplaceOne(filterQuestion, docQ);

                        // check Q text
                        if (c.category == "text")
                        {
                            docA = new BsonDocument();
                            var filterAnswer = Builders<BsonDocument>.Filter.Eq("reference", c.code);
                            filterAnswer = filterAnswer & Builders<BsonDocument>.Filter.Eq("isActive", true);
                            var update = Builders<BsonDocument>.Update.Set("isActive", false);
                            colA.UpdateMany(filterAnswer, update);

                            docA = new BsonDocument
                {
                    { "code", "".toCode() },
                    { "title", c.answers[0].title },
                    { "reference", codeC },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                    };
                            colA.InsertOne(docA);
                        }
                        else
                        {
                            if (c.answers.Count > 0)
                            {
                                docA = new BsonDocument();
                                c.answers.ForEach(o =>
                                {
                                    if (!string.IsNullOrEmpty(o.code))
                                    {
                                        var filterAnswer = Builders<BsonDocument>.Filter.Eq("code", o.code);
                                        docA = colA.Find(filterAnswer).FirstOrDefault();
                                        if (!string.IsNullOrEmpty(o.title)) { docA["title"] = o.title; }

                                        docA["isActive"] = o.isActive;
                                        docA["updateBy"] = value.updateBy;
                                        docA["updateDate"] = DateTime.Now.toStringFromDate();
                                        colA.ReplaceOne(filterAnswer, docA);
                                    }
                                    else
                                    {
                                        docA = new BsonDocument
                            {
                    { "code", "".toCode() },
                    { "title", o.title },
                    { "reference", codeC },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                };
                                        colA.InsertOne(docA);
                                    }
                                });
                            }
                        }
                    }
                    else
                    {
                        var codeNewQuestion = "".toCode();
                        docQ = new BsonDocument
                        {
                    { "code", codeNewQuestion },
                    { "title", c.title },
                    { "category", c.category },
                    { "reference", value.code },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                };
                        colQ.InsertOne(docQ);
                        c.answers.ForEach(o =>
                        {
                            docA = new BsonDocument
                            {
                    { "code", "".toCode() },
                    { "title", o.title },
                    { "reference", codeNewQuestion },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                };
                            colA.InsertOne(docA);
                        });
                    }
                });

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] Poll value)
        {
            try
            {
                var col = new Database().MongoClient( "poll");

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

        #region question

        // POST /create
        [HttpPost("question/create")]
        public ActionResult<Response> QuestionCreate([FromBody] PollQuestion value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "pollQuestion");

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
                    { "category", value.category },
                    { "reference", value.reference },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
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

        // POST /update
        [HttpPost("question/delete")]
        public ActionResult<Response> QuestionUpdate([FromBody] Poll value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "pollQuestion");

                var filter = Builders<BsonDocument>.Filter.Eq("reference", value.code);
                var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
                col.UpdateMany(filter, update);

                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        #region answer

        // POST /create
        [HttpPost("answer/create")]
        public ActionResult<Response> AnswerCreate([FromBody] PollAnswer value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "pollAnswer");

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
                    { "reference", value.reference },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
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

        #endregion

        #region reply

        // POST /create
        [HttpPost("reply/read")]
        public ActionResult<Response> ReplyRead([FromBody] PollReply value)
        {
            try
            {
                var col = new Database().MongoClient<PollReply>( "pollReply");

                var filter = Builders<PollReply>.Filter.Eq("isActive", true);
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<PollReply>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<PollReply>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<PollReply>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<PollReply>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.answer)) { filter = filter & Builders<PollReply>.Filter.Regex("answer", new BsonRegularExpression(string.Format(".*{0}.*", value.answer), "i")); }
                    if (!string.IsNullOrEmpty(value.username)) { filter = filter & Builders<PollReply>.Filter.Regex("username", value.username); }
                    if (!string.IsNullOrEmpty(value.reference)) { filter = filter & Builders<PollReply>.Filter.Regex("reference", value.reference); }
                    if (!string.IsNullOrEmpty(value.firstName)) { filter = filter & Builders<PollReply>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.firstName), "i")); }
                    if (!string.IsNullOrEmpty(value.lastName)) { filter = filter & Builders<PollReply>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.lastName), "i")); }
                }

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).ThenByDescending(o => o.updateTime).Project(c => new { c.code, c.isActive, c.title, c.answer, c.username, c.firstName, c.lastName, c.reference, c.createBy, c.createDate, c.updateBy, c.updateDate }).ToList();


                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
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
                var col = new Database().MongoClient( "pollCategory");

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
                    { "title", value.title },
                    { "language", value.language },
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
                var col = new Database().MongoClient<Category>( "pollCategory");

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

                var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive, c.updateDate, c.updateBy, c.sequence }).ToList();

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
                var col = new Database().MongoClient( "pollCategory");

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
                    var collectionContent = new Database().MongoClient("poll");
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
                    var updatePermission = Builders<BsonDocument>.Update.Set("pollPage", false).Set("isActive", false);
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
                var col = new Database().MongoClient( "pollCategory");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
                col.UpdateOne(filter, update);

                // ------- update content ------
                if (!value.isActive)
                {
                    var collectionContent = new Database().MongoClient("poll");
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
                    var updatePermission = Builders<BsonDocument>.Update.Set("pollPage", false).Set("isActive", false);
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
                var col = new Database().MongoClient<BsonDocument>("poll");
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