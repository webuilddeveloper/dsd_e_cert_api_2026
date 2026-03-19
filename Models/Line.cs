using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    public class Line
    {
        public Line()
        {
            
        }

        public List<LineEvent> events { get; set; }
        public string destination { get; set; }

    }

    public class LineEvent
    {
        public LineEvent()
        {

        }

        public string type { get; set; }
        public string replyToken { get; set; }
        public string mode { get; set; }
        public LineSource source { get; set; }
        public LineMessage message { get; set; }
        public LinePostback postback { get; set; }
    }

    public class LineSource
    {
        public LineSource()
        {

        }

        public string userId { get; set; }
        public string type { get; set; }
        public string groupId { get; set; }
        public string roomId { get; set; }

    }

    public class LineMessage
    {
        public LineMessage()
        {

        }

        public string type { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }

    public class LinePostback
    {
        public LinePostback()
        {

        }

        public string data { get; set; }
    }

    public class Push
    {
        public List<PushMessage> messages { get; set; }
    }

    public class PushMessage
    {
        public string type { get; set; }
        public string altText { get; set; }
        public PushTemplate template { get; set; }
    }

    public class PushTemplate
    {
        public string type { get; set; }
        public string imageAspectRatio { get; set; }
        public string imageSize { get; set; }
        public List<PushColumns> columns { get; set; }
    }

    public class PushColumns
    {
        public string imageUrl { get; set; }
        public string thumbnailImageUrl { get; set; }
        public string imageBackgroundColor { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public PushDefaultAction defaultAction { get; set; }
        public List<PushAction> actions { get; set; }
        public PushAction action { get; set; }
    }

    public class PushAction
    {
        public string type { get; set; }
        public string label { get; set; }
        public string data { get; set; }
        public string text { get; set; }
    }

    public class PushDefaultAction
    {
        public string type { get; set; }
        public string label { get; set; }
        public string uri { get; set; }
    }
}
