using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class Training : Identity
    {
        public Training()
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

    public class TrainingModel
    {
        [JsonProperty("SITE")]
        public string Site { get; set; }

        [JsonProperty("DEPT_ID")]
        public string DeptId { get; set; }

        [JsonProperty("TRAINING_ID")]
        public string TrainingId { get; set; }

        [JsonProperty("TRAINING_OCCUPATION_ID")]
        public string TrainingOccupationId { get; set; }

        [JsonProperty("COURSE")]
        public string Course { get; set; }

        [JsonProperty("PERIOD")]
        public int Period { get; set; } // ใช้ int เพราะใน JSON เป็นตัวเลข 30

        [JsonProperty("CLASS_NO")]
        public string ClassNo { get; set; }

        [JsonProperty("DSD_START_DATE")]
        public DateTime? DsdStartDate { get; set; }

        [JsonProperty("DSD_END_DATE")]
        public DateTime? DsdEndDate { get; set; }

        [JsonProperty("ACTIVITY_ID")]
        public string ActivityId { get; set; }

        [JsonProperty("PROVINCE_NAME")]
        public string ProvinceName { get; set; }

        [JsonProperty("BUDGET_YEAR")]
        public string BudgetYear { get; set; }
    }

    public class PersonalTrainingModel
    {
        [JsonProperty("PERSONAL_ID")]
        public string PersonalId { get; set; }

        [JsonProperty("TRAINING_ID")]
        public string TrainingId { get; set; }

        [JsonProperty("TRAINING_OCCUPATION_ID")]
        public string TrainingOccupationId { get; set; }

        [JsonProperty("COURSE")]
        public string Course { get; set; }

        [JsonProperty("PERIOD")]
        public int Period { get; set; }

        [JsonProperty("CLASS_NO")]
        public string ClassNo { get; set; }

        [JsonProperty("DSD_START_DATE")]
        public DateTime? DsdStartDate { get; set; }

        [JsonProperty("DSD_END_DATE")]
        public DateTime? DsdEndDate { get; set; }

        [JsonProperty("PROVINCE_NAME")]
        public string ProvinceName { get; set; }

        [JsonProperty("BUDGET_YEAR")]
        public string BudgetYear { get; set; }

        [JsonProperty("STATUS_CHECK")]
        public string StatusCheck { get; set; }
    }
}
