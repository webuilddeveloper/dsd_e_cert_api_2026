using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class RegisterNotification : Identity
    {

        public RegisterNotification()
        {
            username = "";
           
        }

        public string username { get; set; }
       

    }
}
