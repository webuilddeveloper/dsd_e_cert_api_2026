using System;
namespace cms_api.Models
{
    public class Privilege : Identity
    {
        public Privilege()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            dateStart = "";
            dateEnd = "";
            receive = false;
            view = 0;
        }

        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string dateStart { get; set; }
        public string dateEnd { get; set; }
        public bool receive { get; set; }
        public int view { get; set; }
    }
}
