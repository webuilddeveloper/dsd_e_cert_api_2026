using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class Warning : Identity
    {

        public Warning()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            view = 0;
        }

        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public int view { get; set; }
    }
}
