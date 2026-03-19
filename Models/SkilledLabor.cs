using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class SkilledLabor : Identity
    {
        public SkilledLabor()
        {
            imageUrl = "";
            view = 0;
            year = 0;
            imageUrlCreateBy = "";
            dateStart = "";
            dateEnd = "";
            confirmStatus = "";
            linkFacebook = "";
            linkYoutube = "";
            status2 = false;
            firstName = "";
            lastName = "";
            numberOfDayNotification = 0;

            docDateStartEvent = DateTime.Now;
            docDateEndEvent = DateTime.Now;
        }

       
        public string imageUrl { get; set; }
        public int view { get; set; }
        public int year { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string dateStart { get; set; }
        public string dateEnd { get; set; }
        public string confirmStatus { get; set; }
        public string linkFacebook { get; set; }
        public string linkYoutube { get; set; }
        public bool status2 { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime docDateStartEvent { get; set; }
        public DateTime docDateEndEvent { get; set; }
        public int numberOfDayNotification { get; set; }

        public string duration { get; set; }
        public string type { get; set; }
        public string agency { get; set; }
    }
}
