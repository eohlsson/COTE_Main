using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CoTE.Areas.Portals.Models
{
    public class StudentPortalContext : DbContext
    {
        public StudentPortalContext()
            : base("UserDB")
        {
        }
    }

    public class StudentAuthorized 
    {
        public string dataset { get; set; }
        public string user_name { get; set; }
        public bool beta { get; set; }
    }

    public class StudentDeficiency
    {
        public string dataset { get; set; }
        public string deficiency { get; set; }
    }

    public class StudentChecklistHeader
    {
        public string dataset { get; set; }
        public string header { get; set; }
    }

    public class StudentChecklistProfile
    {
        public string dataset { get; set; }
        public string full_name { get; set; }
        public string email_address { get; set; }
        public string UIUC_ProgramName { get; set; }
        public string program_status { get; set; }
    }

    public class StudentChecklistSection
    {
        public string dataset { get; set; }
        public string level { get; set; }
        public string section_name { get; set; }
        public string met_unmet { get; set; }
        public string met_text { get; set; }
        public string unmet_text { get; set; }
        public string required { get; set; }
    }

    public class StudentChecklistData
    {
        public List<StudentAuthorized> AuthorizedDataSet { get; set; }
        public List<StudentChecklistHeader> ChecklistHeaderDataSet { get; set; }
        public List<StudentChecklistProfile> ChecklistProfileDataSet { get; set; }
        public List<StudentChecklistSection> ChecklistSectionDataSet { get; set; }

        public StudentChecklistData(List<StudentAuthorized> d1, List<StudentChecklistHeader> d2, List<StudentChecklistProfile> d3, List<StudentChecklistSection> d4)
        {
            this.AuthorizedDataSet = d1;
            this.ChecklistHeaderDataSet = d2;
            this.ChecklistProfileDataSet = d3;
            this.ChecklistSectionDataSet = d4;
        }
    }

    public class StudentProfile
    {
        public string dataset { get; set; }	
        public int role_id { get; set; }
        public string network_id { get; set; }
        public string last_name	{ get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string role { get; set; }
        public string role_code { get; set; }
        public string UIUC_ProgramName { get; set; }
        public string input_name { get; set; }

     }

    public class StudentCAP
    {
        public string dataset { get; set; }
        public string Program { get; set; }
        public string Basic_Skills_Date { get; set; }
        public string CBC_Date_Cleared { get; set; }
        public string BBP_Date_Completed { get; set; }
        public string Safety_Date_Completed { get; set; }
        public decimal? EFE_hours	{ get; set; }
        public string EFE_recommendation { get; set; }
        public string Content_Test_Date	{ get; set; }
        public string Other_Test { get; set; }
        public string ST_AGREEMENT { get; set; }
        public string MANDATED_REPORTER { get; set; }
        public string APT_Test_Date { get; set; }
        public int? supervisor_total	{ get; set; }
        public int? supervisor_done { get; set; } 
        public int? coop_total { get; set; }
        public int? coop_done { get; set; }
        public int? candidate_total { get; set; }
        public int? candidate_done { get; set; }
    }

    public class StudentLinks
    {
        public string dataset  { get; set; }	
        public int entry_id  { get; set; }
        public string section_type	 { get; set; }
        public string short_title { get; set; }
        public string heading	 { get; set; }
        public string link	 { get; set; }
        public int link_order { get; set; }
        public string UIUC_Program_ID { get; set; }
        public string UIUC_ProgramGroup	 { get; set; }
        public bool not_in_flag { get; set; }
        public string short_heading { get; set; }
    }

    public class StudentData
    {
        public List<StudentAuthorized> AuthorizedDataSet { get; set; }
        public List<StudentDeficiency> DeficiencyDataSet { get; set; }
        public List<StudentProfile> ProfileDataSet { get; set; }
        public List<StudentCAP> CAPDataSet { get; set; }
        public List<StudentLinks> LinksDataSet { get; set; }

        public StudentData(List<StudentAuthorized> d1, List<StudentDeficiency> d2, List<StudentProfile> d3, List<StudentCAP> d4,  List<StudentLinks> d5)
        {
            this.AuthorizedDataSet = d1;
            this.DeficiencyDataSet = d2;
            this.ProfileDataSet = d3;
            this.CAPDataSet = d4;
            this.LinksDataSet = d5;
        }
    }

}