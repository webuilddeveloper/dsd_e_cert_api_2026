using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class Poll : Identity
    {
        public Poll()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            view = 0;
            status2 = false;
        }

        public int view { get; set; }
        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public bool status2 { get; set; }
        public List<Questions> questions { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Questions
    {
        public Questions()
        {
            value = false;
            isRequired = true;
        }
        public string code { get; set; }
        public string reference { get; set; }
        public string title { get; set; }
        public bool value { get; set; }
        public string category { get; set; }
        public bool isActive { get; set; }
        public bool isRequired { get; set; }
        public List<Answer> answers { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Answer
    {
        public Answer()
        {
            value = false;
        }
        public string code { get; set; }
        public string title { get; set; }
        public bool value { get; set; }
        public bool isActive { get; set; }
    }
}