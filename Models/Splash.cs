using System;
namespace cms_api.Models
{
    public class Splash : Identity
    {
        public Splash()
        {
            imageUrl = "";
            timeOut = "";
        }

    
        public string imageUrl { get; set; }
        public string timeOut { get; set; }
        
    }
}
