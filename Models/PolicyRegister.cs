using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class PolicyRegister : Identity
    {

        public PolicyRegister()
        {
            username = "";
            reference = "";
        }

        public string username { get; set; }
        public string reference { get; set; }
        public List<Register> registerList { get; set; }
        public List<Policy> policyList { get; set; }
    }
}
