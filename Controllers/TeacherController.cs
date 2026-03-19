using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class TeacherController : Controller
    {
        public TeacherController() { }

        #region main

        // POST /create
        [HttpPost("read")]
        public async System.Threading.Tasks.Task<ActionResult<Response>> CreateAsync([FromBody] Criteria value)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("X-Office-Request", "WEBUILD");
                client.DefaultRequestHeaders.Add("X-Office-Key", "970A52B19165B169A9B942CDAA4C8073054981B6");

                if (value.firstName == "")
                    value.firstName = "@";

                if (value.lastName == "")
                    value.lastName = "@";

                String path = "https://regis-api.opec.go.th/api/v1/GetEmployeeOpec/" + value.firstName + "/" + value.lastName;

                var response =await client.GetAsync(path);
                var contents = await response.Content.ReadAsStringAsync();

                if (contents == "")
                {
                    return new Response { status = "E", message = "data not found.", jsonData = value.ToJson(), objectData = new TeacherList() };
                }
                try
                {
                    // response array data.
                    TeacherList json = JsonConvert.DeserializeObject<TeacherList>(contents);
                    return new Response { status = "S", message = "success", jsonData = json.ToJson(), objectData = json };
                }
                catch
                {
                    // response object data.
                    Teacher jsonRaw = JsonConvert.DeserializeObject<Teacher>(contents);
                    TeacherList json = new TeacherList();
                    json.EmployeeOpec.Add(jsonRaw.EmployeeOpec);
                    return new Response { status = "S", message = "success", jsonData = json.ToJson(), objectData = json };
                }

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message =  "is exist", jsonData = value.ToJson(), objectData = value };
            }

        }
        #endregion

    }
}