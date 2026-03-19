using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class PollQuestion : Identity
    {
        public PollQuestion()
        {
            reference = "";
        }

        public bool value { get; set; }
        public bool isRequired { get; set; }
        public string reference { get; set; }
        public List<PollAnswer> answers { get; set; }
    }
}
