using System;
namespace cms_api.Models
{
    public class Contact : Identity
    {
        public Contact()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            phone = "";
            note = "";
        }

        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string phone { get; set; }
        public string note { get; set; }
    }
}
