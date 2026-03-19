using System;
using System.Collections.Generic;
using System.Text;

namespace cms_api.Models
{
    public class Login
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class ResponseX
    {
        public string username { get; set; }
        public string token { get; set; }
        public string message { get; set; }
        public bool status { get; set; }
    }
}
