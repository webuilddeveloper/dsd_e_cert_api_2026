using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class TechnicianController : Controller
    {
        public TechnicianController() { }

        [HttpPost("updatepdpa")]
        public ActionResult<Response> UpdatePDPA([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                value.logCreate("verify/update", value.code);

                var col = new Database().MongoClient("register");
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();

                doc["isPdpa"] = value.isPdpa;
               
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success" };

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

                var col = new Database().MongoClient<Register>("register");
                //var filter = (Builders<Register>.Filter.Eq(x => x.isActive, true || false));
                //&value.filterOrganization<Register>()
                var filter = Builders<Register>.Filter.Ne("status", "D") & Builders<Register>.Filter.Eq("isPdpa", true) & (Builders<Register>.Filter.Eq(x => x.category, "guest") | Builders<Register>.Filter.Eq(x => x.category, "facebook") | Builders<Register>.Filter.Eq(x => x.category, "google") | Builders<Register>.Filter.Eq(x => x.category, "line") | Builders<Register>.Filter.Eq(x => x.category, "apple") | Builders<Register>.Filter.Eq(x => x.category, "thaid"));

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter =  (filter & Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Register>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }

                if (!string.IsNullOrEmpty(value.firstName))
                {
                    filter = filter & Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.firstName), "i"));
                }

                if (!string.IsNullOrEmpty(value.lastName))
                {
                    filter = filter & Builders<Register>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.lastName), "i"));
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
                    c.appleID,
                    c.isCert,
                    c.isInterest,
                    c.isPdpa,

                }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }
    }
}