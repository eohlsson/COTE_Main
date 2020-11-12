using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CoTE.Areas.Portals.Models
{
    public class CoopPortalContext : DbContext
    {
        public CoopPortalContext()
            : base("UserDB")
        {
        }
    }

    public class CoopAuthorized 
    {
        public string dataset { get; set; }
        public string user_name { get; set; }
        public bool beta { get; set; }
    }

    public class CoopProfile
    {
        public string dataset { get; set; }	
        public int curr_time_rpts { get; set; }
        public int num_incomplete_time_rpts { get; set; }
        public int role_id { get; set; }
        public string net_id { get; set; }
        public string last_name	{ get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string full_role_type { get; set; }
        public string uiuc_programname { get; set; }
        public int programcode { get; set; }
        public string programgroup { get; set; }
        public string job_title { get; set; }
        public string email { get; set; }
        public string uin { get; set; }

     }

    public class CoopLinks
    {
        public string dataset { get; set; }
        public string short_heading { get; set; }
        public int entry_id { get; set; }
        public string section_type	 { get; set; }
        public string short_title { get; set; }
        public string heading	 { get; set; }
        public string link	 { get; set; }
        public int? link_order { get; set; }
        public string UIUC_Program_ID { get; set; }
        public string UIUC_ProgramGroup	 { get; set; }
        public bool not_in_flag { get; set; }
        public string waiver_term { get; set; }
    }

    public class CoopAgreement
    {
        public string dataset { get; set; }
        public string signed { get; set; }
    }

    public class CoopData
    {
        public List<CoopAuthorized> AuthorizedDataSet { get; set; }
        public List<CoopProfile> ProfileDataSet { get; set; }
        public List<CoopLinks> LinksDataSet { get; set; }
        public List<CoopAgreement> AgreementDataSet { get; set; }

        public CoopData(List<CoopAuthorized> d1, List<CoopProfile> d2, List<CoopLinks> d3, List<CoopAgreement> d4)
        {
            this.AuthorizedDataSet = d1;
            this.ProfileDataSet = d2;
            this.LinksDataSet = d3;
            this.AgreementDataSet = d4; 
        }
    }

    public class EOC_PDU
    {
        public string full_name { get; set; }
        public string location { get; set; }
        public string dates { get; set; }
        public string pdus { get; set; }
        public string eoc_date { get; set; }
    }

    public class EOC_COOP
    {
        public string dataset { get; set; }
        public string coop { get; set; }
        public string facility { get; set; }
        public string district { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
    }

    public class EOC_STUDENT
    {
        public string dataset { get; set; }
        public string pretty_term { get; set; }
        public string placement_type { get; set; }
        public string UIUC_ProgramName { get; set; }
        public string course { get; set; }
        public string student { get; set; }
    }

    public class EOC_Data
    {
        public List<CoopAuthorized> AuthorizedDataSet { get; set; }
        public List<EOC_COOP> EOCCoopDataSet { get; set; }
        public List<EOC_STUDENT> EOCStudentDataSet { get; set; }

        public EOC_Data(List<CoopAuthorized> d1, List<EOC_COOP> d2, List<EOC_STUDENT> d3)
        {
            this.AuthorizedDataSet = d1;
            this.EOCCoopDataSet = d2;
            this.EOCStudentDataSet = d3;
        }
    }

    public class EOC_PDU_Data
    {
        public List<CoopAuthorized> AuthorizedDataSet { get; set; }
        public List<EOC_PDU> EOCCoopDataSet { get; set; }

        public EOC_PDU_Data(List<CoopAuthorized> d1, List<EOC_PDU> d2)
        {
            this.AuthorizedDataSet = d1;
            this.EOCCoopDataSet = d2;
        }
    }
    
    public class CoopChecklistHeader
    {
        public string dataset { get; set; }
        public string header { get; set; }
    }

    public class CoopChecklistProfile
    {
        public string dataset { get; set; }
        public string full_name { get; set; }
        public string email { get; set; }
        public string uin { get; set; }
    }

    public class CoopChecklistSection
    {
        public string dataset { get; set; }
        public string level { get; set; }
        public string section_name { get; set; }
        public string met_unmet { get; set; }
        public string met_text { get; set; }
        public string unmet_text { get; set; }
        public string required { get; set; }
    }

    public class CoopChecklistData
    {
        public List<CoopAuthorized> AuthorizedDataSet { get; set; }
        public List<CoopChecklistHeader> ChecklistHeaderDataSet { get; set; }
        public List<CoopChecklistProfile> ChecklistProfileDataSet { get; set; }
        public List<CoopChecklistSection> ChecklistSectionDataSet { get; set; }

        public CoopChecklistData(List<CoopAuthorized> d1, List<CoopChecklistHeader> d2, List<CoopChecklistProfile> d3, List<CoopChecklistSection> d4)
        {
            this.AuthorizedDataSet = d1;
            this.ChecklistHeaderDataSet = d2;
            this.ChecklistProfileDataSet = d3;
            this.ChecklistSectionDataSet = d4;
        }
    }

}