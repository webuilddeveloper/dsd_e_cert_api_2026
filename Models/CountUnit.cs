using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class CountUnit
    {

        public CountUnit()
        {

            lv0 = "";
            titleLv0 = "";
            lv1 = "";
            titleLv1 = "";
            lv2 = "";
            titleLv2 = "";
            lv3 = "";
            titleLv3 = "";
            lv4 = "";
            titleLv4 = "";
            status = "";

        }


        public string lv0 { get; set; }
        public string titleLv0 { get; set; }
        public string lv1 { get; set; }
        public string titleLv1 { get; set; }
        public string lv2 { get; set; }
        public string titleLv2 { get; set; }
        public string lv3 { get; set; }
        public string titleLv3 { get; set; }
        public string lv4 { get; set; }
        public string titleLv4 { get; set; }
        public string status { get; set; }

    }
}
