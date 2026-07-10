using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class Testing : Identity
    {
        public Testing()
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
            isRead = false;
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

        public int duration { get; set; }
        public string type { get; set; }
        public string agency { get; set; }
        public bool isRead { get; set; }
    }

    public class TestingModel
    {
        [JsonProperty("TESTING_ID")]
        public string TestingId { get; set; }

        [JsonProperty("TEST_OCCUPATION_NAME")]
        public string TestOccupationName { get; set; }

        [JsonProperty("TEST_OCCUPATION_LEVEL")]
        public string TestOccupationLevel { get; set; }

        [JsonProperty("TEST_TIME")]
        public int TestTime { get; set; }

        [JsonProperty("START_DATE")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("END_DATE")]
        public DateTime? EndDate { get; set; }

        [JsonProperty("SITE")]
        public string Site { get; set; }

        [JsonProperty("PROVINCE_NAME")]
        public string ProvinceName { get; set; }

        [JsonProperty("BUDGET_YEAR")]
        public string BudgetYear { get; set; }
    }

    public class PersonalTestingModel
    {
        [JsonProperty("PERSONAL_ID")]
        public string PersonalId { get; set; }

        [JsonProperty("TESTING_ID")]
        public string TestingId { get; set; }

        [JsonProperty("PROVINCE_NAME")]
        public string ProvinceName { get; set; }

        [JsonProperty("TEST_OCCUPATION_NAME")]
        public string TestOccupationName { get; set; }

        [JsonProperty("TEST_OCCUPATION_LEVEL")]
        public string TestOccupationLevel { get; set; }

        [JsonProperty("TEST_TIME")]
        public int TestTime { get; set; }

        [JsonProperty("START_DATE")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("END_DATE")]
        public DateTime? EndDate { get; set; }

        [JsonProperty("BUDGET_YEAR")]
        public string BudgetYear { get; set; }

        [JsonProperty("STATUS_CHECK")]
        public string StatusCheck { get; set; }
    }

}
