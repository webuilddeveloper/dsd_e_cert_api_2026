using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    public class Response
    {
        public Response()
        {
            status2 = false;
        }

        public string status { get; set; }
        public string message { get; set; }
        public string jsonData { get; set; }
        public object objectData { get; set; }
        public object nearByData { get; set; }
        public long totalData { get; set; }
        public bool status2 { get; set; }
    }
}
