using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class Identity
    {
        public Identity()
        {
            status = "";
            code = "";
            sequence = 0;
            titleShort = "";
            codeShort = "";

            category = "";
            language = "";

            title = "";
            description = "";

            titleEN = "";
            descriptionEN = "";

            createBy = "system";
            updateBy = "system";
            isActive = false;
            isHighlight = false;
            isPublic = false;
            docDate = DateTime.Now;

            mainPage = false;
            newsPage = false;
            eventPage = false;
            contactPage = false;
            knowledgePage = false;
            privilegePage = false;
            poiPage = false;
            pollPage = false;
            suggestionPage = false;
            notificationPage = false;
            reporterPage = false;
            trainingPage = false;
            welfarePage = false;
            warningPage = false;
            fundPage = false;
            cooperativeFormPage = false;
            //report
            reportNumberMemberRegisterPage = false;
            reportMemberRegisterPage = false;
            reportNewsCategoryPage = false;
            reportNewsPage = false;
            reportKnowledgeCategoryPage = false;
            reportKnowledgePage = false;
            reportNewsKeysearchPage = false;
            reportKnowledgeKeysearchPage = false;
            //Master
            swearWordsPage = false;

            byPass = false;

            lv0 = "";
            lv1 = "";
            lv2 = "";
            lv3 = "";
            lv4 = "";
            lv5 = "";

            organizationMode = "";

            linkUrl = "";
            textButton = "";
            action = "";
            fileUrl = "";
            platform = "";

            profileCode = "";
            userList = new List<Register1>();
        }

        public ObjectId _id { get; set; }

        public string profileCode { get; set; }
        public string status { get; set; }
        public string code { get; set; }
        public int sequence { get; set; }
        public string titleShort { get; set; }
        public string codeShort { get; set; }

        public string category { get; set; }
        public string language { get; set; }

        public string title { get; set; }
        public string description { get; set; }

        public string titleEN { get; set; }
        public string descriptionEN { get; set; }

        public string createDate { get; set; }
        public string createTime { get; set; }
        public string createBy { get; set; }
        public string updateDate { get; set; }
        public string updateTime { get; set; }
        public string updateBy { get; set; }
        public bool isActive { get; set; }
        public bool isHighlight { get; set; }
        public bool isPublic { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime docDate { get; set; }
        public string docTime { get; set; }

        public bool mainPage { get; set; }
        public bool newsPage { get; set; }
        public bool eventPage { get; set; }
        public bool contactPage { get; set; }
        public bool knowledgePage { get; set; }
        public bool privilegePage { get; set; }
        public bool poiPage { get; set; }
        public bool pollPage { get; set; }
        public bool suggestionPage { get; set; }
        public bool notificationPage { get; set; }
        public bool reporterPage { get; set; }
        public bool trainingPage { get; set; }
        public bool skilledLaborPage { get; set; }
        public bool welfarePage { get; set; }
        public bool warningPage { get; set; }
        public bool fundPage { get; set; }
        public bool cooperativeFormPage { get; set; }

        public bool reportNumberMemberRegisterPage { get; set; }
        public bool reportMemberRegisterPage { get; set; }
        public bool reportNewsCategoryPage { get; set; }
        public bool reportNewsPage { get; set; }
        public bool reportKnowledgeCategoryPage { get; set; }
        public bool reportKnowledgePage { get; set; }
        public bool reportNewsKeysearchPage { get; set; }
        public bool reportKnowledgeKeysearchPage { get; set; }
        public bool swearWordsPage { get; set; }

        public bool byPass { get; set; }
        public string lv0 { get; set; }
        public string lv1 { get; set; }
        public string lv2 { get; set; }
        public string lv3 { get; set; }
        public string lv4 { get; set; }
        public string lv5 { get; set; }

        public List<Category> categoryList { get; set; }
        public List<Register1> userList { get; set; }
        public List<Category> lv0List { get; set; }
        public List<Category> lv1List { get; set; }
        public List<Category> lv2List { get; set; }
        public List<Category> lv3List { get; set; }
        public List<Category> lv4List { get; set; }
        public List<Category> lv5List { get; set; }

        public List<Organization> organization { get; set; }
        public string organizationMode { get; set; }

        public string fileUrl { get; set; }
        public string linkUrl { get; set; }
        public string textButton { get; set; }
        public string action { get; set; }
        public string platform { get; set; }
    }
    public class BlankIdentity
    {
        public BlankIdentity()
        {
            status = "";
            code = "";
            sequence = 0;
            titleShort = "";
            codeShort = "";

            category = "";
            language = "";

            title = "";
            description = "";

            titleEN = "";
            descriptionEN = "";

            createBy = "system";
            updateBy = "system";
            isActive = false;
            isHighlight = false;
            isPublic = false;
            docDate = DateTime.Now;

            mainPage = false;
            newsPage = false;
            eventPage = false;
            contactPage = false;
            knowledgePage = false;
            privilegePage = false;
            poiPage = false;
            pollPage = false;
            suggestionPage = false;
            notificationPage = false;
            reporterPage = false;
            trainingPage = false;
            skilledLaborPage = false;
            welfarePage = false;
            warningPage = false;
            fundPage = false;
            cooperativeFormPage = false;
            //report
            reportNumberMemberRegisterPage = false;
            reportMemberRegisterPage = false;
            reportNewsCategoryPage = false;
            reportNewsPage = false;
            reportKnowledgeCategoryPage = false;
            reportKnowledgePage = false;
            reportNewsKeysearchPage = false;
            reportKnowledgeKeysearchPage = false;
            //Master
            swearWordsPage = false;

            byPass = false;

            lv0 = "";
            lv1 = "";
            lv2 = "";
            lv3 = "";
            lv4 = "";
            lv5 = "";

            organizationMode = "";

            linkUrl = "";
            textButton = "";
            action = "";
            fileUrl = "";
            platform = "";
        }

        public ObjectId _id { get; set; }

        public string status { get { return ""; } set { } }
        public string code { get { return ""; } set { } }
        public int sequence { get; set; }
        public string titleShort { get { return ""; } set { } }
        public string codeShort { get { return ""; } set { } }

        public string category { get { return ""; } set { } }
        public string language { get { return ""; } set { } }

        public string title { get { return ""; } set { } }
        public string description { get { return ""; } set { } }

        public string titleEN { get { return ""; } set { } }
        public string descriptionEN { get { return ""; } set { } }

        public string createDate { get { return ""; } set { } }
        public string createTime { get { return ""; } set { } }
        public string createBy { get { return ""; } set { } }
        public string updateDate { get { return ""; } set { } }
        public string updateTime { get { return ""; } set { } }
        public string updateBy { get { return ""; } set { } }
        public bool isActive { get; set; }
        public bool isHighlight { get; set; }
        public bool isPublic { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime docDate { get; set; }
        public string docTime { get { return ""; } set { } }

        public bool mainPage { get { return false; } set { } }
        public bool newsPage { get { return false; } set { } }
        public bool eventPage { get { return false; } set { } }
        public bool contactPage { get { return false; } set { } }
        public bool knowledgePage { get { return false; } set { } }
        public bool privilegePage { get { return false; } set { } }
        public bool poiPage { get { return false; } set { } }
        public bool pollPage { get { return false; } set { } }
        public bool suggestionPage { get { return false; } set { } }
        public bool notificationPage { get { return false; } set { } }
        public bool reporterPage { get { return false; } set { } }
        public bool trainingPage { get { return false; } set { } }
        public bool skilledLaborPage { get { return false; } set { } }
        public bool welfarePage { get { return false; } set { } }
        public bool warningPage { get { return false; } set { } }
        public bool fundPage { get { return false; } set { } }
        public bool cooperativeFormPage { get { return false; } set { } }

        public bool reportNumberMemberRegisterPage { get { return false; } set { } }
        public bool reportMemberRegisterPage { get { return false; } set { } }
        public bool reportNewsCategoryPage { get { return false; } set { } }
        public bool reportNewsPage { get { return false; } set { } }
        public bool reportKnowledgeCategoryPage { get { return false; } set { } }
        public bool reportKnowledgePage { get { return false; } set { } }
        public bool reportNewsKeysearchPage { get { return false; } set { } }
        public bool reportKnowledgeKeysearchPage { get { return false; } set { } }
        public bool swearWordsPage { get { return false; } set { } }

        public bool byPass { get { return false; } set { } }
        public string lv0 { get { return ""; } set { } }
        public string lv1 { get { return ""; } set { } }
        public string lv2 { get { return ""; } set { } }
        public string lv3 { get { return ""; } set { } }
        public string lv4 { get { return ""; } set { } }
        public string lv5 { get { return ""; } set { } }

        public List<Category> categoryList { get { return new List<Category>(); } set { } }
        public List<Register1> userList { get; set; }
        public List<Category> lv0List { get { return new List<Category>(); } set { } }
        public List<Category> lv1List { get { return new List<Category>(); } set { } }
        public List<Category> lv2List { get { return new List<Category>(); } set { } }
        public List<Category> lv3List { get { return new List<Category>(); } set { } }
        public List<Category> lv4List { get { return new List<Category>(); } set { } }
        public List<Category> lv5List { get { return new List<Category>(); } set { } }

        public List<Organization> organization { get { return new List<Organization>(); } set { } }
        public string organizationMode { get { return ""; } set { } }

        public string fileUrl { get { return ""; } set { } }
        public string linkUrl { get { return ""; } set { } }
        public string textButton { get { return ""; } set { } }
        public string action { get { return ""; } set { } }
        public string platform { get { return ""; } set { } }
    }

}
