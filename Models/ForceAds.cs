using System;
namespace cms_api.Models
{
    public class ForceAds : Identity
    {
        public ForceAds()
        {
            note = "";
            imageUrl = "";
            reference = "";
        }

        public string note { get; set; }
        public string imageUrl { get; set; }
        public string reference { get; set; }
    }
}
