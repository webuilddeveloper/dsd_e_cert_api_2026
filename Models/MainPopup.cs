using System;
namespace cms_api.Models
{
    public class MainPopup : Identity
    {
        public MainPopup()
        {
            imageUrl = "";
        }

        public string imageUrl { get; set; }
    }
}
