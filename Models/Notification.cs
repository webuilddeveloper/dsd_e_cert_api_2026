using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class FirebaseModel
    {
        public FirebaseModel()
        {
            priority = "high";
        }

        public List<string> registration_ids { get; set; }
        public string priority { get; set; }
        public NotificationModel notification { get; set; }
        public DataModel data { get; set; }
    }

    public class NotificationModel
    {
        public NotificationModel()
        {
            title = "";
            body = "";
            image = "";
        }

        public string title { get; set; }
        public string body { get; set; }
        public string image { get; set; }
    }

    public class DataModel
    {
        public DataModel()
        {
            title = "";
            body = "";
            image = "";
            click_action = "FLUTTER_NOTIFICATION_CLICK";
            status = "done";
            page = "";
            code = "";
        }

        public string title { get; set; }
        public string body { get; set; }
        public string image { get; set; }
        public string click_action { get; set; }
        public string status { get; set; }

        public string page { get; set; }
        public string code { get; set; }
    }

    public class Notification : Identity
    {

        public Notification()
        {
            reference = "";
            token = "";
            page = "";
            username = "";
            to = "";
            sound = "";
            body = "";
            priority = "";
            channelId = "";
            _displayInForeground = false;
            imageUrlCreateBy = "";
            items = new List<NotificationItem>();
        }

        public string reference { get; set; }
        public string token { get; set; }
        public string to { get; set; }
        public string sound { get; set; }
        public string body { get; set; }
        public string priority { get; set; }
        public string channelId { get; set; }
        public bool _displayInForeground { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string page { get; set; }
        public string username { get; set; }
        public int total { get; set; }
        public DataPush data { get; set; }
        public List<NotificationItem> items { get; set; }
    }

    public class DataPush
    {
        public DataPush()
        {
            code = "";
            page = "";
        }
        public string code { get; set; }
        public string page { get; set; }
    }

    public class NotificationItem
    {
        public bool isSelected { get; set; }
        public string username { get; set; }
        public string category { get; set; }
    }
}
