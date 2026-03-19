using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class Register : Identity
    {

        public Register()
        {
            token = "";
            imageUrl = "";
            username = "";
            password = "";
            reference = "";

            prefixName = "";
            firstName = "";
            lastName = "";
            birthDay = "";
            phone = "";
            email = "";
            facebookID = "";
            googleID = "";
            lineID = "";
            appleID = "";
            line = "";

            sex = "";
            soi = "";
            address = "";
            moo = "";
            road = "";
            tambon = "";
            amphoe = "";
            province = "";
            postno = "";
            job = "";
            idcard = "";
            officerCode = "";
            countUnit = "";

            tambonCode = "";
            amphoeCode = "";
            provinceCode = "";
            postnoCode = "";

            linkAccount = "";
            laserID = "";
            lasercode = "";
            lasernum = "";
        }

        public string token { get; set; }
        public bool isOnline { get; set; }
        public string imageUrl { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string reference { get; set; }

        public string prefixName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string birthDay { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string facebookID { get; set; }
        public string googleID { get; set; }
        public string lineID { get; set; }
        public string appleID { get; set; }
        public string line { get; set; }

        public string newPassword { get; set; }

        public string sex { get; set; }
        public string soi { get; set; }
        public string address { get; set; }
        public string moo { get; set; }
        public string road { get; set; }
        public string tambon { get; set; }
        public string amphoe { get; set; }
        public string province { get; set; }
        public string postno { get; set; }
        public string job { get; set; }
        public string idcard { get; set; }
        public string officerCode { get; set; }
        public string countUnit { get; set; }
        public string tambonCode { get; set; }
        public string amphoeCode { get; set; }
        public string provinceCode { get; set; }
        public string postnoCode { get; set; }

        public string linkAccount { get; set; }
        public string laserID { get; set; }
        public string lasercode { get; set; }
        public string lasernum { get; set; }
    }
    public class Register1 : BlankIdentity
    {

        public Register1()
        {
            token = "";
            imageUrl = "";
            username = "";
            password = "";
            reference = "";

            prefixName = "";
            firstName = "";
            lastName = "";
            birthDay = "";
            phone = "";
            email = "";
            facebookID = "";
            googleID = "";
            lineID = "";
            appleID = "";
            line = "";

            sex = "";
            soi = "";
            address = "";
            moo = "";
            road = "";
            tambon = "";
            amphoe = "";
            province = "";
            postno = "";
            job = "";
            idcard = "";
            officerCode = "";
            countUnit = "";

            tambonCode = "";
            amphoeCode = "";
            provinceCode = "";
            postnoCode = "";

            linkAccount = "";

        }

        public string token { get { return ""; } set { } }
        public bool isOnline { get { return false; } set { } }
        public string imageUrl { get; set; }
        public string username { get; set; }
        public string password { get { return ""; } set { } }
        public string reference { get { return ""; } set { } }

        public string prefixName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string birthDay { get { return ""; } set { } }
        public string phone { get { return ""; } set { } }
        public string email { get { return ""; } set { } }
        public string facebookID { get { return ""; } set { } }
        public string googleID { get { return ""; } set { } }
        public string lineID { get { return ""; } set { } }
        public string appleID { get { return ""; } set { } }
        public string line { get { return ""; } set { } }

        public string newPassword { get { return ""; } set { } }

        public string sex { get { return ""; } set { } }
        public string soi { get { return ""; } set { } }
        public string address { get { return ""; } set { } }
        public string moo { get { return ""; } set { } }
        public string road { get { return ""; } set { } }
        public string tambon { get { return ""; } set { } }
        public string amphoe { get { return ""; } set { } }
        public string province { get { return ""; } set { } }
        public string postno { get { return ""; } set { } }
        public string job { get { return ""; } set { } }
        public string idcard { get { return ""; } set { } }
        public string officerCode { get { return ""; } set { } }
        public string countUnit { get { return ""; } set { } }
        public string tambonCode { get { return ""; } set { } }
        public string amphoeCode { get { return ""; } set { } }
        public string provinceCode { get { return ""; } set { } }
        public string postnoCode { get { return ""; } set { } }

        public string linkAccount { get { return ""; } set { } }
    }
}
