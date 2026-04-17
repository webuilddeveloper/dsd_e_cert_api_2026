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

namespace mobilev2_api.Controllers
{
    [Route("m/v2/[controller]")]
    public class RegisterController : Controller
    {
        public RegisterController() { }

        // POST /login
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");
                filter &= Builders<Register>.Filter.Eq("code", value.code);

                var idcard = col.Find(filter).Project(c => c.idcard ).FirstOrDefault();
                var driverLicence = new Database().MongoClient<driverLicenceInfobase>("driverLicence")
                    .Find(Builders<driverLicenceInfobase>.Filter.Eq("docNo", idcard)
                    & Builders<driverLicenceInfobase>.Filter.Eq("reference", value.code)
                    & Builders<driverLicenceInfobase>.Filter.Ne("status", "D")
                    ).ToList();

                var doc = col.Find(filter).Project(c => new
                {
                    c.code,
                    c.idcard,
                    c.username,
                    c.password,
                    c.category,
                    c.imageUrl,
                    c.firstName,
                    c.lastName,
                    c.birthDay,
                    c.phone,
                    c.email,
                    c.countUnit,
                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,
                    c.lv0List,
                    c.lv1List,
                    c.lv2List,
                    c.lv3List,
                    c.lv4List,
                    isDF = c.status == "A" ? true:false,
                    driverLicence = driverLicence.FirstOrDefault(c => c.isActive)
                }).FirstOrDefault();
                return new Response { status = "S", message = "success", objectData = doc };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        //[HttpPost("update")]
        //public ActionResult<Response> Update([FromBody] Register value)
        //{

        //    try
        //    {
        //        var col = new Database().MongoClient("register");
        //        var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
        //        var doc = new BsonDocument();
        //        doc = col.Find(filter).FirstOrDefault();
        //        doc["firstName"] = value.firstName;
        //        doc["lastName"] = value.lastName;
        //        doc["birthDay"] = value.birthDay;
        //        doc["phone"] = value.phone;
        //        doc["email"] = value.email;
        //        doc["imageUrl"] = value.imageUrl;
        //        col.ReplaceOne(filter, doc);

        //        var result = Read(new Register { code = value.code });
        //        return new Response { status = "S", message = "Success", objectData = "บันทึกรายการเรียบร้อย" };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Response { status = "E", message = ex.Message };
        //    }
        //}

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                value.logCreate("verify/update", value.code);

                var col = new Database().MongoClient("register");
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                //var model = BsonSerializer.Deserialize<object>(doc);

