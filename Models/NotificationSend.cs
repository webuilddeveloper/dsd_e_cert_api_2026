using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class NotificationSend : Identity
    {

        public NotificationSend()
        {
            reference = "";
            username = "";
            imageUrlCreateBy = "";
        }

        public string reference { get; set; }
        public string username { get; set; }
        public string imageUrlCreateBy { get; set; }
        public int total { get; set; }
        public List<Notification> notificationList { get; set; }
    }
}
