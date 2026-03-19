using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class SystemMenu
    {
        public SystemMenu()
        {
            
        }

        public ObjectId _id { get; set; }
        public string name { get; set; }
        public string routing { get; set; }
        public string data { get; set; }
        public string type { get; set; }
        public bool isActive { get; set; }
        public bool isShow { get; set; }

    }
}
