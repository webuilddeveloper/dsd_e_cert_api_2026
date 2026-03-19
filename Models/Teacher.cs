using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    public class TeacherList
    {
        public TeacherList()
        {
            EmployeeOpec = new List<Employee>();
        }
        public List<Employee> EmployeeOpec { get; set; }
    }

    public class Teacher
    {
        public Teacher()
        {
            EmployeeOpec = new Employee();
        }
        public Employee EmployeeOpec { get; set; }
    }

    public class Employee
    {
        public Employee()
        {
            certificateCode = "";
            certificateStart = "";
            certificateStop = "";
            certificateType = "";
            employeePositionID = "";
            employeePositionName = "";
            firstName = "";
            idCard = "";
            lastName = "";
            nationCode = "";
            nationName = "";
            prefix = "";
            raceCode = "";
            teachDegreeLevelID = "";
            teachSubjectID = "";
            fullName = "";
            teachSubjectName = "";
            teachDegreeLevelName = "";
            certificateTypeName = "";
            schoolName = "";
            email = "";
            phone = "";
            imageUrl = "";
        }

        public string certificateCode { get; set; }
        public string certificateStart { get; set; }
        public string certificateStop { get; set; }
        public string certificateType { get; set; }
        public string certificateStatusName { get; set; }
        public string certificateColor { get; set; }
        public string certificateTypeName { get; set; }
        public string employeePositionID { get; set; }
        public string employeePositionName { get; set; }
        public string fullName { get; set; }
        public string firstName { get; set; }
        public string idCard { get; set; }
        public string lastName { get; set; }
        public string nationCode { get; set; }
        public string nationName { get; set; }
        public string prefix { get; set; }
        public string raceCode { get; set; }
        public string teachDegreeLevelID { get; set; }
        public string teachDegreeLevelName { get; set; }
        public string teachSubjectID { get; set; }
        public string teachSubjectName { get; set; }
        public string schoolName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string imageUrl { get; set; }
    }

    public class CertificateTeacherModel
    {
        public CertificateTeacherModel()
        {
            code = "";
            certificateTypeName = "";
            statusName = "";
            color = "";
        }
        public string code { get; set; }
        public string certificateTypeName { get; set; }
        public string color { get; set; }
        public string statusName { get; set; }
    }
}
