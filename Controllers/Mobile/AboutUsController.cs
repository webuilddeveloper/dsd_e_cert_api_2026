using System;
using System.Linq;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class AboutUsController : Controller
    {
        public AboutUsController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<AboutUs>("aboutUs");

                var filter = Builders<AboutUs>.Filter.Eq("code", "1");
                //filter = filter | Builders<AboutUs>.Filter.Eq("isActive", false);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<AboutUs>.Filter.Regex("code", value.code); }

                var docs = col.Find(filter).Project(c => new { c.code, c.isActive, c.title, c.imageLogoUrl, c.imageBgUrl, c.description, c.latitude, c.email, c.site, c.longitude, c.address, c.facebook, c.youtube, c.telephone, c.createBy, c.createDate, c.updateBy, c.updateDate, c.lineOfficial }).FirstOrDefault();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }
    }
}