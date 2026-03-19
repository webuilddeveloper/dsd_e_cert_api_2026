using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Models;
using cms_api.Extension;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class GroupByController : Controller
    {

        public GroupByController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                value.statisticsCreate(value.title);
                var col = new Database().MongoClient<News>(value.title);
                var filter = (Builders<News>.Filter.Ne("status", "D") & value.filterOrganization<News>());

                if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<News>.Filter.Eq("sequence", sequence); }

                var docs = col.Find(filter).Project(c => new { c.sequence }).ToList();

                var result = docs.GroupBy(c => c.sequence).Select(c => new { value = c.Key, display = c.Key }).ToList();

                return new Response { status = "S", message = "success", objectData = result };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

    }
}