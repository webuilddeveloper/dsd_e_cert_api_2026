using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using cms_api.Extension;
using cms_api.Models;
using Jose;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class RegisterController : Controller
    {
        public RegisterController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Register value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                {
                    var col = new Database().MongoClient("register");

                    //check duplicate
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                        if (col.Find(filter).Any())
                        {
                            return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                        }
                    }

                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("username", value.username) & Builders<BsonDocument>.Filter.Eq("category", value.category) & Builders<BsonDocument>.Filter.Ne("status", "D");
                        if (col.Find(filter).Any())
                        {
                            //if (value.category == "line")
                            //{
                            //    var col2 = new Database().MongoClient("register");

                            //    var doc2 = col2.Find(filter).FirstOrDefault();

                            //    doc2["lineID"] = value.lineID;

                            //    col2.ReplaceOne(filter, doc2);
                            //}

                            return new Response { status = "E", message = $"username: {value.username} is exist", jsonData = value.ToJson(), objectData = value };
                        }
                    }
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("email", value.email) & Builders<BsonDocument>.Filter.Eq("category", value.category) & Builders<BsonDocument>.Filter.Ne("status", "D");
                        if (col.Find(filter).Any())
                        {
                            return new Response { status = "E", message = $"email: {value.email} is exist", jsonData = value.ToJson(), objectData = value };
                        }
                    }

                    doc = new BsonDocument
                    {
                        { "code", value.code },
                        { "imageUrl", value.imageUrl },
                        { "category", value.category },
                        { "username", value.username },
                        { "password", value.password },
                        { "createBy", value.createBy },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", value.updateBy },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", false },
                        { "status", value.status },
                        { "prefixName", value.prefixName },
                        { "firstName", value.firstName },
                        { "lastName", value.lastName },
                        { "birthDay", value.birthDay },
                        { "phone", value.phone },
                        { "email", value.email },
                        { "facebookID", value.facebookID },
                        { "googleID", value.googleID },
                        { "lineID", value.lineID },
                        { "line", value.line },
                        { "sex", value.sex },
                        { "soi", value.soi },
                        { "address", value.address },
                        { "moo", value.moo },
                        { "road", value.road },
                        { "tambonCode", value.tambonCode },
                        { "tambon", value.tambon },
                        { "amphoeCode", value.amphoeCode },
                        { "amphoe", value.amphoe },
                        { "provinceCode", value.provinceCode },
                        { "province", value.province },
                        { "postnoCode", value.postnoCode },
                        { "postno", value.postno },
                        { "job", value.job },
                        { "idcard", value.idcard },
                        { "officerCode", value.officerCode },
                        { "countUnit", value.countUnit },
                        { "lv0", value.lv0 },
                        { "lv1", value.lv1 },
                        { "lv2", value.lv2 },
                        { "lv3", value.lv3 },
                        { "lv4", value.lv4 },
                        { "linkAccount", value.linkAccount },
                        { "appleID", value.appleID }
                };
                    col.InsertOne(doc);
                }

                //BEGIN : Statistic
                try
                {
                    var value1 = new Criteria();
                    var age = "";
                    var countUnit = "";
                    try
                    {
                        var colCountUnit = new Database().MongoClient<Register>("organization");
                        countUnit = colCountUnit.Find(Builders<Register>.Filter.Eq("lv0", value.lv0)).Project(c => c.title).FirstOrDefault();

                    }
                    catch { }

                    if (!string.IsNullOrEmpty(value.birthDay))
                    {
                        //var year = value.birthDay.Substring(0, 4);
                        //var yearNow = DateTime.Now;
                        //age = year;
                    }

                    value1.title = value.category;
                    value1.platform = value.platform;
                    value1.countUnit = countUnit;
                    value1.sex = value.sex;
                    value1.age = age;
                    value1.updateBy = value.username;

                    if (!string.IsNullOrEmpty(value.code))
                        value1.reference = value.code;

                    value1.statisticsCreate("register");
                }
                catch { }
                //END : Statistic

                //ฺBEGIN :menuNotification >>>>>>>>>>>>>>>>>>>>>>>

                {
                    var col = new Database().MongoClient("registerNotification");

                    var docNoti = new BsonDocument
                    {
                        { "code", "".toCode() },
                        { "username", value.username },
                        { "mainPage", true },
                        { "newsPage", true },
                        { "eventPage", true },
                        { "contactPage", true },
                        { "knowledgePage", true },
                        { "privilegePage", true },
                        { "poiPage", true },
                        { "pollPage", true },
                        { "suggestionPage", true },
                        { "reporterPage", true },
                        { "trainingPage", true },
                        { "welfarePage", true },
                        { "warningPage", true },
                        { "fundPage", true },
                        { "cooperativeFormPage", true },
                        { "createBy", value.createBy },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", value.updateBy },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true }
                    };
                    col.InsertOne(docNoti);
                }

                {
                    //var menuList = new[]
                    //{
                    //    "News", "Event", "Contact", "Knowledge", "Privilege", "POI", "Poll", "Suggestion"
                    //};

                    //foreach (var item in menuList)
                    //{
                    //    var col = new Database().MongoClient( "menuNotification");

                    //    var docNoti = new BsonDocument
                    //    {
                    //        { "code", "".toCode() },
                    //        { "username", value.username },
                    //        { "title", item },
                    //        { "createBy", value.createBy },
                    //        { "createDate", DateTime.Now.toStringFromDate() },
                    //        { "createTime", DateTime.Now.toTimeStringFromDate() },
                    //        { "updateBy", value.updateBy },
                    //        { "updateDate", DateTime.Now.toStringFromDate() },
                    //        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    //        { "docDate", DateTime.Now.Date.AddHours(7) },
                    //        { "docTime", DateTime.Now.toTimeStringFromDate() },
                    //        { "isActive", true }
                    //    };
                    //    col.InsertOne(docNoti);
                    //}
                }

                //END :menuNotification <<<<<<<<<<<<<<<<<<<<<<<<<

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
                //var filter = (Builders<Register>.Filter.Eq(x => x.isActive, true || false));
                //&value.filterOrganization<Register>()
                var filter = (Builders<Register>.Filter.Ne("status", "D")) & (Builders<Register>.Filter.Eq(x => x.category, "guest") | Builders<Register>.Filter.Eq(x => x.category, "facebook") | Builders<Register>.Filter.Eq(x => x.category, "google") | Builders<Register>.Filter.Eq(x => x.category, "line") | Builders<Register>.Filter.Eq(x => x.category, "apple"));

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")) | (filter & Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Register>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.status)) {
                        if (value.status == "VR")
                        {
                            filter = (Builders<Register>.Filter.Eq("status", "V") | Builders<Register>.Filter.Eq("status", "R"));
                        } else
                        {
                            filter = filter & Builders<Register>.Filter.Eq("status", value.status);
                        }
                    }

                    if (!string.IsNullOrEmpty(value.username)) { filter = filter & Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.username), "i")); }
                    if (!string.IsNullOrEmpty(value.password)) { filter = filter & Builders<Register>.Filter.Eq("password", value.password); }
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Register>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Register>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Register>.Filter.Regex("category", value.category); }
                    if (!string.IsNullOrEmpty(value.sex)) { filter = filter & Builders<Register>.Filter.Regex("sex", value.sex); }
                    if (!string.IsNullOrEmpty(value.lineID)) { filter = filter & Builders<Register>.Filter.Regex("lineID", value.lineID); }
                    if (!string.IsNullOrEmpty(value.line)) { filter = filter & Builders<Register>.Filter.Regex("line", value.line); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Register>.Filter.Regex("createBy", new BsonRegularExpression(string.Format(".*{0}.*", value.createBy), "i")); }

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
                    c.docTime
                    ,
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
                    c.line,
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
                    c.linkAccount,
                    c.appleID

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


        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                value.logCreate("register/update", value.code);

                var col2 = new Database().MongoClient("register");
                var filter2 = Builders<BsonDocument>.Filter.Eq("email", value.email);


                if (!string.IsNullOrEmpty(value.category) && !string.IsNullOrEmpty(value.code))
                {
                    var col3 = new Database().MongoClient<Register>("register");
                    var filter3 = Builders<Register>.Filter.Ne("status", "D") & Builders<Register>.Filter.Eq("code", value.code);
                    var doc3 = col3.Find(filter3).Project(c => new { c.countUnit }).FirstOrDefault();

                    var dataCountUnit = "";
                    if (string.IsNullOrEmpty(doc3.countUnit))
                    {
                        dataCountUnit = "[]";
                    }
                    else
                    {
                        dataCountUnit = doc3.countUnit;
                    }

                    List<CountUnit> countUnitDB = JsonConvert.DeserializeObject<List<CountUnit>>(dataCountUnit); // count unit from database
                    List<CountUnit> countUnitValue = JsonConvert.DeserializeObject<List<CountUnit>>(value.countUnit); // count unit from value

                    if (countUnitDB.Count != countUnitValue.Count)
                    {
                        if (countUnitValue.Count == 0)
                        {
                            value.isActive = false;
                            value.status = "N";
                        }
                        else if (countUnitValue.Count > 0)
                        {
                            for (int i = 0; i < countUnitValue.Count; i++)
                            {
                                if (countUnitValue[i].status != "A")
                                {
                                    value.isActive = false;
                                    value.status = "V";
                                    break;
                                }
                                else
                                {
                                    value.isActive = true;
                                    value.status = "A";
                                }
                            }
                        }
                    }
                }

                // start update linkAccount
                var col = new Database().MongoClient("register");

                if (string.IsNullOrEmpty(value.linkAccount) && !string.IsNullOrEmpty(value.code))
                {
                    var filter = Builders<BsonDocument>.Filter.Ne("status", "D") & Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = col.Find(filter).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);

                    var json = JsonConvert.SerializeObject(model);
                    var doc1 = new BsonDocument();
                    var col1 = new Database().MongoClient("_logRegister");
                    doc1 = new BsonDocument
                    {
                        { "code", "".toCode() },
                        { "step", "updateLinkAccount" },
                        { "raw", json },
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
                    col1.InsertOne(doc1);

                    if (value.category != "guest")
                    {
                        doc["username"] = value.username;
                    }
                    doc["imageUrl"] = value.imageUrl;
                    doc["category"] = value.category;
                    doc["prefixName"] = value.prefixName;
                    doc["firstName"] = value.firstName;
                    doc["lastName"] = value.lastName;
                    doc["birthDay"] = value.birthDay;
                    doc["phone"] = value.phone;
                    doc["email"] = value.email;
                    doc["facebookID"] = value.facebookID;
                    doc["googleID"] = value.googleID;
                    doc["lineID"] = value.lineID;
                    doc["line"] = value.line;
                    doc["password"] = value.password;
                    doc["sex"] = value.sex;
                    doc["soi"] = value.soi;
                    doc["address"] = value.address;
                    doc["moo"] = value.moo;
                    doc["road"] = value.road;
                    doc["tambonCode"] = value.tambonCode;
                    doc["tambon"] = value.tambon;
                    doc["amphoeCode"] = value.amphoeCode;
                    doc["amphoe"] = value.amphoe;
                    doc["provinceCode"] = value.provinceCode;
                    doc["province"] = value.province;
                    doc["postnoCode"] = value.postnoCode;
                    doc["postno"] = value.postno;
                    doc["job"] = value.job;
                    doc["idcard"] = value.idcard;
                    doc["officerCode"] = value.officerCode;
                    doc["countUnit"] = value.countUnit;
                    doc["lv0"] = value.lv0;
                    doc["lv1"] = value.lv1;
                    doc["lv2"] = value.lv2;
                    doc["lv3"] = value.lv3;
                    doc["lv4"] = value.lv4;
                    doc["linkAccount"] = value.linkAccount;
                    doc["description"] = value.description;
                    doc["isActive"] = value.isActive;
                    doc["status"] = value.status;
                    doc["updateBy"] = value.updateBy;
                    doc["updateDate"] = DateTime.Now.toStringFromDate();
                    doc["appleID"] = value.appleID;
                    col.ReplaceOne(filter, doc);
                }
                else if (!string.IsNullOrEmpty(value.linkAccount))
                {
                    var collectionPermission = new Database().MongoClient("register");
                    var filterPermission = Builders<BsonDocument>.Filter.Ne("status", "D") & Builders<BsonDocument>.Filter.Eq("linkAccount", value.linkAccount);

                    var json = JsonConvert.SerializeObject(filterPermission);
                    var doc1 = new BsonDocument();
                    var col1 = new Database().MongoClient("_logRegister");
                    doc1 = new BsonDocument
                    {
                        { "code", "".toCode() },
                        { "step", "updateLinkAccount" },
                        { "raw", json },
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
                    col1.InsertOne(doc1);


                    var updatePermission = Builders<BsonDocument>.Update
                    .Set("imageUrl", value.imageUrl)
                    .Set("prefixName", value.prefixName)
                    .Set("firstName", value.firstName)
                    .Set("lastName", value.lastName)
                    .Set("birthDay", value.birthDay)
                    .Set("phone", value.phone)
                    .Set("facebookID", value.facebookID)
                    .Set("googleID", value.googleID)
                    .Set("lineID", value.lineID)
                    .Set("line", value.line)
                    .Set("password", value.password)
                    .Set("email", value.email)
                    .Set("sex", value.sex)
                    .Set("soi", value.soi)
                    .Set("address", value.address)
                    .Set("moo", value.moo)
                    .Set("road", value.road)
                    .Set("tambonCode", value.tambonCode)
                    .Set("tambon", value.tambon)
                    .Set("amphoeCode", value.amphoeCode)
                    .Set("amphoe", value.amphoe)
                    .Set("province", value.province)
                    .Set("provinceCode", value.provinceCode)
                    .Set("postnoCode", value.postnoCode)
                    .Set("postno", value.postno)
                    .Set("job", value.job)
                    .Set("idcard", value.idcard)
                    .Set("officerCode", value.officerCode)
                    .Set("countUnit", value.countUnit)
                    .Set("lv0", value.lv0)
                    .Set("lv1", value.lv1)
                    .Set("lv2", value.lv2)
                    .Set("lv3", value.lv3)
                    .Set("lv4", value.lv4)
                    .Set("isActive", value.isActive)
                    .Set("description", value.description)
                    .Set("status", value.status)
                    .Set("updateBy", value.updateBy)
                    .Set("appleID", value.appleID)
                    .Set("updateDate", DateTime.Now.toStringFromDate());
                    collectionPermission.UpdateMany(filterPermission, updatePermission);

                }
                // end update linkAccount

                //BEGIN :read >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                var registerCol = new Database().MongoClient<Register>("register");
                var registerFilter = Builders<Register>.Filter.Eq("code", value.code);
                var registerDoc = registerCol.Find(registerFilter).Project(c => new
                {
                    c.code,
                    c.username,
                    c.password,
                    c.status,
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
                    c.appleID,
                    c.line,
                    c.sex,
                    c.soi,
                    c.address,
                    c.moo,
                    c.road,
                    c.tambon,
                    c.amphoe,
                    c.province,
                    c.postno,
                    c.tambonCode,
                    c.amphoeCode,
                    c.provinceCode,
                    c.postnoCode,
                    c.job,
                    c.idcard,
                    c.officerCode,
                    c.countUnit,
                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,
                    c.linkAccount,

                }).FirstOrDefault();

                //END :read <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                return new Response { status = "S", message = "success", jsonData = registerDoc.ToJson(), objectData = registerDoc };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("linkAccount/create")]
        public ActionResult<Response> LinkAccountCreate([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                value.logCreate("linkAccount/create", value.code);

                var col = new Database().MongoClient("register");


                {
                    var filter2 = Builders<BsonDocument>.Filter.Eq("category", value.category) & Builders<BsonDocument>.Filter.Ne("status", "D");
                    if (value.category == "facebook")
                    {
                        filter2 &= Builders<BsonDocument>.Filter.Eq("facebookID", value.facebookID);
                    }
                    else if (value.category == "line")
                    {
                        filter2 &= Builders<BsonDocument>.Filter.Eq("lineID", value.lineID);
                    }
                    else if (value.category == "google")
                    {
                        filter2 &= Builders<BsonDocument>.Filter.Eq("googleID", value.googleID);
                    }
                    else if (value.category == "apple")
                    {
                        filter2 &= Builders<BsonDocument>.Filter.Eq("appleID", value.appleID);
                    }

                    if (col.Find(filter2).Any())
                    {
                        return new Response { status = "E", message = $"{value.category} ได้ทำการเชื่อมต่อกับบัญชีอื่นแล้ว", jsonData = value.ToJson(), objectData = value };
                    }
                }

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);


                // start update linkAccount
                var colRegister = new Database().MongoClient("register");

                if (string.IsNullOrEmpty(value.linkAccount) && !string.IsNullOrEmpty(value.code))
                {
                    value.linkAccount = "".toCode();
                    var filterRegister = Builders<BsonDocument>.Filter.Ne("status", "D") & Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colRegister.Find(filterRegister).FirstOrDefault();
                    var modelRegister = BsonSerializer.Deserialize<object>(doc);

                    var json = JsonConvert.SerializeObject(modelRegister);
                    var doc1 = new BsonDocument();
                    var col1 = new Database().MongoClient("_logLinkAccount");
                    doc1 = new BsonDocument
                    {
                        { "code", "".toCode() },
                        { "step", "linkAccount" },
                        { "raw", json },
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
                    col1.InsertOne(doc1);

                    doc["linkAccount"] = value.linkAccount;
                    doc["appleID"] = value.appleID;
                    doc["facebookID"] = value.facebookID;
                    doc["googleID"] = value.googleID;
                    doc["lineID"] = value.lineID;
                    doc["updateBy"] = value.updateBy;
                    doc["updateDate"] = DateTime.Now.toStringFromDate();
                    colRegister.ReplaceOne(filterRegister, doc);
                }
                else if (!string.IsNullOrEmpty(value.linkAccount))
                {
                    var collectionPermission = new Database().MongoClient("register");
                    var filterPermission = Builders<BsonDocument>.Filter.Ne("status", "D") & Builders<BsonDocument>.Filter.Eq("linkAccount", value.linkAccount);

                    var json = JsonConvert.SerializeObject(filterPermission);
                    var doc1 = new BsonDocument();
                    var col1 = new Database().MongoClient("_logLinkAccount");
                    doc1 = new BsonDocument
                    {
                        { "code", "".toCode() },
                        { "step", "linkAccount" },
                        { "raw", json },
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
                    col1.InsertOne(doc1);

                    var updatePermission = Builders<BsonDocument>.Update

                    .Set("linkAccount", value.linkAccount)
                    .Set("appleID", value.appleID)
                    .Set("facebookID", value.facebookID)
                    .Set("googleID", value.googleID)
                    .Set("lineID", value.lineID)
                    .Set("updateBy", value.updateBy)
                    .Set("updateDate", DateTime.Now.toStringFromDate());
                    collectionPermission.UpdateMany(filterPermission, updatePermission);

                }
                // end update linkAccount    

                doc = new BsonDocument
                    {
                        { "code", "".toCode() },
                        { "imageUrl", value.imageUrl },
                        { "category", value.category },
                        { "username", value.username },
                        { "password", value.password },
                        { "createBy", value.createBy },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", value.updateBy },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", value.isActive },
                        { "status", value.status },
                        { "prefixName", value.prefixName },
                        { "firstName", value.firstName },
                        { "lastName", value.lastName },
                        { "birthDay", value.birthDay },
                        { "phone", value.phone },
                        { "email", value.email },
                        { "line", value.line },
                        { "sex", value.sex },
                        { "soi", value.soi },
                        { "address", value.address },
                        { "moo", value.moo },
                        { "road", value.road },
                        { "tambonCode", value.tambonCode },
                        { "tambon", value.tambon },
                        { "amphoeCode", value.amphoeCode },
                        { "amphoe", value.amphoe },
                        { "provinceCode", value.provinceCode },
                        { "province", value.province },
                        { "postnoCode", value.postnoCode },
                        { "postno", value.postno },
                        { "job", value.job },
                        { "idcard", value.idcard },
                        { "officerCode", value.officerCode },
                        { "countUnit", value.countUnit },
                        { "lv0", value.lv0 },
                        { "lv1", value.lv1 },
                        { "lv2", value.lv2 },
                        { "lv3", value.lv3 },
                        { "lv4", value.lv4 },
                        { "linkAccount", value.linkAccount },
                        { "description", value.description },
                        { "appleID",value.appleID },
                        { "facebookID", value.facebookID },
                        { "googleID", value.googleID },
                        { "lineID", value.lineID },
                };
                col.InsertOne(doc);

                //BEGIN :read >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                var registerCol = new Database().MongoClient<Register>("register");
                var registerFilter = Builders<Register>.Filter.Eq("code", value.code);
                var registerDoc = registerCol.Find(registerFilter).Project(c => new
                {
                    c.code,
                    c.username,
                    c.password,
                    c.status,
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
                    c.line,
                    c.sex,
                    c.soi,
                    c.address,
                    c.moo,
                    c.road,
                    c.tambon,
                    c.amphoe,
                    c.province,
                    c.postno,
                    c.tambonCode,
                    c.amphoeCode,
                    c.provinceCode,
                    c.postnoCode,
                    c.job,
                    c.idcard,
                    c.officerCode,
                    c.countUnit,
                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,
                    c.linkAccount,
                    c.description,
                    c.appleID,

                }).FirstOrDefault();

                //END :read <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                return new Response { status = "S", message = "success", jsonData = registerDoc.ToJson(), objectData = registerDoc };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("linkAccount/delete")]
        public ActionResult<Response> linkAccountDelete([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient("register");

                var filter = Builders<BsonDocument>.Filter.Eq("linkAccount", value.linkAccount) & Builders<BsonDocument>.Filter.Eq("category", value.category) & Builders<BsonDocument>.Filter.Ne("status", "D");

                var update = Builders<BsonDocument>.Update.Set("status", "D").Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                col.UpdateOne(filter, update);


                var collectionPermission = new Database().MongoClient("register");
                var filterPermission = Builders<BsonDocument>.Filter.Ne("status", "D") & Builders<BsonDocument>.Filter.Eq("linkAccount", value.linkAccount);

                var json = JsonConvert.SerializeObject(filterPermission);
                var doc1 = new BsonDocument();
                var col1 = new Database().MongoClient("_logRegister");
                doc1 = new BsonDocument
                {
                        { "code", "".toCode() },
                        { "step", "unlinkAccount" },
                        { "raw", json },
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
                col1.InsertOne(doc1);

                if (value.category == "facebook")
                {
                    var updatePermission = Builders<BsonDocument>.Update
                    .Set("updateBy", value.updateBy)
                    .Set("facebookID", "")
                    .Set("updateDate", DateTime.Now.toStringFromDate());
                    collectionPermission.UpdateMany(filterPermission, updatePermission);
                }
                else if (value.category == "google")
                {
                    var updatePermission = Builders<BsonDocument>.Update
                    .Set("updateBy", value.updateBy)
                    .Set("googleID", "")
                    .Set("updateDate", DateTime.Now.toStringFromDate());
                    collectionPermission.UpdateMany(filterPermission, updatePermission);
                }
                else if (value.category == "line")
                {
                    var updatePermission = Builders<BsonDocument>.Update
                    .Set("updateBy", value.updateBy)
                    .Set("lineID", "")
                    .Set("updateDate", DateTime.Now.toStringFromDate());
                    collectionPermission.UpdateMany(filterPermission, updatePermission);
                }
                else if (value.category == "apple")
                {
                    var updatePermission = Builders<BsonDocument>.Update
                    .Set("updateBy", value.updateBy)
                    .Set("appleID", "")
                    .Set("updateDate", DateTime.Now.toStringFromDate());
                    collectionPermission.UpdateMany(filterPermission, updatePermission);
                }


                var registerCol = new Database().MongoClient<Register>("register");
                var registerFilter = Builders<Register>.Filter.Eq("code", value.code);
                var registerDoc = registerCol.Find(registerFilter).Project(c => new
                {
                    c.code,
                    c.username,
                    c.password,
                    c.status,
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
                    c.line,
                    c.sex,
                    c.soi,
                    c.address,
                    c.moo,
                    c.road,
                    c.tambon,
                    c.amphoe,
                    c.province,
                    c.postno,
                    c.tambonCode,
                    c.amphoeCode,
                    c.provinceCode,
                    c.postnoCode,
                    c.job,
                    c.idcard,
                    c.officerCode,
                    c.countUnit,
                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,
                    c.linkAccount,
                    c.description,
                    c.appleID

                }).FirstOrDefault();

                //var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                //var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                //col.UpdateOne(filter, update);
                return new Response { status = "S", message = $"code: {value.code} is unlink", jsonData = registerDoc.ToJson(), objectData = registerDoc };
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
                var col = new Database().MongoClient("register");

                var codeList = value.code.Split(",");

                foreach (var code in codeList)
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("status", "D").Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                    col.UpdateOne(filter, update);
                }

                //var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                //var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                //col.UpdateOne(filter, update);
                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /validate
        [HttpPost("validate")]
        public ActionResult<Response> Validate([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                value.logCreate("register/validate", value.code);

                var col2 = new Database().MongoClient("register");
                var filter2 = Builders<BsonDocument>.Filter.Eq("email", value.email);

                var filter3 = Builders<BsonDocument>.Filter.Eq("idcard", value.idcard) & Builders<BsonDocument>.Filter.Ne("code", value.code);
                if (col2.Find(filter3).Any())
                {
                    //return new Response { status = "E", message = $"idcard: {value.idcard} is exist", jsonData = value.ToJson(), objectData = value };
                    return new Response { status = "E", message = $"รหัสบัตรประชาชน {value.idcard} ได้ทำการยืนยันตัวตนเรียบร้อยแล้ว กรุณาตรวจสอบใหม่อีกครั้ง", jsonData = value.ToJson(), objectData = value };

                }
                else
                {

                    return new Response { status = "S", message = "success" };
                }

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        #region password

        // POST /login
        [HttpPost("apple/login")]
        public ActionResult<Response> LoginApple([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");

                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");

                filter &= Builders<Register>.Filter.Eq("category", "apple");
                filter &= Builders<Register>.Filter.Eq("appleID", value.username);         

                var doc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category }).FirstOrDefault();

                if (doc != null)
                {
                    value.code = value.code.toCode();
                    var token = $"{doc.username}|{doc.password}|{doc.category}|{value.code}".toEncode();

                    //BEGIN :disable session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :disable seesion <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :create session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :create session <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :create activity >>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :create activity <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :read >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    var docs = col.Find(filter).Project(c => new
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
                        c.line,
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
                        c.linkAccount,
                        c.appleID

                    }).FirstOrDefault();

                    //END :read <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    return new Response { status = "S", message = "success", jsonData = token, objectData = docs };
                }
                else
                {
                    try
                    {
                        var register = new Database().MongoClient("register");
                        var docRegister = new BsonDocument();
                        docRegister = new BsonDocument
                {
                    { "code", "".toCode() },
                    { "imageUrl", "" },
                    { "category", "apple" },
                    { "username", value.username },
                    { "password", value.password },
                    { "prefixName", "" },
                    { "firstName", value.firstName },
                    { "lastName", value.lastName },
                    { "birthDay", "" },
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
                    { "isActive", true },
                    { "status", "A"}
                };
                        register.InsertOne(docRegister);

                        return new Response { status = "S", message = "success", jsonData = docRegister.ToJson(), objectData = BsonSerializer.Deserialize<object>(docRegister) };
                    }
                    catch (Exception ex)
                    {
                        return new Response { status = "E", message = ex.Message };
                    }
                }

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /login
        [HttpPost("line/login")]
        public ActionResult<Response> LoginLine([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");

                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");
                filter &= Builders<Register>.Filter.Eq("category", "line");
                filter &= Builders<Register>.Filter.Eq("lineID", value.username);

                var doc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category }).FirstOrDefault();

                if (doc != null)
                {
                    value.code = value.code.toCode();
                    var token = $"{doc.username}|{doc.password}|{doc.category}|{value.code}".toEncode();

                    //BEGIN :disable session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :disable seesion <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :create session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :create session <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :create activity >>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :create activity <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :read >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    var docs = col.Find(filter).Project(c => new
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
                        c.line,
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
                        c.linkAccount,
                        c.appleID

                    }).FirstOrDefault();

                    //END :read <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    return new Response { status = "S", message = "success", jsonData = token, objectData = docs };
                }
                else
                {
                    var filter1 = Builders<Register>.Filter.Ne(x => x.status, "D");
                    filter1 &= Builders<Register>.Filter.Eq("category", "line");
                    filter1 &= Builders<Register>.Filter.Eq("email", value.email);

                    var doc1 = col.Find(filter1).Project(c => new { c.code, c.username, c.password, c.category }).FirstOrDefault();

                    if (doc1 != null)
                    {
                        value.code = value.code.toCode();
                        var token = $"{doc1.username}|{doc1.password}|{doc1.category}|{value.code}".toEncode();

                        //BEGIN :disable session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                        //END :disable seesion <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                        //BEGIN :create session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                        //END :create session <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                        //BEGIN :create activity >>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                        //END :create activity <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                        //BEGIN :read >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                        var docs = col.Find(filter1).Project(c => new
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
                            c.line,
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
                            c.linkAccount,
                            c.appleID

                        }).FirstOrDefault();

                        //END :read <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                        return new Response { status = "S", message = "success", jsonData = token, objectData = docs };
                    }
                    else
                    {
                        return new Response { status = "F", message = "login failed", jsonData = "", objectData = "" };
                    }
                }

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /login
        [HttpPost("google/login")]
        public ActionResult<Response> LoginGoogle([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");

                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");

                filter &= Builders<Register>.Filter.Eq("username", value.username);
                filter &= Builders<Register>.Filter.Eq("category", "google");


                var doc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category }).FirstOrDefault();

                if (doc != null)
                {
                    value.code = value.code.toCode();
                    var token = $"{doc.username}|{doc.password}|{doc.category}|{value.code}".toEncode();

                    //BEGIN :disable session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :disable seesion <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :create session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :create session <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :create activity >>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :create activity <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :read >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    var docs = col.Find(filter).Project(c => new
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
                        c.line,
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
                        c.linkAccount,
                        c.appleID

                    }).FirstOrDefault();

                    //END :read <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    return new Response { status = "S", message = "success", jsonData = token, objectData = docs };
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

        // POST /login
        [HttpPost("facebook/login")]
        public ActionResult<Response> LoginFacebook([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");

                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");
                filter &= Builders<Register>.Filter.Eq("category", "facebook");

                filter &= Builders<Register>.Filter.Eq("facebookID", value.username);
           
                var doc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category }).FirstOrDefault();

                if (doc != null)
                {
                    value.code = value.code.toCode();
                    var token = $"{doc.username}|{doc.password}|{doc.category}|{value.code}".toEncode();

                    //BEGIN :disable session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :disable seesion <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :create session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :create session <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :create activity >>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :create activity <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :read >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    var docs = col.Find(filter).Project(c => new
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
                        c.line,
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
                        c.linkAccount,
                        c.appleID

                    }).FirstOrDefault();

                    //END :read <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    return new Response { status = "S", message = "success", jsonData = token, objectData = docs };
                }
                else
                {
                    var filter1 = Builders<Register>.Filter.Ne(x => x.status, "D");
                    filter1 &= Builders<Register>.Filter.Eq("category", "facebook");

                    filter1 &= Builders<Register>.Filter.Eq("email", value.email);

                    var doc1 = col.Find(filter1).Project(c => new { c.code, c.username, c.password, c.category }).FirstOrDefault();

                    if (doc1 != null)
                    {
                        value.code = value.code.toCode();
                        var token = $"{doc1.username}|{doc1.password}|{doc1.category}|{value.code}".toEncode();

                        //BEGIN :disable session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                        //END :disable seesion <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                        //BEGIN :create session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                        //END :create session <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                        //BEGIN :create activity >>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                        //END :create activity <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                        //BEGIN :read >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                        var docs = col.Find(filter1).Project(c => new
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
                            c.line,
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
                            c.linkAccount,
                            c.appleID

                        }).FirstOrDefault();

                        //END :read <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                        return new Response { status = "S", message = "success", jsonData = token, objectData = docs };
                    }
                    else
                    {
                        return new Response { status = "F", message = "login failed", jsonData = "", objectData = "" };
                    }
                }

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /login
        [HttpPost("login")]
        public ActionResult<Response> Login([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");

                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");
                //filter &= Builders<Register>.Filter.Eq("category", value.category);
                filter &= Builders<Register>.Filter.Eq("username", value.username);

                if (!string.IsNullOrEmpty(value.category))
                {
                    filter &= Builders<Register>.Filter.Eq("category", value.category);

                    if (value.category == "guest")
                    {
                        filter &= Builders<Register>.Filter.Eq("password", value.password);
                    }
                }
                else
                {
                    filter &= (Builders<Register>.Filter.Eq(x => x.category, "guest") | Builders<Register>.Filter.Eq(x => x.category, "facebook") | Builders<Register>.Filter.Eq(x => x.category, "google") | Builders<Register>.Filter.Eq(x => x.category, "line") | Builders<Register>.Filter.Eq(x => x.category, "apple"));
                    filter &= Builders<Register>.Filter.Eq("password", value.password);
                }

                var doc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category }).FirstOrDefault();

                if (doc != null)
                {
                    value.code = value.code.toCode();
                    var token = $"{doc.username}|{doc.password}|{doc.category}|{value.code}".toEncode();

                    //BEGIN :disable session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :disable seesion <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :create session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :create session <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :create activity >>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

                    //END :create activity <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    //BEGIN :read >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    var docs = col.Find(filter).Project(c => new
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
                        c.line,
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
                        c.linkAccount,
                        c.appleID

                    }).FirstOrDefault();

                    //END :read <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                    return new Response { status = "S", message = "success", jsonData = token, objectData = docs };
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

        // POST /update
        [HttpPost("change")]
        public ActionResult<Response> Change([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                {
                    var col = new Database().MongoClient<Register>("register");
                    var filter = Builders<Register>.Filter.Ne("status", "D") & Builders<Register>.Filter.Eq("code", value.code) & Builders<Register>.Filter.Eq("password", value.password);
                    var docs = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.isActive, c.createBy, c.createDate, c.imageUrl, c.updateBy, c.updateDate, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.prefixName, c.firstName, c.lastName, c.birthDay, c.phone, c.email, c.facebookID, c.googleID, c.lineID }).FirstOrDefault();

                    if (docs == null)
                    {
                        return new Response { status = "E", message = "รหัสผ่านไม่ถูกต้อง", };
                    }

                }

                {
                    var col = new Database().MongoClient("register");
                    var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = col.Find(filter).FirstOrDefault();

                    if (!string.IsNullOrEmpty(value.newPassword)) { doc["password"] = value.newPassword; }
                    doc["updateBy"] = value.updateBy;
                    doc["updateDate"] = DateTime.Now.toStringFromDate();
                    col.ReplaceOne(filter, doc);

                    doc = col.Find(filter).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);

                    return new Response { status = "S", message = "success", jsonData = model.ToJson(), objectData = model };
                }

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("forgot/password")]
        public ActionResult<Response> ForgetReadAsync([FromBody] Register value)
        {
            try
            {
                var doc = new BsonDocument();
                var newPassword = "".getRandom();
                var col = new Database().MongoClient("register");
                var filter = Builders<BsonDocument>.Filter.Eq("email", value.email);
                var filterConfig = Builders<BsonDocument>.Filter.Eq("title", "email");

                var colConfig = new Database().MongoClient("configulation");

                var docConfig = colConfig.Find(filterConfig).FirstOrDefault();
                var mailServier = BsonSerializer.Deserialize<Register>(docConfig);

                string name = mailServier.username;
                string password = mailServier.password;
                string emailHost = mailServier.email;
                bool enableSsl = true;
                int port = 587;

                if (col.Find(filter).Any())
                {
                    doc = col.Find(filter).FirstOrDefault();
                    doc["password"] = newPassword;
                    col.ReplaceOne(filter, doc);

                    SmtpClient client = new SmtpClient("smtp.gmail.com");
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(emailHost, password);
                    client.EnableSsl = enableSsl;
                    client.Port = port;

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(emailHost, name);
                    mailMessage.To.Add(value.email);
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Body = "รหัสผ่านใหม่ของคุณคือ  " + newPassword;
                    mailMessage.Subject = "ยืนยันการเปลี่ยนรหัสผ่าน";
                    client.Send(mailMessage);

                    return new Response { status = "S", message = "change password success.", jsonData = doc.ToJson() };
                }
                else
                {
                    return new Response { status = "E", message = "email not found." };
                }


            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /Admin approve (Org) from user moblie
        [HttpPost("approve/register")]
        public ActionResult<Response> approveRegisterAsync([FromBody] Register value)
        {
            try
            {
                var doc = new BsonDocument();
                var col = new Database().MongoClient("register");
                var filter = Builders<BsonDocument>.Filter.Eq("email", value.email);
                var filterConfig = Builders<BsonDocument>.Filter.Eq("title", "email");

                var colConfig = new Database().MongoClient("configulation");

                var docConfig = colConfig.Find(filterConfig).FirstOrDefault();
                var mailServier = BsonSerializer.Deserialize<Register>(docConfig);

                string name = mailServier.username;
                string password = mailServier.password;
                string emailHost = mailServier.email;
                bool enableSsl = true;
                int port = 587;

                if (col.Find(filter).Any())
                {

                    SmtpClient client = new SmtpClient("smtp.gmail.com");
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(emailHost, password);
                    client.EnableSsl = enableSsl;
                    client.Port = port;

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(emailHost, name);
                    mailMessage.To.Add(value.email);
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Body = "ผู้ดูแลระบบได้ทำการอนุมัติสิทธ์ของคุณเรียบร้อยแล้ว สามารถตรวจสอบข้อมูลได้บน Application สช. On Moblie";
                    mailMessage.Subject = "อนุมัติสิทธ์การใช้งาน สช. On Moblie";
                    client.Send(mailMessage);

                    return new Response { status = "S", message = "send Email Success.", jsonData = doc.ToJson() };
                }
                else
                {
                    return new Response { status = "E", message = "email not found." };
                }


            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /Identity Verification From Moblie App(Org) to admin(Org)
        [HttpPost("sendMail/verification")]
        public ActionResult<Response> sendMailRegisterAsync([FromBody] Register value)
        {
            try
            {
                var doc = new BsonDocument();
                //var newPassword = "".getRandom();
                var col = new Database().MongoClient<Register>("register");
                //var filter = Builders<BsonDocument>.Filter.Eq("email", value.email);

                var filterConfig = Builders<BsonDocument>.Filter.Eq("title", "email");

                var colConfig = new Database().MongoClient("configulation");

                var docConfig = colConfig.Find(filterConfig).FirstOrDefault();
                var mailServier = BsonSerializer.Deserialize<Register>(docConfig);

                string name = mailServier.username;
                string password = mailServier.password;
                string emailHost = mailServier.email;
                bool enableSsl = true;
                int port = 587;


                var filter = Builders<Register>.Filter.Ne("status", "D")
                    & (Builders<Register>.Filter.Ne(x => x.category, "guest")) & (Builders<Register>.Filter.Ne(x => x.category, "facebook")) & (Builders<Register>.Filter.Ne(x => x.category, "google")) & (Builders<Register>.Filter.Ne(x => x.category, "line"));

                List<Register> docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).As<Register>().ToList();

                if(docs.Count > 0)
                {
                    foreach (var item in docs)
                    {
                        if (!string.IsNullOrEmpty(item.email)){
                            SmtpClient client = new SmtpClient("smtp.gmail.com");
                            client.UseDefaultCredentials = false;
                            client.Credentials = new NetworkCredential(emailHost, password);
                            client.EnableSsl = enableSsl;
                            client.Port = port;

                            MailMessage mailMessage = new MailMessage();
                            //mailMessage.From = new MailAddress(emailHost, name);
                            mailMessage.From = new MailAddress(value.email, value.firstName);
                            mailMessage.To.Add(item.email);
                            mailMessage.IsBodyHtml = true;
                            mailMessage.Body = value.firstName + " ได้ทำการยืนยันตัวตนบน Application สช. On Moblie กรุณาตรวจสอบการอนุมัติสิทธิ์ของชื่อผู้ใช้ " + value.username;
                            mailMessage.Subject = "แจ้งตรวจสอบการอนุมัติสิทธ์การใช้งาน สช. On Moblie";
                            client.Send(mailMessage);
                        }
                    }
                    return new Response { status = "S", message = "send Email Success.", jsonData = "" };
                }
                else
                {
                    return new Response { status = "S", message = "user admin no data.", jsonData = "" };
                }      

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("token/create")]
        public ActionResult<Response> TokenCreate([FromBody] Register value)
        {
            value.code = value.code.toCode();
            var doc = new BsonDocument();
            try
            {
                

                {
                    var col = new Database().MongoClient("register");
                    var filter = Builders<BsonDocument>.Filter.Eq("token", value.token);
                    var update = Builders<BsonDocument>.Update.Set("isOnline", false);
                    col.UpdateMany(filter, update);
                }

                {
                    var doc2 = new BsonDocument();
                    var col2 = new Database().MongoClient("register");
                    var filter2 = Builders<BsonDocument>.Filter.Eq("username", value.username);
                    doc2 = col2.Find(filter2).FirstOrDefault();
                    doc2["token"] = value.token;
                    doc2["isOnline"] = true;
                    doc2["updateDate"] = DateTime.Now.toStringFromDate();
                    col2.ReplaceOne(filter2, doc2);
                }
                
                //{
                //    var col = new Database().MongoClient("registerToken");

                //    //check duplicate
                //    {
                //        var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                //        if (col.Find(filter).Any())
                //        {
                //            return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                //        }
                //    }

                //    var col2 = new Database().MongoClient<Register>("registerToken");
                //    var filter2 = Builders<Register>.Filter.Eq("username", value.username);
                //    var docs2 = col2.Find(filter2).Project(c => new { c.code, c.username }).ToList();
                //    this.Delete(new Register { code = string.Join(",", docs2.Select(c => c.code).ToList()) });

                //    doc = new BsonDocument
                //    {
                //        { "code", value.code },
                //        { "username", value.username },
                //        { "token", value.token },
                //        { "createBy", value.createBy },
                //        { "createDate", DateTime.Now.toStringFromDate() },
                //        { "createTime", DateTime.Now.toTimeStringFromDate() },
                //        { "updateBy", value.updateBy },
                //        { "updateDate", DateTime.Now.toStringFromDate() },
                //        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                //        { "docDate", DateTime.Now.Date.AddHours(7) },
                //        { "docTime", DateTime.Now.toTimeStringFromDate() },
                //        { "isActive", true }
                //    };
                //    col.InsertOne(doc);
                //}

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /read
        [HttpPost("token/read")]
        public ActionResult<Response> TokenRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("registerToken");
                var filter = Builders<Register>.Filter.Eq(x => x.isActive, true);
                //var filter = Builders<Register>.Filter.Ne(x => x.status, "D") & (Builders<Register>.Filter.Eq(x => x.category, "guest") | Builders<Register>.Filter.Eq(x => x.category, "facebook") | Builders<Register>.Filter.Eq(x => x.category, "google") | Builders<Register>.Filter.Eq(x => x.category, "line"));
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Register>.Filter.Regex("title", value.keySearch)) | (filter & Builders<Register>.Filter.Regex("description", value.keySearch));
                }
                else
                {

                    if (!string.IsNullOrEmpty(value.username)) { filter = filter & Builders<Register>.Filter.Regex("username", value.username); }
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Register>.Filter.Regex("code", value.code); }
                    //if (!string.IsNullOrEmpty(value.token)) { filter = filter & Builders<Register>.Filter.Regex("token", value.token); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", de.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).SortByDescending(o => o.createDate).Project(c => new
                {
                    c.code,
                    c.username,
                    c.token,
                    c.isActive,
                    c.createBy,
                    c.createDate,
                    c.imageUrl,
                    c.updateBy,
                    c.updateDate,
                    c.createTime,
                    c.updateTime,
                    c.docDate,
                    c.docTime
                }).ToList();

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
        [HttpPost("line/read")]
        public ActionResult<Response> LineRead([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = new { username = value.username, email = value.email, firstName = value.firstName, imageUrl = value.imageUrl, lineID = value.lineID } };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        #endregion

        #region notification

        // POST /read
        [HttpPost("notification/read")]
        public ActionResult<Response> NotificationRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<RegisterNotification>("registerNotification");
                var filter = Builders<RegisterNotification>.Filter.Eq(x => x.username, value.username);
                //& (Builders<RegisterNotification>.Filter.Eq(x => x.category, "guest") | Builders<RegisterNotification>.Filter.Eq(x => x.category, "facebook") | Builders<RegisterNotification>.Filter.Eq(x => x.category, "google") | Builders<RegisterNotification>.Filter.Eq(x => x.category, "line"));
                var doc = col.Find(filter).Project(c => new { c.code, c.username, c.mainPage, c.newsPage, c.eventPage, c.contactPage, c.knowledgePage, c.privilegePage, c.poiPage, c.pollPage, c.suggestionPage, c.reporterPage, c.trainingPage, c.welfarePage, c.fundPage, c.cooperativeFormPage, c.isActive, c.createBy, c.createDate, c.updateBy, c.updateDate, c.createTime, c.updateTime, c.docDate, c.docTime, c.category }).FirstOrDefault();

                if (doc == null)
                {
                    {
                        var notiCol = new Database().MongoClient("registerNotification");

                        var docNoti = new BsonDocument
                        {
                            { "code", "".toCode() },
                            { "username", value.username },
                            { "mainPage", true },
                            { "newsPage", true },
                            { "eventPage", true },
                            { "contactPage", true },
                            { "knowledgePage", true },
                            { "privilegePage", true },
                            { "poiPage", true },
                            { "pollPage", true },
                            { "suggestionPage", true },
                            { "reporterPage", true },
                            { "trainingPage", true },
                            { "welfarePage", true },
                            { "warningPage", true },
                            { "fundPage", true },
                            { "cooperativeFormPage", true },
                            { "createBy", value.createBy },
                            { "createDate", DateTime.Now.toStringFromDate() },
                            { "createTime", DateTime.Now.toTimeStringFromDate() },
                            { "updateBy", value.updateBy },
                            { "updateDate", DateTime.Now.toStringFromDate() },
                            { "updateTime", DateTime.Now.toTimeStringFromDate() },
                            { "docDate", DateTime.Now.Date.AddHours(7) },
                            { "docTime", DateTime.Now.toTimeStringFromDate() },
                            { "isActive", true }
                        };
                        notiCol.InsertOne(docNoti);

                    }
                    return new Response { status = "S", message = "success", jsonData = new RegisterNotification { username = value.username, mainPage = true, newsPage = true, eventPage = true, contactPage = true, knowledgePage = true, privilegePage = true, poiPage = true, pollPage = true, suggestionPage = true, reporterPage = true, trainingPage = true, welfarePage = true, warningPage = true }.ToJson(), objectData = new RegisterNotification { username = value.username, mainPage = true, newsPage = true, eventPage = true, contactPage = true, knowledgePage = true, privilegePage = true, poiPage = true, pollPage = true, suggestionPage = true, reporterPage = true, trainingPage = true, welfarePage = true, warningPage = true } };
                }
                else
                    return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = doc };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("notification/update")]
        public ActionResult<Response> NotificationUpdate([FromBody] RegisterNotification value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("registerNotification");

                var filter = Builders<BsonDocument>.Filter.Eq("username", value.username);
                //& (Builders<BsonDocument>.Filter.Eq("category", "guest") | Builders<BsonDocument>.Filter.Eq("category", "facebook") | Builders<BsonDocument>.Filter.Eq("category", "google") | Builders<BsonDocument>.Filter.Eq("category", "line"));
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                doc["mainPage"] = value.mainPage;
                doc["newsPage"] = value.newsPage;
                doc["eventPage"] = value.eventPage;
                doc["contactPage"] = value.contactPage;
                doc["knowledgePage"] = value.knowledgePage;
                doc["privilegePage"] = value.privilegePage;
                doc["poiPage"] = value.poiPage;
                doc["pollPage"] = value.pollPage;
                doc["suggestionPage"] = value.suggestionPage;
                doc["reporterPage"] = value.reporterPage;
                doc["trainingPage"] = value.trainingPage;
                doc["welfarePage"] = value.welfarePage;
                doc["warningPage"] = value.warningPage;
                doc["fundPage"] = value.fundPage;
                doc["cooperativeFormPage"] = value.cooperativeFormPage;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                col.ReplaceOne(filter, doc);

                //BEGIN :read >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>


                var registerCol = new Database().MongoClient<RegisterNotification>("registerNotification");
                var registerFilter = Builders<RegisterNotification>.Filter.Eq(x => x.username, value.username);
                //& (Builders<RegisterNotification>.Filter.Eq(x => x.category, "guest") | Builders<RegisterNotification>.Filter.Eq(x => x.category, "facebook") | Builders<RegisterNotification>.Filter.Eq(x => x.category, "google") | Builders<RegisterNotification>.Filter.Eq(x => x.category, "line"));
                var registerDoc = registerCol.Find(registerFilter).Project(c => new { c.code, c.username, c.mainPage, c.newsPage, c.eventPage, c.contactPage, c.knowledgePage, c.privilegePage, c.poiPage, c.pollPage, c.suggestionPage, c.welfarePage, c.warningPage, c.reporterPage, c.trainingPage, c.fundPage, c.cooperativeFormPage, c.isActive, c.createBy, c.createDate, c.updateBy, c.updateDate, c.createTime, c.updateTime, c.docDate, c.docTime, c.category }).FirstOrDefault();

                //END :read <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                return new Response { status = "S", message = "success", jsonData = registerDoc.ToJson(), objectData = registerDoc };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion      

    }
}