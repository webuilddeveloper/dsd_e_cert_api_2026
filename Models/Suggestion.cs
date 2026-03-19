using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    public class Suggestion : Identity
    {
        public Suggestion()
        {
            imageUrl = "";
            latitude = "";
            longitude = "";
            imageUrlCreateBy = "";
            status2 = "";
            view = 0;
            firstName = "";
            lastName = "";
            gallery = new List<Gallery>();
        }

        public string imageUrl { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string status2 { get; set; }
        public string province { get; set; }
        public int view { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }

        public List<Gallery> gallery { get; set; }
    }
}
