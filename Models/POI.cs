using System;
namespace cms_api.Models
{
    public class POI : Identity
    {
        public POI()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            latitude = "";
            longitude = "";
            address = "";
            view = 0;
            action = "";
        }

        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string address { get; set; }
        public int view { get; set; }
        public double distance { get; set; }

    }
}
