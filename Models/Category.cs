using System;
namespace cms_api.Models
{
    public class Category : Identity
    {
        public Category()
        {
            imageUrl = "";
        }

        public string imageUrl { get; set; }
    }
}
