using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    public class Criteria
    {
        public Criteria()
        {
            keySearch = "";
            sequence = "";
            skip = 0;
            limit = 99999;
            startDate = "";
            endDate = "";
            code = "";
            codeShort = "";
            title = "";
            titleEN = "";
            category = "";
            description = "";
            createBy = "";
            updateBy = "";
            isActive = false;
            isHighlight = false;
            isPublic = false;
            permission = "";
            sex = "";
            age = "";
            language = "";
            reference = "";
            action = "";
            imageUrlCreateBy = "";
            status2 = "";
            email = "";

            lv0 = "";
            lv1 = "";
            lv2 = "";
            lv3 = "";
            lv4 = "";
            lv5 = "";
            organization = new List<Organization>();

            province = "";
            district = "";
            tambon = "";
            postno = "";
            phone = "";
            firstName = "";
            lastName = "";

            databaseName = "";
            page = "";
            platform = "";
            countUnit = "";

            startDateEvent = "";
            endDateEvent = "";

            latitude = 0.0;
            longitude = 0.0;

            profileCode = "";
            app = "";
        }

        public string profileCode { get; set; }
        public string keySearch { get; set; }
        public string sequence { get; set; }
        public int skip { get; set; }
        public int limit { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string code { get; set; }
        public string codeShort { get; set; }
        public string status { get; set; }
        public string title { get; set; }
        public string titleEN { get; set; }
        public string category { get; set; }
        public string description { get; set; }
        public string dateStart { get; set; }
        public string dateEnd { get; set; }
        public string createBy { get; set; }
        public string updateBy { get; set; }
        public bool isActive { get; set; }
        public bool isHighlight { get; set; }
        public bool isPublic { get; set; }
        public string reference { get; set; }
        public string action { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string status2 { get; set; }
        public string email { get; set; }

        public string username { get; set; }
        public string language { get; set; }
        public string sex { get; set; }
        public string age { get; set; }
        public string password { get; set; }
        public string date { get; set; }
        public string phone { get; set; }

        public string permission { get; set; }
        public string lv0 { get; set; }
        public string lv1 { get; set; }
        public string lv2 { get; set; }
        public string lv3 { get; set; }
        public string lv4 { get; set; }
        public string lv5 { get; set; }
        public List<Organization> organization { get; set; }

        public string province { get; set; }
        public string district { get; set; }
        public string tambon { get; set; }
        public string postno { get; set; }
        public string lineID { get; set; }
        public string line { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }

        public string databaseName { get; set; }
        public string page { get; set; }
        public string platform { get; set; }
        public string countUnit { get; set; }

        public string startDateEvent { get; set; }
        public string endDateEvent { get; set; }

        public double latitude { get; set; }
        public double longitude { get; set; }

        public string app { get; set; }
    }
}
