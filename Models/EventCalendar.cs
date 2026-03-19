using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class EventCalendar : Identity
    {
        public EventCalendar()
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
    }

    public class EventCalendar2
    {
        public string monthYear { get; set; }
        public string dateStart { get; set; }
        public string dateEnd { get; set; }
        public List<EventCalendar2Item> items { get; set; }

    }

    public class EventCalendar2Item
    {
        public string code { get; set; }
        public string dateStart { get; set; }
        public string dateEnd { get; set; }
        public string title { get; set; }
    }
    public class EventCalendarMark2
    {
        public EventCalendarMark2()
        {
            date = "";
            items = new List<EventDataMark2>();
        }
        public string date { get; set; }
        public List<EventDataMark2> items { get; set; }
    }

    public class EventDataMark2
    {
        public EventDataMark2()
        {
            code = "";
            dateStart = "";
            dateEnd = "";
            title = "";
            imageUrl = "";
        }

        public string code { get; set; }
        public string dateStart { get; set; }
        public string dateEnd { get; set; }
        public string title { get; set; }
        public string imageUrl { get; set; }
    }
}
