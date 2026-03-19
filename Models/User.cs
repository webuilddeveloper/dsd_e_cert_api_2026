using System;
namespace cms_api.Models
{
    public class User : Identity
    {
        public User()
        {
            username = "";
            password = "";
            prefixName = "";
            firstName = "";
            lastName = "";
            birthDay = "";
            phone = "";
            email = "";
            imageUrl = "";
            facebookID = "";
            googleID = "";
            lineID = "";
            position = "";
            level = "";
            expirationDate = "";
        }

        public string username { get; set; }
        public string password { get; set; }
        public string prefixName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string birthDay { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string imageUrl { get; set; }
        public string facebookID { get; set; }
        public string googleID { get; set; }
        public string lineID { get; set; }
        public string position { get; set; }
        public string level { get; set; }
        public string expirationDate { get; set; }
    }
}
