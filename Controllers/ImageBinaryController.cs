using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace cms_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageBinaryController : ControllerBase
    {
        // POST /read
        [HttpPost("read")]
        public async Task<ImageBinary> Read([FromBody] Criteria value)
        {
            var result = new ImageBinary();

            using (WebClient client = new WebClient())
            {
                //byte[] bytes = await client.DownloadDataTaskAsync("http://202.139.196.8/opec-document/images/news/news_213827873.png");
                byte[] bytes = await client.DownloadDataTaskAsync("https://regis.opec.go.th/regis/Employee.htm?mode=showPicture&id=111225");
                result.bytes = bytes;
            }

            return result;
        }
    }

    public class ImageBinary
    {
        public byte[] bytes { get; set; }
    }
}
