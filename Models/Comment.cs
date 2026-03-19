using System;
namespace cms_api.Models
{
    public class Comment : Identity
    {
        public Comment()
        {
            imageUrlCreateBy = "";
            reference = "";
            idcard = "";
            firstName = "";
            lastName = "";
            original = "";
            type = "";
        }

        public string imageUrlCreateBy { get; set; }
        public string reference { get; set; }
        public string idcard { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string original { get; set; }
        public string type { get; set; }
    }
}
