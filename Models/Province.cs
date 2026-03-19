using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class Province : Identity
    {

        public Province()
        {
            imageUrlCreateBy = "";
        }

        public string imageUrlCreateBy { get; set; }
        
    }
}
