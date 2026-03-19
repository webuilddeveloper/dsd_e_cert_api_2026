using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace cms_api.Models
{
    public class DLTLC
    {
        public DLTLC()
        {
            createBy = "";
            updateBy = "";
        }
        public string createBy { get; set; }
        public string updateBy { get; set; }
        public List<driverLicenceInfo> driverLicenceInfo { get; set; }
    }

    public class driverLicenceInfo
    {
        public driverLicenceInfo()
        {

            addrNo = "";
            bldName = "";
            canFlag = "";
            completeFlag = "";
            conditionDesc = "";
            docNo = "";
            docType = "";
            excFee = "";
            fname = "";
            fnameEng = "";
            lname = "";
            lnameEng = "";
            locCode = "";
            locDesc = "";
            locFullDesc = "";
            message = "";
            natCode = "";
            natDesc = "";
            needDrive = "";
            needEff = "";
            needPaper = "";
            needTrain = "";
            offLocCode = "";
            passDriveExam = "";
            passEffExam = "";
            passPaperExam = "";
            passTrainExam = "";
            pcNo = "";
            pltCode = "";
            pltDesc = "";
            pltNo = "";
            rcpNo = "";
            reqNo = "";
            reqTrCode = "";
            rvkFlag = "";
            sex = "";
            soi = "";
            srlNo = "";
            street = "";
            titleDesc = "";
            titleEngDesc = "";
            villageName = "";
            villageNo = "";
            zipCode = "";
            status = "";
            reference = "";
        }


        public string addrNo { get; set; }
        public DateTime? birthDate { get; set; }
        public string bldName { get; set; }
        public string canFlag { get; set; }
        public string completeFlag { get; set; }
        public string conditionDesc { get; set; }
        public string docNo { get; set; }
        public string docType { get; set; }
        public string excFee { get; set; }
        public DateTime? expDate { get; set; }
        public string fname { get; set; }
        public string fnameEng { get; set; }
        public DateTime? issDate { get; set; }
        public string lname { get; set; }
        public string lnameEng { get; set; }
        public string locCode { get; set; }
        public string locDesc { get; set; }
        public string locFullDesc { get; set; }
        public string message { get; set; }
        public string natCode { get; set; }
        public string natDesc { get; set; }
        public string needDrive { get; set; }
        public string needEff { get; set; }
        public string needPaper { get; set; }
        public string needTrain { get; set; }
        public string offLocCode { get; set; }
        public string passDriveExam { get; set; }
        public string passEffExam { get; set; }
        public string passPaperExam { get; set; }
        public string passTrainExam { get; set; }
        public string pcNo { get; set; }
        public string pltCode { get; set; }
        public string pltDesc { get; set; }
        public string pltNo { get; set; }
        public string rcpNo { get; set; }
        public DateTime? reqDate { get; set; }
        public string reqNo { get; set; }
        public string reqTrCode { get; set; }
        public bool? result { get; set; }
        public string rvkFlag { get; set; }
        public string sex { get; set; }
        public string soi { get; set; }
        public string srlNo { get; set; }
        public string street { get; set; }
        public string titleDesc { get; set; }
        public string titleEngDesc { get; set; }
        public string villageName { get; set; }
        public string villageNo { get; set; }
        public string zipCode { get; set; }
        public string status { get; set; }
        public string reference { get; set; }
    }

    public class DriverLicence
    {
        public DriverLicence()
        {
            code = "";
            docNo = "";
            docType = "";
            pltCode = "";
            pltNo = "";
            reqDocNo = "";
            createBy = "";
            updateBy = "";
            reference = "";
        }
        public string code { get; set; }
        public string docNo { get; set; }
        public string docType { get; set; }
        public string pltCode { get; set; }
        public string pltNo { get; set; }
        public string reqDocNo { get; set; }
        public string createBy { get; set; }
        public string updateBy { get; set; }
        public string issDate { get; set; }
        public string reference { get; set; }

    }

    public class driverLicenceInfobase : Identity
    {
        public driverLicenceInfobase()
        {
            code = "";
            addrNo = "";
            birthDate = "";
            bldName = "";
            canFlag = "";
            completeFlag = "";
            conditionDesc = "";
            docNo = "";
            docType = "";
            excFee = "";
            fname = "";
            fnameEng = "";
            lname = "";
            lnameEng = "";
            locCode = "";
            locDesc = "";
            locFullDesc = "";
            message = "";
            natCode = "";
            natDesc = "";
            needDrive = "";
            needEff = "";
            needPaper = "";
            needTrain = "";
            offLocCode = "";
            passDriveExam = "";
            passEffExam = "";
            passPaperExam = "";
            passTrainExam = "";
            pcNo = "";
            pltCode = "";
            pltDesc = "";
            pltNo = "";
            rcpNo = "";
            reqNo = "";
            reqTrCode = "";
            rvkFlag = "";
            sex = "";
            soi = "";
            srlNo = "";
            street = "";
            titleDesc = "";
            titleEngDesc = "";
            villageName = "";
            villageNo = "";
            zipCode = "";
            status = "";
            reference = "";
            expDate = "";
            issDate = "";
            reqDate = "";
        }
        
        public string addrNo { get; set; }
        public string birthDate { get; set; }
        public string bldName { get; set; }
        public string canFlag { get; set; }
        public string completeFlag { get; set; }
        public string conditionDesc { get; set; }
        public string docNo { get; set; }
        public string docType { get; set; }
        public string excFee { get; set; }
        public string expDate { get; set; }
        public string fname { get; set; }
        public string fnameEng { get; set; }
        public string issDate { get; set; }
        public string lname { get; set; }
        public string lnameEng { get; set; }
        public string locCode { get; set; }
        public string locDesc { get; set; }
        public string locFullDesc { get; set; }
        public string message { get; set; }
        public string natCode { get; set; }
        public string natDesc { get; set; }
        public string needDrive { get; set; }
        public string needEff { get; set; }
        public string needPaper { get; set; }
        public string needTrain { get; set; }
        public string offLocCode { get; set; }
        public string passDriveExam { get; set; }
        public string passEffExam { get; set; }
        public string passPaperExam { get; set; }
        public string passTrainExam { get; set; }
        public string pcNo { get; set; }
        public string pltCode { get; set; }
        public string pltDesc { get; set; }
        public string pltNo { get; set; }
        public string rcpNo { get; set; }
        public string reqDate { get; set; }
        public string reqNo { get; set; }
        public string reqTrCode { get; set; }
        public bool? result { get; set; }
        public string rvkFlag { get; set; }
        public string sex { get; set; }
        public string soi { get; set; }
        public string srlNo { get; set; }
        public string street { get; set; }
        public string titleDesc { get; set; }
        public string titleEngDesc { get; set; }
        public string villageName { get; set; }
        public string villageNo { get; set; }
        public string zipCode { get; set; }
        public string reference { get; set; }
    }

}