                //if (value.category != "guest")
                //{
                //    doc["username"] = value.username;
                //}

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
                //doc["line"] = value.line;
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
                //doc["job"] = value.job;
                doc["idcard"] = value.idcard;
                doc["officerCode"] = value.officerCode;
                //doc["countUnit"] = value.countUnit;
                //doc["lv0"] = value.lv0;
                //doc["lv1"] = value.lv1;
                //doc["lv2"] = value.lv2;
                //doc["lv3"] = value.lv3;
                //doc["lv4"] = value.lv4;
                //doc["linkAccount"] = value.linkAccount;
                doc["description"] = value.description;
                doc["isActive"] = value.isActive;
                doc["status"] = value.status;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                col.ReplaceOne(filter, doc);

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
                    //c.line,
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
                    //c.job,
                    c.idcard,
                    c.officerCode,
                    c.countUnit,
                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,
                    c.linkAccount
                }).FirstOrDefault();

                return new Response { status = "S", message = "success", jsonData = registerDoc.ToJson(), objectData = registerDoc };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("verify/update")]
        public ActionResult<Response> VerifyUpdate([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                value.logCreate("verify/update", value.code);
            
                var col = new Database().MongoClient("register");
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                //var model = BsonSerializer.Deserialize<object>(doc);

                //if (value.category != "guest")
                //{
                //    doc["username"] = value.username;
                //}

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
                //doc["line"] = value.line;
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
                //doc["job"] = value.job;
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
                col.ReplaceOne(filter, doc);

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
                    //c.line,
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
                    //c.job,
                    c.idcard,
                    c.officerCode,
                    c.countUnit,
                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,
                    c.linkAccount
                }).FirstOrDefault();

                return new Response { status = "S", message = "success", jsonData = registerDoc.ToJson(), objectData = registerDoc };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /login
        [HttpPost("facebook/login")]
        public ActionResult<Response> FacebookLogin([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");
                filter &= Builders<Register>.Filter.Eq("category", "facebook");
                filter &= Builders<Register>.Filter.Eq("facebookID", value.facebookID);

                //var idcard = col.Find(filter).Project(c => new { c.idcard, c.code }).FirstOrDefault();
                //var driverLicence = new Database().MongoClient<driverLicenceInfobase>("driverLicence")
                //    .Find(Builders<driverLicenceInfobase>.Filter.Eq("docNo", idcard?.idcard ?? "")
                //    & Builders<driverLicenceInfobase>.Filter.Eq("reference", idcard?.code ?? "")
                //    & Builders<driverLicenceInfobase>.Filter.Ne("status", "D")
                //    ).ToList();
                var doc = col.Find(filter).Project(c => new
                {
                    c.idcard,
                    c.code,
                    c.username,
                    c.password,
                    c.category,
                    c.imageUrl,
                    c.firstName,
                    c.lastName,
                    c.countUnit,
                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,
                    c.lv0List,
                    c.lv1List,
                    c.lv2List,
                    c.lv3List,
                    c.lv4List,
                    isDF = c.status == "A" ? true : false,
                    //driverLicence = idcard != null ? driverLicence.FirstOrDefault(c => c.isActive) : new driverLicenceInfobase()
                }).FirstOrDefault();

                if (doc == null)
                {
                    // insert record and read again
                    value.category = "facebook";
                    this.create(value);
                    var newDoc = col.Find(filter).Project(c => new { c.idcard, c.code, c.username, c.password, c.category, c.prefixName, c.firstName, c.lastName, c.imageUrl, c.email, c.phone, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List,
                        isDF = c.status == "A" ? true : false,
                        //driverLicence = driverLicence.FirstOrDefault(c => c.isActive)
                    }).FirstOrDefault();
                    return new Response { status = "S", message = "success", objectData = newDoc };
                }
                else
                {
                    // read current record
                    return new Response { status = "S", message = "success", objectData = doc };
                }
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /login
        [HttpPost("google/login")]
        public ActionResult<Response> GoogleLogin([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");
                filter &= Builders<Register>.Filter.Eq("category", "google");
                filter &= Builders<Register>.Filter.Eq("googleID", value.googleID);


                //var idcard = col.Find(filter).Project(c => new { c.idcard, c.code }).FirstOrDefault();
                //var driverLicence = new Database().MongoClient<driverLicenceInfobase>("driverLicence")
                //    .Find(Builders<driverLicenceInfobase>.Filter.Eq("docNo", idcard?.idcard ?? "")
                //    & Builders<driverLicenceInfobase>.Filter.Eq("reference", idcard?.code ?? "")
                //    & Builders<driverLicenceInfobase>.Filter.Ne("status", "D")
                //    ).ToList();
                var doc = col.Find(filter).Project(c => new { c.idcard, c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List,
                    isDF = c.status == "A" ? true : false,
                    //driverLicence = idcard != null ? driverLicence.FirstOrDefault(c => c.isActive) : new driverLicenceInfobase()
                }).FirstOrDefault();

                if (doc == null)
                {
                    // insert record and read again
                    value.category = "google";
                    this.create(value);
                    var newDoc = col.Find(filter).Project(c => new { c.idcard, c.code, c.username, c.password, c.category, c.prefixName, c.firstName, c.lastName, c.imageUrl, c.email, c.phone, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List,
                        isDF = c.status == "A" ? true : false,
                        //driverLicence = driverLicence.FirstOrDefault(c => c.isActive)
                    }).FirstOrDefault();
                    return new Response { status = "S", message = "success", objectData = newDoc };
                }
                else
                {
                    // read current record
                    return new Response { status = "S", message = "success", objectData = doc };
                }
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /login
        [HttpPost("apple/login")]
        public ActionResult<Response> AppleLogin([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");
                filter &= Builders<Register>.Filter.Eq("category", "apple");
                filter &= Builders<Register>.Filter.Eq("appleID", value.appleID);


                //var idcard = col.Find(filter).Project(c => new { c.idcard, c.code }).FirstOrDefault();
                //var driverLicence = new Database().MongoClient<driverLicenceInfobase>("driverLicence")
                //    .Find(Builders<driverLicenceInfobase>.Filter.Eq("docNo", idcard?.idcard ?? "")
                //    & Builders<driverLicenceInfobase>.Filter.Eq("reference", idcard?.code ?? "")
                //    & Builders<driverLicenceInfobase>.Filter.Ne("status", "D")
                //    ).ToList();
                var doc = col.Find(filter).Project(c => new {
                    c.idcard,
                    c.code,
                    c.username,
                    c.password,
                    c.category,
                    c.imageUrl,
                    c.firstName,
                    c.lastName,
                    c.countUnit,
                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,
                    c.lv0List,
                    c.lv1List,
                    c.lv2List,
                    c.lv3List,
                    c.lv4List,
                    isDF = c.status == "A" ? true : false,
                    //driverLicence = idcard != null ? driverLicence.FirstOrDefault(c => c.isActive) : new driverLicenceInfobase()
                }).FirstOrDefault();

                if (doc == null)
                {
                    // insert record and read again
                    value.category = "apple";
                    this.create(value);
                    var newDoc = col.Find(filter).Project(c => new {
                        c.idcard,
                        c.code,
                        c.username,
                        c.password,
                        c.category,
                        c.prefixName,
                        c.firstName,
                        c.lastName,
                        c.imageUrl,
                        c.email,
                        c.phone,
                        c.countUnit,
                        c.lv0,
                        c.lv1,
                        c.lv2,
                        c.lv3,
                        c.lv4,
                        c.lv0List,
                        c.lv1List,
                        c.lv2List,
                        c.lv3List,
                        c.lv4List,
                        isDF = c.status == "A" ? true : false,
                        //driverLicence = driverLicence.FirstOrDefault(c => c.isActive)
                    }).FirstOrDefault();
                    return new Response { status = "S", message = "success", objectData = newDoc };
                }
                else
                {
                    // read current record
                    return new Response { status = "S", message = "success", objectData = doc };
                }
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /login
        [HttpPost("line/login")]
        public ActionResult<Response> LineLogin([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");
                filter &= Builders<Register>.Filter.Eq("category", "line");
                filter &= Builders<Register>.Filter.Eq("lineID", value.lineID);

                //var idcard = col.Find(filter).Project(c => new { c.idcard, c.code }).FirstOrDefault();
                //var driverLicence = new Database().MongoClient<driverLicenceInfobase>("driverLicence")
                //    .Find(Builders<driverLicenceInfobase>.Filter.Eq("docNo", idcard?.idcard ?? "")
                //    & Builders<driverLicenceInfobase>.Filter.Eq("reference", idcard?.code ?? "")
                //    & Builders<driverLicenceInfobase>.Filter.Ne("status", "D")
                //    ).ToList();

                var doc = col.Find(filter).Project(c => new { c.idcard, c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List,
                    isDF = c.status == "A" ? true : false,
                    //driverLicence = idcard != null ? driverLicence.FirstOrDefault(c => c.isActive) : new driverLicenceInfobase()
                }).FirstOrDefault();

                if (doc == null)
                {
                    // insert record and read again
                    value.category = "line";
                    this.create(value);
                    var newDoc = col.Find(filter).Project(c => new { c.idcard, c.code, c.username, c.password, c.category, c.prefixName, c.firstName, c.lastName, c.imageUrl, c.email, c.phone, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List,
                        isDF = c.status == "A" ? true : false,
                        //driverLicence = driverLicence.FirstOrDefault(c => c.isActive)
                    }).FirstOrDefault();
                    return new Response { status = "S", message = "success", objectData = newDoc };
                }
                else
                {
                    var col2 = new Database().MongoClient("register");
                    var filter2 = Builders<BsonDocument>.Filter.Ne(x => x["status"], "D");
                    filter2 &= Builders<BsonDocument>.Filter.Eq("category", "line");
                    filter2 &= Builders<BsonDocument>.Filter.Eq("lineID", value.lineID);
                    var doc2 = col2.Find(filter2).FirstOrDefault();
                    if (!string.IsNullOrEmpty(value.imageUrl)) { doc2["imageUrl"] = value.imageUrl; }
                    col2.ReplaceOne(filter2, doc2);
                    // read current record
                    return new Response { status = "S", message = "success", objectData = doc };
                }
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        private void create(Register value)
        {
            value.code = "".toCode();

            //insert record
            var newCol = new Database().MongoClient("register");

            //check duplicate
            var newFilter = Builders<BsonDocument>.Filter.Eq("code", value.code);
            if (newCol.Find(newFilter).Any())
                value.code = "".toCode();

            //return new Response { status = "E", message = $"code: {value.code} is exist", objectData = value };

            var newDoc = new BsonDocument
            {
                { "code", value.code },
                { "imageUrl", value.imageUrl },
                { "category", value.category },
                { "username", value.username },
                { "password", "" },
                { "prefixName", value.prefixName },
                { "firstName", value.firstName },
                { "lastName", value.lastName },
                { "email", value.email },
                { "appleID", value.appleID },
                { "facebookID", value.facebookID },
                { "googleID", value.googleID },
                { "lineID", value.lineID },
                { "countUnit", "[]" },
                { "lv0", "" },
                { "lv1", "" },
                { "lv2", "" },
                { "lv3", "" },
                { "lv4", "" },

                { "createBy", value.createBy },
                { "createDate", DateTime.Now.toStringFromDate() },
                { "createTime", DateTime.Now.toTimeStringFromDate() },
                { "updateBy", value.updateBy },
                { "updateDate", DateTime.Now.toStringFromDate() },
                { "updateTime", DateTime.Now.toTimeStringFromDate() },
                { "docDate", DateTime.Now.Date.AddHours(7) },
                { "docTime", DateTime.Now.toTimeStringFromDate() },
                { "isActive", true },
                { "status", "N" },
            };
            newCol.InsertOne(newDoc);
        }
    }
}