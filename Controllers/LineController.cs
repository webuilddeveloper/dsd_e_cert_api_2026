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
    public class LineController : Controller
    {
        public LineController() { }

        #region main

        // POST /create
        [HttpPost]
        public void Post([FromBody] Line value)
        {
            try
            {
                
                // Create Log
                {
                    var doc = new BsonDocument();
                    var col = new Database().MongoClient("log_" + "line");
                    doc = new BsonDocument
                    {
                        { "code", "".toCode() },
                        { "raw", JsonConvert.SerializeObject(value) },
                        { "createBy", "" },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", "" },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true }
                    };
                    col.InsertOne(doc);
                }

                if (value.events[0].type == "message")
                {
                    if (value.events[0].message.text == "Register Now")
                    {
                        var payload = new Push();
                        payload.messages = new List<PushMessage>
                        {
                            new PushMessage {
                                type = "template",
                                altText = "ลงทะเบียน",
                                template = new PushTemplate
                                {
                                    type = "image_carousel",
                                    columns = new List<PushColumns>
                                    {
                                        new PushColumns
                                        {
                                            imageUrl = "https://cio-training.onde.go.th/document/images/line/register001.png",
                                            action = new PushAction
                                            {
                                                type = "postback",
                                                label = "สมัครสมาชิก",
                                                data = "action=buy&itemid=111"
                                            }
                                        },
                                        new PushColumns
                                        {
                                            imageUrl = "https://cio-training.onde.go.th/document/images/line/register002.png",
                                            action = new PushAction
                                            {
                                                type = "message",
                                                label = "ยืนยันตัวตน",
                                                text = "ตกลง"
                                            }
                                        }
                                    }
                                }
                            }
                        };

                        HttpClient client = new HttpClient();
                        client.DefaultRequestHeaders.Add("accept", "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "ILEXZGLhi6NPDyAmYFNLmV6RVy1kCBLkIEmcG04vJHO8y24CfWvMfNIyUnOoX80knHbHxmG6sdfdNJLZFCog/yJBQdigrcbaqbj///fYFpyeo6omg8Y/2sMcOOit6H5bnnhU37WpJSeJwHHRGwhdOwdB04t89/1O/w1cDnyilFU=");
                        //client.DefaultRequestHeaders.Add("accept-encoding", "gzip,deflate");

                        var json = JsonConvert.SerializeObject(payload);
                        HttpContent httpContent = new StringContent(json);
                        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        client.PostAsync("https://api.line.me/v2/bot/message/broadcast", httpContent);
                    }
                    else if (value.events[0].message.text == "Privilege List")
                    {
                        var payload = new Push();
                        payload.messages = new List<PushMessage>
                        {
                            new PushMessage {
                                type = "template",
                                altText = "สิทธิประโยชน์",
                                template = new PushTemplate
                                {
                                    type = "carousel",
                                    imageAspectRatio = "rectangle",
                                    imageSize = "cover",
                                    columns = new List<PushColumns>
                                    {
                                        new PushColumns
                                        {
                                            thumbnailImageUrl = "https://d-api.feyverly-crm.com/storage/155/Pun-Thai_DoubleA_Resize.jpg",
                                            imageBackgroundColor = "#FFFFFF",
                                            title = "กาแฟพันธุ์ไทย",
                                            text = "รับคูปองส่วนลดเงินสดมูลค่า 50 บาท",
                                            defaultAction = new PushDefaultAction
                                            {
                                                type = "uri",
                                                label = "รายละเอียด",
                                                uri = "https://www.google.com/"
                                            },
                                            actions = new List<PushAction>{
                                                new PushAction
                                                {
                                                    type = "postback",
                                                    label = "รับสิทธิ์",
                                                    data = "action=buy&itemid=111"
                                                }
                                            }  
                                        },
                                        new PushColumns
                                        {
                                            thumbnailImageUrl = "https://d-api.feyverly-crm.com/storage/156/Major_DoubleA_Resize.jpg",
                                            imageBackgroundColor = "#FFFFFF",
                                            title = "Major Cineplex",
                                            text = "แลกรับบัตรชมทุกสาขาทั่วประเทศ ฟรี 1 ที่นั่ง",
                                            defaultAction = new PushDefaultAction
                                            {
                                                type = "uri",
                                                label = "รายละเอียด",
                                                uri = "https://www.google.com/"
                                            },
                                            actions = new List<PushAction>{
                                                new PushAction
                                                {
                                                    type = "postback",
                                                    label = "รับสิทธิ์",
                                                    data = "action=buy&itemid=111"
                                                }
                                            }
                                        },
                                        new PushColumns
                                        {
                                            thumbnailImageUrl = "https://d-api.feyverly-crm.com/storage/157/Starbuck_DoubleA_Resize.jpg",
                                            imageBackgroundColor = "#FFFFFF",
                                            title = "Starbucks Coffee",
                                            text = "แลกรับ Starbucks Coffee e-Coupon มูลค่า 400 บาท",
                                            defaultAction = new PushDefaultAction
                                            {
                                                type = "uri",
                                                label = "รายละเอียด",
                                                uri = "https://www.google.com/"
                                            },
                                            actions = new List<PushAction>{
                                                new PushAction
                                                {
                                                    type = "postback",
                                                    label = "รับสิทธิ์",
                                                    data = "action=buy&itemid=111"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        };

                        HttpClient client = new HttpClient();
                        client.DefaultRequestHeaders.Add("accept", "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "ILEXZGLhi6NPDyAmYFNLmV6RVy1kCBLkIEmcG04vJHO8y24CfWvMfNIyUnOoX80knHbHxmG6sdfdNJLZFCog/yJBQdigrcbaqbj///fYFpyeo6omg8Y/2sMcOOit6H5bnnhU37WpJSeJwHHRGwhdOwdB04t89/1O/w1cDnyilFU=");
                        //client.DefaultRequestHeaders.Add("accept-encoding", "gzip,deflate");

                        var json = JsonConvert.SerializeObject(payload);
                        HttpContent httpContent = new StringContent(json);
                        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        client.PostAsync("https://api.line.me/v2/bot/message/broadcast", httpContent);
                    }
                    else
                    {
                        // Create Issue Tracking
                        {
                            var doc = new BsonDocument();
                            var col = new Database().MongoClient("issueTracking");
                            doc = new BsonDocument
                            {
                                { "code", "".toCode() },
                                { "title", value.events[0].message.text },
                                { "createBy", value.events[0].source.userId },
                                { "createDate", DateTime.Now.toStringFromDate() },
                                { "createTime", DateTime.Now.toTimeStringFromDate() },
                                { "updateBy", value.events[0].source.userId },
                                { "updateDate", DateTime.Now.toStringFromDate() },
                                { "updateTime", DateTime.Now.toTimeStringFromDate() },
                                { "docDate", DateTime.Now.Date.AddHours(7) },
                                { "docTime", DateTime.Now.toTimeStringFromDate() },
                                { "status", "N" },
                                { "isActive", false }
                            };
                            col.InsertOne(doc);
                        }
                    }
                    
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<News>("issueTracking");
                var filter = (Builders<News>.Filter.Ne("status", "D"));
                //& value.filterOrganization<News>());

                if (!string.IsNullOrEmpty(value.keySearch))
                    filter = (filter & Builders<News>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                var docs = col.Find(filter)
                    .SortByDescending(o => o.docDate)
                    .ThenByDescending(o => o.updateTime)
                    .Skip(value.skip)
                    .Limit(value.limit)
                    .Project(c => new {
                        c.code,
                        c.title,
                        c.status,
                        c.isActive,
                        c.createBy,
                        c.createDate,
                        c.updateBy,
                        c.updateDate,
                        c.lv0,
                        c.lv1,
                        c.lv2,
                        c.lv3 })
                    .ToList();

                return new Response { status = "S", message = "success", jsonData = "", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] News value)
        {
            try
            {
                var col = new Database().MongoClient("issueTracking");
                var codeList = value.code.Split(",");

                foreach (var code in codeList)
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("status", "D")
                        .Set("updateBy", value.updateBy)
                        .Set("updateDate", DateTime.Now.toStringFromDate())
                        .Set("updateTime", DateTime.Now.toTimeStringFromDate());
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
    }
}