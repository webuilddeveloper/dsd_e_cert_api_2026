using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class Menu : Identity
    {

        public Menu()
        {
            imageUrl = "";
            direction = "";
        }

        
        public string imageUrl { get; set; }
        public string direction { get; set; }
        
    }
}
