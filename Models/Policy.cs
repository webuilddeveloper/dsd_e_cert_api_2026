using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class Policy : Identity
    {

        public Policy()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            view = 0;
            isRequired = true;
        }

        public string imageUrl { get; set; }
        public bool isRequired { get; set; }
        public string imageUrlCreateBy { get; set; }
        public int view { get; set; }
    }
}
