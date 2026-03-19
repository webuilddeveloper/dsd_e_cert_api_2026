using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    public class Permission : Identity
    {
        public Permission()
        {
            page = "";
            reference = "";
        }

        public string page { get; set; }
        public string reference { get; set; }
        public List<Category> newsCategoryList { get; set; }
        public List<Category> eventCategoryList { get; set; }
        public List<Category> contactCategoryList { get; set; }
        public List<Category> knowledgeCategoryList { get; set; }
        public List<Category> privilegeCategoryList { get; set; }
        public List<Category> poiCategoryList { get; set; }
        public List<Category> pollCategoryList { get; set; }
        public List<Category> suggestionCategoryList { get; set; }
        public List<Category> notificationCategoryList { get; set; }
        public List<Category> welfareCategoryList { get; set; }
        public List<Category> trainingCategoryList { get; set; }
        public List<Category> reporterCategoryList { get; set; }
        public List<Category> warningCategoryList { get; set; }
        public List<Category> fundCategoryList { get; set; }
        public List<Category> cooperativeFormCategoryList { get; set; }
    }
}
