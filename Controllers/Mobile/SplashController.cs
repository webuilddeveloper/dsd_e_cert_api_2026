using System;
using System.Linq;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class SplashController : Controller
    {
        public SplashController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Splash>( "splash");

                var filter = Builders<Splash>.Filter.Eq("isActive", true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Splash>.Filter.Regex("code", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).Skip(value.skip).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Limit(value.limit).SortBy( o => o.sequence).Project(c => new { c.imageUrl, c.timeOut }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }
    }
}