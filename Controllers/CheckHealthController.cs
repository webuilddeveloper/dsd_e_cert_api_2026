using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class CheckHealthController : Controller
    {
        public CheckHealthController() { }

        // GET
        [HttpGet("")]
        public ActionResult<Response> Create()
        {
            try
            {
                var memory = 0.0;
                using (Process proc = Process.GetCurrentProcess())
                {
                    // The proc.PrivateMemorySize64 will returns the private memory usage in byte.
                    // Would like to Convert it to Megabyte? divide it by 2^20
                    memory = proc.PrivateMemorySize64 / (1024 * 1024);
                }

                return new Response { status = "S", message = "success", objectData = memory };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

    }
}