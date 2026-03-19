using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class MenuNotification : Identity
    {

        public MenuNotification()
        {
            username = "";
        }
       
        public string username { get; set; }
        
    }
}
