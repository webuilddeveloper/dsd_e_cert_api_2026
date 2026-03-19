using System;
namespace cms_api.Models
{
    public class AboutUs : Identity
    {
        public AboutUs()
        {
            imageLogoUrl = "";
            imageBgUrl = "";
            address = "";
            email = "";
            telephone = "";
            site = "";
            youtube = "";
            facebook = "";
            latitude = "";
            longitude = "";
            lineOfficial = "";
        }

        public string imageLogoUrl { get; set; }
        public string imageBgUrl { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string telephone { get; set; }
        public string site { get; set; }
        public string youtube { get; set; }
        public string facebook { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string lineOfficial { get; set; }
    }
}
