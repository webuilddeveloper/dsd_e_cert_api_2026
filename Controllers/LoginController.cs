using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class LoginController : Controller
    {
        public LoginController() { }

        // GET /login
        [HttpGet("")]
        public ActionResult<IEnumerable<string>> Gets()
        {
            //var xx = new GeodesicDistance().distance(32.9697, -96.80322, 29.46786, -98.53506, 'K');
            //return new string[] { xx.ToString(), "value1", "value2" };
            return new string[] { "service:5000", "value1", "value2" };
        }

        // GET /login/5
        [HttpGet("{id}")]
        public ActionResult<string> GetById(int id)
        {
            return "value" + id;
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