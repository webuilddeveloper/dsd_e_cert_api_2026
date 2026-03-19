using MongoDB.Bson.Serialization.Attributes;
using System;
namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class PollAnswer : Identity
    {
        public PollAnswer()
        {
            reference = "";
        }

        public string reference { get; set; }
        public bool value { get; set; }

    }
}
