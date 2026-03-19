using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class MongoDBController : Controller
    {
        public MongoDBController() { }

        // GET /login
        [HttpGet("")]
        public ActionResult<IEnumerable<string>> Gets()
        {
            try
            {
                var dbClient = new MongoClient("mongodb://202.139.196.4");
                //var dbClient = new MongoClient("mongodb://122.155.223.63");
                var dbList = dbClient.ListDatabases().ToList();

                //Console.WriteLine("The list of databases are:");

                //foreach (var item in dbList)
                //{
                //    Console.WriteLine(item);
                //}

                return new string[] { string.Join(", ", dbList) };
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                return new string[] { ex.Message };
            }
      
        }

        // GET /login/5
        [HttpGet("{collection}")]
        public ActionResult<string> GetById(string collection)
        {
            try
            {
                //[username:password@]hostname[:port][/[database][?options]]
                //var dbClient = new MongoClient("mongodb://myUserAdmin:iChxj7$y@122.155.223.63:27017/admin");
                var dbClient = new MongoClient("mongodb://kt-tded.com");
                IMongoDatabase db = dbClient.GetDatabase("ddpm");

                var register = db.GetCollection<BsonDocument>(collection);
                var documents = register.Find(new BsonDocument()).ToList();

                //foreach (BsonDocument doc in documents)
                //{
                //    Console.WriteLine(doc.ToString());
                //}

                return string.Join(", ", documents);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                return ex.Message;
            }
        }

        // POST /login
        [HttpPost("")]
        public ActionResult<ResponseX> Post([FromBody] Login value) {

            if (value.username == "admin" && value.password == "admin")
            {
                return new ResponseX { username = value.username, token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6ImFkbWluIiwicGFzc3dvcmQiOiJhZG1pbiJ9.rFcqI_6iHyIx450Esqa3yXqyZLhPhKt9eKeHcnjYujQ", message = "Log on: " + DateTime.Now.ToString("dd-MM-yyyy"), status = true };
            }
            else
            {
                return new ResponseX { username = value.username, token = "", message = "Log Failed: " + DateTime.Now.ToString("dd-MM-yyyy"), status = false };
            }

        }
    }
}