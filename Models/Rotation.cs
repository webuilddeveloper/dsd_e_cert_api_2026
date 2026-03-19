using System;
namespace cms_api.Models
{
    public class Rotation : Identity
    {
        public Rotation()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            note = "";
            reference = "";

        }

        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string note { get; set; }
        public string reference { get; set; }

    }
}
