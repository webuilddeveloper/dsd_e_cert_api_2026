using MongoDB.Bson.Serialization.Attributes;
using System;
namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class QR : Identity
    {
        public QR()
        {
            username = "";
            token = "";
            reference = "";
        }

        public string username { get; set; }
        public string token { get; set; }
        public string reference { get; set; }
        
    }
}
