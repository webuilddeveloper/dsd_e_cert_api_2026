using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class OrganizationController : Controller
    {
        public OrganizationController() { }

        #region main

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<News>( "organization");

                var filter = Builders<News>.Filter.Eq("isActive", true) & Builders<News>.Filter.Eq("category", value.category);

                var docs = col.Find(filter).Project(c => new { c.code, c.title, c.category, c.imageUrl, c.createBy, c.createDate, c.updateBy, c.updateDate, c.isActive }).ToList();

                var model = new List<object>();
                docs.ForEach(c =>
                {
                    model.Add(new { title = c.title, label = c.title, value = c.code, imageUrl = c.imageUrl });
                });

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