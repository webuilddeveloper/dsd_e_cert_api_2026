using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cms_api.Extension;
using cms_api.Models;
using Jose;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class RegisterController : Controller
    {
        public RegisterController() { }

        #region login

        // POST /login
        [HttpPost("login")]
        public ActionResult<Response> Login([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");

                var filter = Builders<Register>.Filter.Eq(x => x.isActive, true) & Builders<Register>.Filter.Ne(x => x.status, "D");
                filter = filter & Builders<Register>.Filter.Eq("username", value.username);
                filter = filter & Builders<Register>.Filter.Eq("password", value.password);

                var doc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl }).FirstOrDefault();

                if (doc != null)
                {
                    value.code = value.code.toCode();
                    var token = $"{doc.username}|{doc.password}|{doc.category}|{value.code}".toEncode();

                    //BEGIN =disable session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    {
                        var colSession = new Database().MongoClient<Register>("registerSession");
                        var filterSession = Builders<Register>.Filter.Eq("username", value.username);
                        filterSession = filterSession & Builders<Register>.Filter.Eq("isActive", true);

                        //get last session
                        var docSession = colSession.Find(filterSession).Project(c => new { c.token }).FirstOrDefault();

                        //update last session
                        var updateSession = Builders<Register>.Update.Set("isActive", false).Set("updateBy", "system").Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                        colSession.UpdateMany(filterSession, updateSession);

                        //set activity
                        if (docSession != null)
                        {
                            {
                                var colActivity = new Database().MongoClient<Register>("registerActivity");

                                //update last activity
                                var updateActivity = Builders<Register>.Update.Set("isActive", false).Set("updateBy", "system").Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                                colActivity.UpdateMany(filterSession, updateActivity);

                            }

                            {
                                var colActivity = new Database().MongoClient("registerActivity");

                                var docActivity = new BsonDocument
                                {
                                    { "token", docSession.token },
                                    { "code", value.code },
                                    { "username", value.username },
                                    { "description", "ออกจากระบบเนื่องจากมีการเข้าใช้งานจากที่อื่น" },
                                    { "createBy", "system" },
                                    { "createDate", DateTime.Now.toStringFromDate() },
                                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                                    { "updateBy", "system" },
                                    { "updateDate", DateTime.Now.toStringFromDate() },
                                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                                    { "docDate", DateTime.Now.Date.AddHours(7) },
                                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                                    { "isActive", false }
                                };
                                colActivity.InsertOne(docActivity);
                            }

                        }
                    }

                    //END =disable seesion <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<





                    //BEGIN =create session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    {
                        var colSession = new Database().MongoClient("registerSession");
                        var docSession = new BsonDocument
                        {
                            { "token", token },
                            { "code", value.code },
                            { "username", value.username },
                            { "createBy", "system" },
                            { "createDate", DateTime.Now.toStringFromDate() },
                            { "createTime", DateTime.Now.toTimeStringFromDate() },
                            { "updateBy", "system" },
                            { "updateDate", DateTime.Now.toStringFromDate() },
                            { "updateTime", DateTime.Now.toTimeStringFromDate() },
                            { "docDate", DateTime.Now.Date.AddHours(7) },
                            { "docTime", DateTime.Now.toTimeStringFromDate() },
                            { "isActive", true }
                        };
                        colSession.InsertOne(docSession);
                    }

                    //END =create session <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<





                    //BEGIN =create activity >>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    {
                        var colActivity = new Database().MongoClient("registerActivity");
                        var docActivity = new BsonDocument
                        {
                            { "token", token },
                            { "code", value.code },
                            { "username", value.username },
                            { "description", "เข้าใช้งานระบบ" },
                            { "createBy", "system" },
                            { "createDate", DateTime.Now.toStringFromDate() },
                            { "createTime", DateTime.Now.toTimeStringFromDate() },
                            { "updateBy", "system" },
                            { "updateDate", DateTime.Now.toStringFromDate() },
                            { "updateTime", DateTime.Now.toTimeStringFromDate() },
                            { "docDate", DateTime.Now.Date.AddHours(7) },
                            { "docTime", DateTime.Now.toTimeStringFromDate() },
                            { "isActive", true }
                        };
                        colActivity.InsertOne(docActivity);
                    }

                    //END =create activity <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                    return new Response { status = "S", message = "success", jsonData = token, objectData = new { code = doc.code, username = value.username, category = doc.category, imageUrl = doc.imageUrl } };
                }
                else
                {
                    return new Response { status = "F", message = "login failed", jsonData = "", objectData = "" };
                }

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Register value)
        {
            value.code = value.code.toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("register");

                {
                    //check duplicate
                    var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    if (col.Find(filter).Any())
                    {
                        return new Response { status = "E", message = $"code= {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                    }
                }

                {
                    //check duplicate
                    var filter = Builders<BsonDocument>.Filter.Eq("username", value.username) & Builders<BsonDocument>.Filter.Ne("status", "D");
                    if (col.Find(filter).Any())
                    {
                        return new Response { status = "E", message = $"username= {value.username} is exist", jsonData = value.ToJson(), objectData = value };
                    }
                }

                //{
                //    //check duplicate
                //    var filter = Builders<BsonDocument>.Filter.Eq("idcard", value.idcard) & Builders<BsonDocument>.Filter.Ne("status", "D");
                //    if (col.Find(filter).Any())
                //    {
                //        return new Response { status = "E", message = $"idcard= {value.idcard} is exist", jsonData = value.ToJson(), objectData = value };
                //    }
                //}


                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "imageUrl", value.imageUrl },
                    { "category", value.category },
                    { "username", value.username },
                    { "password", value.password },
                    { "prefixName", value.prefixName },
                    { "firstName", value.firstName },
                    { "lastName", value.lastName },
                    { "birthDay", value.birthDay },
                    { "phone", value.phone },
                    { "email", value.email },
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
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne("status", "D")
                                        & (Builders<Register>.Filter.Eq(x => x.category, ""));
                                        //& (Builders<Register>.Filter.Ne(x => x.category, "guest")) & (Builders<Register>.Filter.Ne(x => x.category, "facebook")) & (Builders<Register>.Filter.Ne(x => x.category, "google")) & (Builders<Register>.Filter.Ne(x => x.category, "line") | Builders<Register>.Filter.Ne(x => x.category, "apple"));
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {

                    if (!string.IsNullOrEmpty(value.username)) { filter = filter & Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.username), "i")); }
                    if (!string.IsNullOrEmpty(value.password)) { filter = filter & Builders<Register>.Filter.Regex("password", value.password); }
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Register>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Register>.Filter.Regex("category", value.category); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Register>.Filter.Regex("createBy", new BsonRegularExpression(string.Format(".*{0}.*", value.createBy), "i")); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", de.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.imageUrl, c.updateBy, c.updateDate, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.username, c.password, c.prefixName, c.firstName, c.lastName, c.phone, c.email, c.birthDay }).ToList();

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
        public ActionResult<Response> Update([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "register");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.password)) { doc["password"] = value.password; }
                if (!string.IsNullOrEmpty(value.category)) { doc["category"] = value.category; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.prefixName)) { doc["prefixName"] = value.prefixName; }
                if (!string.IsNullOrEmpty(value.firstName)) { doc["firstName"] = value.firstName; }
                if (!string.IsNullOrEmpty(value.lastName)) { doc["lastName"] = value.lastName; }
                if (!string.IsNullOrEmpty(value.phone)) { doc["phone"] = value.phone; }
                if (!string.IsNullOrEmpty(value.email)) { doc["email"] = value.email; }
                if (!string.IsNullOrEmpty(value.firstName)) { doc["birthDay"] = value.birthDay; }

                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["isActive"] = value.isActive;
                doc["status"] = value.isActive ? "A" : "N";
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient( "register");

                var codeList = value.code.Split(",");

                foreach (var code in codeList)
                {

                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                    col.UpdateOne(filter, update);

                }

                //var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                //var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                //col.UpdateOne(filter, update);
                return new Response { status = "S", message = $"code= {value.code} is delete" };
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
        public ActionResult<Response> CategoryCreate([FromBody] RegisterCategory value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("registerCategory");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code= {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "title", value.title },
                    { "createAction", value.createAction },
                    { "readAction", value.readAction },
                    { "updateAction", value.updateAction },
                    { "deleteAction", value.deleteAction },
                    { "approveAction", value.approveAction },

                    { "lv0", value.lv0 },
                    { "lv1", value.lv1 },
                    { "lv2", value.lv2 },
                    { "lv3", value.lv3 },
                    { "lv4", value.lv4 },
                    { "lv5", value.lv5 },

                    { "organizationPage", value.organizationPage },
                    { "userRolePage", value.userRolePage },
                    { "memberPage", value.memberPage },
                    { "memberMobilePage", value.memberMobilePage },

                    { "logoPage", value.logoPage },
                    { "splashPage", value.splashPage },
                    { "mainPopupPage", value.mainPopupPage },
                    { "bannerPage", value.bannerPage },
                    { "forceAdsPage", value.forceAdsPage },
                    { "rotationPage", value.rotationPage },

                    { "newsPage", value.newsPage },
                    { "eventPage", value.eventPage },
                    { "contactPage", value.contactPage },
                    { "knowledgePage", value.knowledgePage },
                    { "privilegePage", value.privilegePage },
                    { "poiPage", value.poiPage },
                    { "pollPage", value.pollPage },
                    { "suggestionPage", value.suggestionPage },
                    { "notificationPage", value.notificationPage },
                    { "welfarePage", value.welfarePage },
                    { "trainingPage", value.trainingPage },
                    { "skilledLaborPage", value.skilledLaborPage },
                    { "reporterPage", value.reporterPage },
                    { "warningPage", value.warningPage },
                    { "fundPage", value.fundPage },
                    { "cooperativeFormPage", value.cooperativeFormPage },

                    { "newsCategoryPage", value.newsCategoryPage },
                    { "eventCategoryPage", value.eventCategoryPage },
                    { "contactCategoryPage", value.contactCategoryPage },
                    { "knowledgeCategoryPage", value.knowledgeCategoryPage },
                    { "privilegeCategoryPage", value.privilegeCategoryPage },
                    { "poiCategoryPage", value.poiCategoryPage },
                    { "pollCategoryPage", value.pollCategoryPage },
                    { "suggestionCategoryPage", value.suggestionCategoryPage },
                    { "notificationCategoryPage", value.notificationCategoryPage },
                    { "welfareCategoryPage", value.welfareCategoryPage },
                    { "trainingCategoryPage", value.trainingCategoryPage },
                    { "skilledLaborCategoryPage", value.skilledLaborCategoryPage },
                    { "reporterCategoryPage", value.reporterCategoryPage },
                    { "warningCategoryPage", value.warningCategoryPage },
                    { "fundCategoryPage", value.fundCategoryPage },
                    { "cooperativeFormCategoryPage", value.cooperativeFormCategoryPage },

                    { "policyApplicationPage", value.policyApplicationPage },
                    { "policyMarketingPage", value.policyMarketingPage },
                    { "memberMobilePolicyApplicationPage", value.memberMobilePolicyApplicationPage },
                    { "memberMobilePolicyMarketingPage", value.memberMobilePolicyMarketingPage },
                    //report
                    { "reportNumberMemberRegisterPage", value.reportNumberMemberRegisterPage },
                    { "reportMemberRegisterPage", value.reportMemberRegisterPage },
                    { "reportNewsCategoryPage", value.reportNewsCategoryPage },
                    { "reportNewsPage", value.reportNewsPage },
                    { "reportKnowledgeCategoryPage", value.reportKnowledgeCategoryPage },
                    { "reportKnowledgePage", value.reportKnowledgePage },
                    { "reportNewsKeysearchPage", value.reportNewsKeysearchPage },
                    { "reportKnowledgeKeysearchPage", value.reportKnowledgeKeysearchPage },
                    //Master
                    { "swearWordsPage", value.swearWordsPage },

                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                    { "status", "A" }
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
                var col = new Database().MongoClient<RegisterCategory>( "registerCategory");

                var filter = Builders<RegisterCategory>.Filter.Ne(x => x.status, "D");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<RegisterCategory>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<RegisterCategory>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                    //filter = (filter & Builders<RegisterCategory>.Filter.Regex("title", value.keySearch)) | (filter & Builders<RegisterCategory>.Filter.Regex("description", value.keySearch));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<RegisterCategory>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<RegisterCategory>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    //if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<RegisterCategory>.Filter.Regex("description", value.description); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<RegisterCategory>.Filter.Gt("docDate", ds.start) & Builders<RegisterCategory>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<RegisterCategory>.Filter.Gt("docDate", ds.start) & Builders<RegisterCategory>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<RegisterCategory>.Filter.Gt("docDate", de.start) & Builders<RegisterCategory>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new
                {
                    c.code,
                    c.title,
                    c.createBy,
                    c.createDate,
                    c.isActive,
                    c.updateDate,
                    c.updateBy,

                    c.createAction,
                    c.readAction,
                    c.updateAction,
                    c.deleteAction,
                    c.approveAction,

                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,
                    c.lv5,

                    c.organizationPage,
                    c.userRolePage,
                    c.memberPage,
                    c.memberMobilePage,

                    c.logoPage,
                    c.splashPage,
                    c.mainPopupPage,
                    c.bannerPage,
                    c.forceAdsPage,
                    c.rotationPage,

                    c.newsPage,
                    c.eventPage,
                    c.contactPage,
                    c.knowledgePage,
                    c.privilegePage,
                    c.poiPage,
                    c.pollPage,
                    c.suggestionPage,
                    c.notificationPage,
                    c.welfarePage,
                    c.trainingPage,
                    c.skilledLaborPage,
                    c.reporterPage,
                    c.warningPage,
                    c.fundPage,
                    c.cooperativeFormPage,

                    c.policyApplicationPage,
                    c.policyMarketingPage,
                    c.memberMobilePolicyApplicationPage,
                    c.memberMobilePolicyMarketingPage,
                    //report
                    c.reportNumberMemberRegisterPage,
                    c.reportMemberRegisterPage,
                    c.reportNewsCategoryPage,
                    c.reportNewsPage,
                    c.reportKnowledgeCategoryPage,
                    c.reportKnowledgePage,
                    c.reportNewsKeysearchPage,
                    c.reportKnowledgeKeysearchPage,
                    //Master
                    c.swearWordsPage,

                    c.newsCategoryPage,
                    c.eventCategoryPage,
                    c.contactCategoryPage,
                    c.knowledgeCategoryPage,
                    c.privilegeCategoryPage,
                    c.poiCategoryPage,
                    c.pollCategoryPage,
                    c.suggestionCategoryPage,
                    c.notificationCategoryPage,
                    c.welfareCategoryPage,
                    c.trainingCategoryPage,
                    c.skilledLaborCategoryPage,
                    c.reporterCategoryPage,
                    c.warningCategoryPage,
                    c.fundCategoryPage,
                    c.cooperativeFormCategoryPage
                }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /update
        [HttpPost("category/update")]
        public ActionResult<Response> CategoryUpdate([FromBody] RegisterCategory value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "registerCategory");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }

                doc["createAction"] = value.createAction;
                doc["readAction"] = value.readAction;
                doc["updateAction"] = value.updateAction;
                doc["deleteAction"] = value.deleteAction;
                doc["approveAction"] = value.approveAction;

                doc["lv0"] = value.lv0;
                doc["lv1"] = value.lv1;
                doc["lv2"] = value.lv2;
                doc["lv3"] = value.lv3;
                doc["lv4"] = value.lv4;
                doc["lv5"] = value.lv5;

                doc["organizationPage"] = value.organizationPage;
                doc["userRolePage"] = value.userRolePage;
                doc["memberPage"] = value.memberPage;
                doc["memberMobilePage"] = value.memberMobilePage;

                doc["logoPage"] = value.logoPage;
                doc["splashPage"] = value.splashPage;
                doc["mainPopupPage"] = value.mainPopupPage;
                doc["bannerPage"] = value.bannerPage;
                doc["forceAdsPage"] = value.forceAdsPage;
                doc["rotationPage"] = value.rotationPage;

                doc["newsPage"] = value.newsPage;
                doc["eventPage"] = value.eventPage;
                doc["contactPage"] = value.contactPage;
                doc["knowledgePage"] = value.knowledgePage;
                doc["privilegePage"] = value.privilegePage;
                doc["poiPage"] = value.poiPage;
                doc["pollPage"] = value.pollPage;
                doc["suggestionPage"] = value.suggestionPage;
                doc["notificationPage"] = value.notificationPage;
                doc["welfarePage"] = value.welfarePage;
                doc["trainingPage"] = value.trainingPage;
                doc["skilledLaborPage"] = value.skilledLaborPage;
                doc["reporterPage"] = value.reporterPage; 
                doc["warningPage"] = value.warningPage;
                doc["fundPage"] = value.fundPage;
                doc["cooperativeFormPage"] = value.cooperativeFormPage;

                doc["policyApplicationPage"] = value.policyApplicationPage;
                doc["policyMarketingPage"] = value.policyMarketingPage;
                doc["memberMobilePolicyApplicationPage"] = value.memberMobilePolicyApplicationPage;
                doc["memberMobilePolicyMarketingPage"] = value.memberMobilePolicyMarketingPage;
                //report
                doc["reportNumberMemberRegisterPage"] = value.reportNumberMemberRegisterPage;
                doc["reportMemberRegisterPage"] = value.reportMemberRegisterPage;
                doc["reportNewsCategoryPage"] = value.reportNewsCategoryPage;
                doc["reportNewsPage"] = value.reportNewsPage;
                doc["reportKnowledgeCategoryPage"] = value.reportKnowledgeCategoryPage;
                doc["reportKnowledgePage"] = value.reportKnowledgePage;
                doc["reportNewsKeysearchPage"] = value.reportNewsKeysearchPage;
                doc["reportKnowledgeKeysearchPage"] = value.reportKnowledgeKeysearchPage;
                //Master
                doc["swearWordsPage"] = value.swearWordsPage;

                doc["newsCategoryPage"] = value.newsCategoryPage;
                doc["eventCategoryPage"] = value.eventCategoryPage;
                doc["contactCategoryPage"] = value.contactCategoryPage;
                doc["knowledgeCategoryPage"] = value.knowledgeCategoryPage;
                doc["privilegeCategoryPage"] = value.privilegeCategoryPage;
                doc["poiCategoryPage"] = value.poiCategoryPage;
                doc["pollCategoryPage"] = value.pollCategoryPage;
                doc["suggestionCategoryPage"] = value.suggestionCategoryPage;
                doc["notificationCategoryPage"] = value.notificationCategoryPage;
                doc["welfareCategoryPage"] = value.welfareCategoryPage;
                doc["trainingCategoryPage"] = value.trainingCategoryPage;
                doc["skilledLaborCategoryPage"] = value.skilledLaborCategoryPage;
                doc["reporterCategoryPage"] = value.reporterCategoryPage;
                doc["warningCategoryPage"] = value.warningCategoryPage;
                doc["fundCategoryPage"] = value.fundCategoryPage;
                doc["cooperativeFormCategoryPage"] = value.cooperativeFormCategoryPage;

                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = true;
                doc["status"] = "A";
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("category/delete")]
        public ActionResult<Response> CategoryDelete([FromBody] Category value)
        {
            try
            {
                var col = new Database().MongoClient("registerCategory");

                var codeList = value.code.Split(",");

                foreach (var code in codeList)
                {

                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                    col.UpdateOne(filter, update);

                }

                //var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                //var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate().Set("updateTime", DateTime.Now.toTimeStringFromDate());
                //col.UpdateOne(filter, update);
                return new Response { status = "S", message = $"code= {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        #region permission (item ของ category)

        // POST /create
        [HttpPost("permission/create")]
        public ActionResult<Response> PermissionCreate([FromBody] Permission value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "registerPermission");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code= {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "page", value.page },
                    { "category", value.category },
                    { "reference", value.reference },

                    { "newsPage", value.newsPage },
                    { "eventPage", value.eventPage },
                    { "contactPage", value.contactPage },
                    { "knowledgePage", value.knowledgePage },
                    { "privilegePage", value.privilegePage },
                    { "poiPage", value.poiPage },
                    { "pollPage", value.pollPage },
                    { "suggestionPage", value.suggestionPage },
                    { "notificationPage", value.notificationPage },
                    { "welfarePage", value.welfarePage },
                    { "trainingPage", value.trainingPage },
                    { "skilledLaborPage", value.skilledLaborPage },
                    { "reporterPage", value.reporterPage },
                    { "warningPage", value.warningPage },
                    { "fundPage", value.fundPage },
                    { "cooperativeFormPage", value.cooperativeFormPage },

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

        // POST /read
        [HttpPost("permission/read")]
        public ActionResult<Response> PermissionRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Permission>( "registerPermission");

                var filter = Builders<Permission>.Filter.Eq(x => x.isActive, true);

                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Permission>.Filter.Eq("reference", value.code); }

                //var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.page, c.category, c.newsPage, c.eventPage, c.contactPage, c.knowledgePage, c.privilegePage, c.poiPage, c.pollPage, c.suggestionPage, c.notificationPage }).ToList();
                List<Permission> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit)
                                     .Lookup("newsCategory", "category", "code", "newsCategoryList")
                                     .Lookup("eventCalendarCategory", "category", "code", "eventCategoryList")
                                     .Lookup("contactCategory", "category", "code", "contactCategoryList")
                                     .Lookup("knowledgeCategory", "category", "code", "knowledgeCategoryList")
                                     .Lookup("privilegeCategory", "category", "code", "privilegeCategoryList")
                                     .Lookup("poiCategory", "category", "code", "poiCategoryList")
                                     .Lookup("pollCategory", "category", "code", "pollCategoryList")
                                     .Lookup("suggestionCategory", "category", "code", "suggestionCategoryList")
                                     .Lookup("notificationCategory", "category", "code", "notificationCategoryList")
                                     .Lookup("welfareCategory", "category", "code", "welfareCategoryList")
                                     .Lookup("trainingCategory", "category", "code", "trainingCategoryList")
                                     .Lookup("skilledLaborCategory", "category", "code", "skilledLaborCategory")
                                     .Lookup("reporterCategory", "category", "code", "reporterCategoryList")
                                     .Lookup("warningCategory", "category", "code", "warningCategoryList")
                                     .Lookup("fundCategory", "category", "code", "fundCategoryList")
                                     .Lookup("cooperativeFormCategory", "category", "code", "cooperativeFormCategoryList")
                                     .As<Permission>()
                                     .ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /create
        [HttpPost("permission/delete")]
        public ActionResult<Response> PermissionDelete([FromBody] Permission value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "registerPermission");

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

        #region role (register + category)

        // POST /create
        [HttpPost("role/create")]
        public ActionResult<Response> RoleCreate([FromBody] Register value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("registerRole");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code= {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "username", value.username },
                    { "category", value.category },

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

        // POST /read
        [HttpPost("role/read")]
        public ActionResult<Response> RoleRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>( "registerRole");

                var filter = Builders<Register>.Filter.Eq(x => x.isActive, true);

                if (!string.IsNullOrEmpty(value.username)) { filter &= Builders<Register>.Filter.Eq("username", value.username); }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.username, c.category }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /create
        [HttpPost("role/delete")]
        public ActionResult<Response> RoleDelete([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "registerRole");

                {
                    //disable all
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("username", value.username);
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

        #region authentication

        // POST /create
        [HttpPost("menu/create")]
        public ActionResult<Response> menuCreate([FromBody] Register value)
        {
            value.code = value.code.toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("mMenu");

                {
                    //check duplicate
                    var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    if (col.Find(filter).Any())
                    {
                        return new Response { status = "E", message = $"code= {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                    }
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "category", value.category },
                    { "sequence", value.sequence },
                    { "title", value.title },
                    { "routing", value.codeShort },
                    { "isShow", false },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", false },
                    { "status", "N" }
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
        [HttpPost("menu/read")]
        public ActionResult<Response> menuRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("mMenu");
                var filter = Builders<Register>.Filter.Ne("status", "D");
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Register>.Filter.Eq("category", value.category); }
                var docs = col.Find(filter).SortBy(o => o.sequence).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.imageUrl, c.updateBy, c.updateDate, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.username, c.password, c.prefixName, c.firstName, c.lastName, c.phone, c.email, c.birthDay }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read ระดับการเข้าถึงเมนู
        [HttpPost("system/read")]
        public ActionResult<Response> systemRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("registerRole");

                var filter = Builders<Register>.Filter.Eq(x => x.isActive, true);
                filter &= Builders<Register>.Filter.Eq("username", value.username);

                var docs = col.Find(filter).SortByDescending(o => o.docDate).Project(c => new { c.category }).ToList();

                var category = new RegisterCategory
                {
                    createAction = false,
                    readAction = false,
                    updateAction = false,
                    deleteAction = false,
                    approveAction = false,

                    lv0 = "",
                    lv1 = "",
                    lv2 = "",
                    lv3 = "",
                    lv4 = "",
                    lv5 = "",

                    organizationPage = false,
                    userRolePage = false,
                    memberPage = false,
                    memberMobilePage = false,

                    logoPage = false,
                    splashPage = false,
                    mainPopupPage = false,
                    bannerPage = false,
                    forceAdsPage = false,
                    rotationPage = false,

                    newsPage = false,
                    eventPage = false,
                    contactPage = false,
                    knowledgePage = false,
                    privilegePage = false,
                    poiPage = false,
                    pollPage = false,
                    suggestionPage = false,
                    notificationPage = false,
                    welfarePage = false,
                    trainingPage = false,
                    skilledLaborPage = false,
                    reporterPage = false,
                    warningPage = false,
                    fundPage = false,
                    cooperativeFormPage = false,

                    newsCategoryPage = false,
                    eventCategoryPage = false,
                    contactCategoryPage = false,
                    knowledgeCategoryPage = false,
                    privilegeCategoryPage = false,
                    poiCategoryPage = false,
                    pollCategoryPage = false,
                    suggestionCategoryPage = false,
                    notificationCategoryPage = false,
                    welfareCategoryPage = false,
                    trainingCategoryPage = false,
                    skilledLaborCategoryPage = false,
                    reporterCategoryPage = false,
                    warningCategoryPage = false,
                    fundCategoryPage = false,
                    cooperativeFormCategoryPage = false,

                    policyApplicationPage = false,
                    policyMarketingPage = false,
                    memberMobilePolicyApplicationPage = false,
                    memberMobilePolicyMarketingPage = false,
                    //report
                    reportNumberMemberRegisterPage = false,
                    reportMemberRegisterPage = false,
                    reportNewsCategoryPage = false,
                    reportNewsPage = false,
                    reportKnowledgeCategoryPage = false,
                    reportKnowledgePage = false,
                    reportNewsKeysearchPage = false,
                    reportKnowledgeKeysearchPage = false,
                    //Master
                    swearWordsPage = false,
            };

                docs.ForEach(c =>
                {
                    var CategoryCol = new Database().MongoClient<RegisterCategory>( "registerCategory");

                    var CategoryFilter = Builders<RegisterCategory>.Filter.Eq(x => x.status, "A");
                    CategoryFilter &= Builders<RegisterCategory>.Filter.Eq("title", c.category);

                    var CategoryDoc = CategoryCol.Find(CategoryFilter).Project(c => new { 
                        c.createAction, 
                        c.readAction, 
                        c.updateAction, 
                        c.deleteAction, 
                        c.approveAction, 
                        c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv5, 
                        c.organizationPage, 
                        c.userRolePage, 
                        c.memberPage, 
                        c.memberMobilePage, 
                        c.logoPage, 
                        c.splashPage, 
                        c.mainPopupPage, 
                        c.bannerPage, 
                        c.forceAdsPage, 
                        c.rotationPage, 
                        c.newsPage, 
                        c.eventPage, 
                        c.contactPage, 
                        c.knowledgePage, 
                        c.privilegePage, 
                        c.poiPage, 
                        c.pollPage, 
                        c.suggestionPage, 
                        c.notificationPage, 
                        c.reporterPage, 
                        c.welfarePage, 
                        c.trainingPage,
                        c.skilledLaborPage,
                        c.warningPage, 
                        c.newsCategoryPage, 
                        c.eventCategoryPage, 
                        c.contactCategoryPage, 
                        c.knowledgeCategoryPage, 
                        c.privilegeCategoryPage, 
                        c.poiCategoryPage, 
                        c.pollCategoryPage, 
                        c.suggestionCategoryPage, 
                        c.notificationCategoryPage, 
                        c.welfareCategoryPage, 
                        c.reporterCategoryPage, 
                        c.trainingCategoryPage,
                        c.skilledLaborCategoryPage,
                        c.warningCategoryPage, 
                        c.policyMarketingPage ,
                        c.policyApplicationPage, 
                        c.memberMobilePolicyApplicationPage, 
                        c.memberMobilePolicyMarketingPage, 
                        c.fundPage, 
                        c.fundCategoryPage, 
                        c.cooperativeFormPage, 
                        c.cooperativeFormCategoryPage,
                        //report
                        c.reportNumberMemberRegisterPage,
                        c.reportMemberRegisterPage,
                        c.reportNewsCategoryPage,
                        c.reportNewsPage,
                        c.reportKnowledgeCategoryPage,
                        c.reportKnowledgePage,
                        c.reportNewsKeysearchPage,
                        c.reportKnowledgeKeysearchPage,
                        //Master
                        c.swearWordsPage,
                    }).FirstOrDefault();

                    if (CategoryDoc != null)
                    {
                        if (CategoryDoc.createAction) { category.createAction = CategoryDoc.createAction; };
                        if (CategoryDoc.readAction) { category.readAction = CategoryDoc.readAction; };
                        if (CategoryDoc.updateAction) { category.updateAction = CategoryDoc.updateAction; };
                        if (CategoryDoc.deleteAction) { category.deleteAction = CategoryDoc.deleteAction; };
                        if (CategoryDoc.approveAction) { category.approveAction = CategoryDoc.approveAction; };

                        if (!string.IsNullOrEmpty(CategoryDoc.lv0))
                            if (string.IsNullOrEmpty(category.lv0))
                                category.lv0 = CategoryDoc.lv0;
                            else
                                category.lv0 = category.lv0 + "," + CategoryDoc.lv0;

                        if (!string.IsNullOrEmpty(CategoryDoc.lv1))
                            if (string.IsNullOrEmpty(category.lv1))
                                category.lv1 = CategoryDoc.lv1;
                            else
                                category.lv1 = category.lv1 + "," + CategoryDoc.lv1;

                        if (!string.IsNullOrEmpty(CategoryDoc.lv2))
                            if (string.IsNullOrEmpty(category.lv2))
                                category.lv2 = CategoryDoc.lv2;
                            else
                                category.lv2 = category.lv2 + "," + CategoryDoc.lv2;

                        if (!string.IsNullOrEmpty(CategoryDoc.lv3))
                            if (string.IsNullOrEmpty(category.lv3))
                                category.lv3 = CategoryDoc.lv3;
                            else
                                category.lv3 = category.lv3 + "," + CategoryDoc.lv3;

                        if (!string.IsNullOrEmpty(CategoryDoc.lv4))
                            if (string.IsNullOrEmpty(category.lv4))
                                category.lv4 = CategoryDoc.lv4;
                            else
                                category.lv4 = category.lv4 + "," + CategoryDoc.lv4;

                        if (!string.IsNullOrEmpty(CategoryDoc.lv5))
                            if (string.IsNullOrEmpty(category.lv5))
                                category.lv5 = CategoryDoc.lv5;
                            else
                                category.lv5 = category.lv5 + "," + CategoryDoc.lv5;


                        if (CategoryDoc.organizationPage) { category.organizationPage = CategoryDoc.organizationPage; };
                        if (CategoryDoc.userRolePage) { category.userRolePage = CategoryDoc.userRolePage; };
                        if (CategoryDoc.memberPage) { category.memberPage = CategoryDoc.memberPage; };
                        if (CategoryDoc.memberMobilePage) { category.memberMobilePage = CategoryDoc.memberMobilePage; };

                        if (CategoryDoc.logoPage) { category.logoPage = CategoryDoc.logoPage; };
                        if (CategoryDoc.splashPage) { category.splashPage = CategoryDoc.splashPage; };
                        if (CategoryDoc.mainPopupPage) { category.mainPopupPage = CategoryDoc.mainPopupPage; };
                        if (CategoryDoc.bannerPage) { category.bannerPage = CategoryDoc.bannerPage; };
                        if (CategoryDoc.forceAdsPage) { category.forceAdsPage = CategoryDoc.forceAdsPage; };
                        if (CategoryDoc.rotationPage) { category.rotationPage = CategoryDoc.rotationPage; };

                        if (CategoryDoc.newsPage) { category.newsPage = CategoryDoc.newsPage; };
                        if (CategoryDoc.eventPage) { category.eventPage = CategoryDoc.eventPage; };
                        if (CategoryDoc.contactPage) { category.contactPage = CategoryDoc.contactPage; };
                        if (CategoryDoc.knowledgePage) { category.knowledgePage = CategoryDoc.knowledgePage; };
                        if (CategoryDoc.privilegePage) { category.privilegePage = CategoryDoc.privilegePage; };
                        if (CategoryDoc.poiPage) { category.poiPage = CategoryDoc.poiPage; };
                        if (CategoryDoc.pollPage) { category.pollPage = CategoryDoc.pollPage; };
                        if (CategoryDoc.suggestionPage) { category.suggestionPage = CategoryDoc.suggestionPage; };
                        if (CategoryDoc.notificationPage) { category.notificationPage = CategoryDoc.notificationPage; };
                        if (CategoryDoc.welfarePage) { category.welfarePage = CategoryDoc.welfarePage; };
                        if (CategoryDoc.trainingPage) { category.trainingPage = CategoryDoc.trainingPage; };
                        if (CategoryDoc.skilledLaborPage) { category.skilledLaborPage = CategoryDoc.skilledLaborPage; };
                        if (CategoryDoc.reporterPage) { category.reporterPage = CategoryDoc.reporterPage; };
                        if (CategoryDoc.warningPage) { category.warningPage = CategoryDoc.warningPage; };
                        if (CategoryDoc.fundPage) { category.fundPage = CategoryDoc.fundPage; };
                        if (CategoryDoc.cooperativeFormPage) { category.cooperativeFormPage = CategoryDoc.cooperativeFormPage; };

                        if (CategoryDoc.newsCategoryPage) { category.newsCategoryPage = CategoryDoc.newsCategoryPage; };
                        if (CategoryDoc.eventCategoryPage) { category.eventCategoryPage = CategoryDoc.eventCategoryPage; };
                        if (CategoryDoc.contactCategoryPage) { category.contactCategoryPage = CategoryDoc.contactCategoryPage; };
                        if (CategoryDoc.knowledgeCategoryPage) { category.knowledgeCategoryPage = CategoryDoc.knowledgeCategoryPage; };
                        if (CategoryDoc.privilegeCategoryPage) { category.privilegeCategoryPage = CategoryDoc.privilegeCategoryPage; };
                        if (CategoryDoc.poiCategoryPage) { category.poiCategoryPage = CategoryDoc.poiCategoryPage; };
                        if (CategoryDoc.pollCategoryPage) { category.pollCategoryPage = CategoryDoc.pollCategoryPage; };
                        if (CategoryDoc.suggestionCategoryPage) { category.suggestionCategoryPage = CategoryDoc.suggestionCategoryPage; };
                        if (CategoryDoc.notificationCategoryPage) { category.notificationCategoryPage = CategoryDoc.notificationCategoryPage; };
                        if (CategoryDoc.welfareCategoryPage) { category.welfareCategoryPage = CategoryDoc.welfareCategoryPage; };
                        if (CategoryDoc.trainingCategoryPage) { category.trainingCategoryPage = CategoryDoc.trainingCategoryPage; };
                        if (CategoryDoc.skilledLaborCategoryPage) { category.skilledLaborCategoryPage = CategoryDoc.skilledLaborCategoryPage; };
                        if (CategoryDoc.reporterCategoryPage) { category.reporterCategoryPage = CategoryDoc.reporterCategoryPage; };
                        if (CategoryDoc.warningCategoryPage) { category.warningCategoryPage = CategoryDoc.warningCategoryPage; };
                        if (CategoryDoc.fundCategoryPage) { category.fundCategoryPage = CategoryDoc.fundCategoryPage; };
                        if (CategoryDoc.cooperativeFormCategoryPage) { category.cooperativeFormCategoryPage = CategoryDoc.cooperativeFormCategoryPage; };

                        if (CategoryDoc.policyApplicationPage) { category.policyApplicationPage = CategoryDoc.policyApplicationPage; };
                        if (CategoryDoc.policyMarketingPage) { category.policyMarketingPage = CategoryDoc.policyMarketingPage; };
                        if (CategoryDoc.memberMobilePolicyApplicationPage) { category.memberMobilePolicyApplicationPage = CategoryDoc.memberMobilePolicyApplicationPage; };
                        if (CategoryDoc.memberMobilePolicyMarketingPage) { category.memberMobilePolicyMarketingPage = CategoryDoc.memberMobilePolicyMarketingPage; };
                        //report
                        if (CategoryDoc.reportNumberMemberRegisterPage) { category.reportNumberMemberRegisterPage = CategoryDoc.reportNumberMemberRegisterPage; };
                        if (CategoryDoc.reportMemberRegisterPage) { category.reportMemberRegisterPage = CategoryDoc.reportMemberRegisterPage; };
                        if (CategoryDoc.reportNewsCategoryPage) { category.reportNewsCategoryPage = CategoryDoc.reportNewsCategoryPage; };
                        if (CategoryDoc.reportNewsPage) { category.reportNewsPage = CategoryDoc.reportNewsPage; };
                        if (CategoryDoc.reportKnowledgeCategoryPage) { category.reportKnowledgeCategoryPage = CategoryDoc.reportKnowledgeCategoryPage; };
                        if (CategoryDoc.reportKnowledgePage) { category.reportKnowledgePage = CategoryDoc.reportKnowledgePage; };
                        if (CategoryDoc.reportNewsKeysearchPage) { category.reportNewsKeysearchPage = CategoryDoc.reportNewsKeysearchPage; };
                        if (CategoryDoc.reportKnowledgeKeysearchPage) { category.reportKnowledgeKeysearchPage = CategoryDoc.reportKnowledgeKeysearchPage; };
                        //Master
                        if (CategoryDoc.swearWordsPage) { category.swearWordsPage = CategoryDoc.swearWordsPage; };
                    }
                });

                //category.lv0 = category.lv0.Substring(1);
                //category.lv1 = category.lv1.Substring(1);
                //category.lv2 = category.lv2.Substring(1);
                //category.lv3 = category.lv3.Substring(1);
                //category.lv4 = category.lv4.Substring(1);

                return new Response { status = "S", message = "success", jsonData = category.ToJson(), objectData = category };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read ระดับการเปิดปิดปุ่ม การเห็นข้อมูล
        [HttpPost("page/read")]
        public ActionResult<Response> pageRead([FromBody] Criteria value)
        {
            try
            {
                var dataList = new List<RegisterCategory>();

                //page นี้ อยู่ใน role ไหนบ้าง จากนั้นก็ดูว่า แต่ละ category ทำอะไรได้บ้าง

                // title = หน้าจอ
                // createBy = username

                var col = new Database().MongoClient<Register>( "registerRole");
                var filter = Builders<Register>.Filter.Eq(x => x.isActive, true);
                filter &= Builders<Register>.Filter.Eq("username", value.updateBy);
                var docs = col.Find(filter).SortByDescending(o => o.docDate).Project(c => new { c.category }).ToList();

                docs.ForEach(c =>
                {
                    var CategoryCol = new Database().MongoClient<RegisterCategory>( "registerCategory");

                    var CategoryFilter = Builders<RegisterCategory>.Filter.Eq(x => x.isActive, true);
                    CategoryFilter &= Builders<RegisterCategory>.Filter.Eq("title", c.category);

                    switch (value.title)
                    {
                        case "newsPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.newsPage, true);
                            break;
                        case "eventPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.eventPage, true);
                            break;
                        case "contactPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.contactPage, true);
                            break;
                        case "knowledgePage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.knowledgePage, true);
                            break;
                        case "privilegePage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.privilegePage, true);
                            break;
                        case "poiPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.poiPage, true);
                            break;
                        case "pollPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.pollPage, true);
                            break;
                        case "suggestionPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.suggestionPage, true);
                            break;
                        case "notificationPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.notificationPage, true);
                            break;
                        case "welfarePage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.welfarePage, true);
                            break;
                        case "trainingPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.trainingPage, true);
                            break;
                        case "skilledLaborPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.skilledLaborPage, true);
                            break;
                        case "reporter":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.reporterPage, true);
                            break;
                        case "warning":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.warningPage, true);
                            break;
                        case "fund":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.fundPage, true);
                            break;
                        case "cooperativeForm":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.cooperativeFormPage, true);
                            break;
                        default:
                            break;
                    }

                   

                    var categoryDocs = CategoryCol.Find(CategoryFilter).Project(c => new { c.code, c.createAction, c.readAction, c.updateAction, c.deleteAction, c.approveAction }).ToList();

                    //ไปหาใน RegisterPermission ต่อว่ามี category อะไรบ้าง
                    categoryDocs.ForEach(c => {
                        var permissionCol = new Database().MongoClient<Permission>( "registerPermission");
                        var permissionFilter = Builders<Permission>.Filter.Eq(x => x.isActive, true);
                        permissionFilter &= Builders<Permission>.Filter.Eq("reference", c.code);

                        switch (value.title)
                        {
                            case "newsPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.newsPage, true);
                                break;
                            case "eventPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.eventPage, true);
                                break;
                            case "contactPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.contactPage, true);
                                break;
                            case "knowledgePage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.knowledgePage, true);
                                break;
                            case "privilegePage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.privilegePage, true);
                                break;
                            case "poiPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.poiPage, true);
                                break;
                            case "pollPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.pollPage, true);
                                break;
                            case "suggestionPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.suggestionPage, true);
                                break;
                            case "notificationPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.notificationPage, true);
                                break;
                            case "welfarePage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.welfarePage, true);
                                break;
                            case "trainingPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.trainingPage, true);
                                break;
                            case "skilledLaborPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.skilledLaborPage, true);
                                break;
                            case "reporterPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.reporterPage, true);
                                break;
                            case "warningPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.warningPage, true);
                                break;
                            case "fundPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.fundPage, true);
                                break;
                            case "cooperativeFormPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.cooperativeFormPage, true);
                                break;
                            default:
                                break;
                        }

                        //เซต action  
                        var permissionDocs = permissionCol.Find(permissionFilter).SortByDescending(o => o.docDate).Project(c => new { c.page, c.category }).ToList();
                        permissionDocs.ForEach(cc => {

                            var isExist = dataList.FirstOrDefault(c => c.title == cc.category);

                            if (isExist != null)
                            {
                                if (c.createAction) { isExist.createAction = c.createAction; }
                                if (c.readAction) { isExist.readAction = c.readAction; }
                                if (c.updateAction) { isExist.updateAction = c.updateAction; }
                                if (c.deleteAction) { isExist.deleteAction = c.deleteAction; }
                                if (c.approveAction) { isExist.approveAction = c.approveAction; }
                            }
                            else
                            {
                                dataList.Add(new RegisterCategory { title = cc.category, createAction = c.createAction, readAction = c.readAction, updateAction = c.updateAction, deleteAction = c.deleteAction, approveAction = c.approveAction });
                            }
                        });
                    });
                });

                return new Response { status = "S", message = "success", jsonData = dataList.ToJson(), objectData = dataList };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("organization/read")]
        public ActionResult<Response> OrganizationRead([FromBody] Criteria value)
        {
            try
            {
                var model = new List<Object>();

                var col = new Database().MongoClient<Register>("registerRole");
                var filter = Builders<Register>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.username)) { filter &= Builders<Register>.Filter.Eq("username", value.username); }
                var docs = col.Find(filter).Project(c => new { c.category }).ToList();
                docs.ForEach(c =>
                {
                    var col2 = new Database().MongoClient<RegisterCategory>("registerCategory");
                    var filter2 = Builders<RegisterCategory>.Filter.Ne(x => x.status, "D") & Builders<RegisterCategory>.Filter.Eq(x => x.title, c.category);
                    var docs2 = col2.Find(filter2).Project(c => new { c.title, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv5, c.status }).ToList();
                    docs2.ForEach(cc =>
                    {
                        model.Add(new
                        {
                            title = cc.title,
                            lv0 = cc.lv0,
                            lv1 = cc.lv1,
                            lv2 = cc.lv2,
                            lv3 = cc.lv3,
                            lv4 = cc.lv4,
                            lv5 = cc.lv5,
                            status = "A"
                        });
                    });
                });

                return new Response { status = "S", message = "success", jsonData = model.ToJson(), objectData = model };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("organization/check")]
        public ActionResult<Response> OrganizationCheck([FromBody] Criteria value)
        {
            try
            {
                var isExist = true;

                var model = new List<Object>();

                value.organization.ForEach(c =>
                {

                    if (!string.IsNullOrEmpty(c.lv0))
                    {
                        var split = c.lv0.Split(",");

                        foreach (var item in split)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                var col = new Database().MongoClient<News>("organization");
                                var filter = Builders<News>.Filter.Eq(x => x.code, item);

                                if (isExist)
                                    isExist = col.Find(filter).Any();
                            }
                        }
                    }
                });

                if (!isExist)
                    return new Response { status = "E" };
                else
                    return new Response { status = "S", message = "success", jsonData = model.ToJson(), objectData = model };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        #endregion
    }
}