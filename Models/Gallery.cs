using System;
namespace cms_api.Models
{
    public class Gallery : Identity
    {
        public Gallery()
        {
            imageUrl = "";
            reference = "";
        }

        public string imageUrl { get; set; }
        public string reference { get; set; }
    }
}
