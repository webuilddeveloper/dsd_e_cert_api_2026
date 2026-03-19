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
    public class OrganizationController : Controller
    {
        public OrganizationController() { }

        // POST /login
        [HttpPost("image/read")]
        public ActionResult<Response> Read([FromBody] Register value)
        {
            try
            {
                var result = new List<object>();

                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Eq(x => x.code, value.code);
                var doc = col.Find(filter).Project(c => new { c.code, c.countUnit, c.status }).FirstOrDefault();

                if (!string.IsNullOrEmpty(doc.countUnit))
                {
                    if (doc.countUnit != "[]")
                    {
                        var model = BsonSerializer.Deserialize<List<CountUnit>>(doc.countUnit);

                        model.ForEach(c =>
                        {
                            if (c.status == "A")
                            {
                                //get imageUrl organization lv0
                                var colOrganization = new Database().MongoClient<News>("organization");
                                var filterOrganization = Builders<News>.Filter.Eq("code", c.lv0);
                                var docOrganization = colOrganization.Find(filterOrganization).Project(c => new { c.imageUrl }).FirstOrDefault();

                                result.Add(new { imageUrl = docOrganization.imageUrl });
                            }
                        });
                    }
                    else
                    {
                        switch (doc.status)
                        {
                            case "N":
                                result.Add(new { title = "รอการยืนยันตัวตน", imageUrl = "" });
                                break;
                            case "V":
                                result.Add(new { title = "รอเจ้าหน้าที่ตรวจสอบข้อมูล", imageUrl = "" });
                                break;
                            default:
                                break;
                        }
                    }
                }
                return new Response { status = "S", message = "success", objectData = result };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

    }
}