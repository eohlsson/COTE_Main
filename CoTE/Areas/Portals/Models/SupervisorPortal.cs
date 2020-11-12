using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CoTE.Areas.Portals.Models
{
    public class SupervisorPortalContext : DbContext
    {
        public SupervisorPortalContext()
            : base("UserDB")
        {
        }
    }

    public class Authorized 
    {
        public string dataset { get; set; }
        public string user_name { get; set; }
        public bool beta { get; set; }
    }

    public class SupervisorProfile
    {
        public string dataset { get; set; }	
        public int curr_time_rpts { get; set; }
        public int num_incomplete_time_rpts { get; set; }
        public int curr_time_rpts_efe { get; set; }
        public int num_incomplete_time_rpts_efe { get; set; }
        public int role_id { get; set; }
        public string net_id { get; set; }
        public string last_name	{ get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string role { get; set; }
        public string role_code { get; set; }
        public string uiuc_programname { get; set; }
        public int programcode { get; set; }
        public string programgroup { get; set; }
        public string job_title { get; set; }
        public string email { get; set; }

     }

    public class SupervisorLinks
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

    public class SupervisorData
    {
        public List<Authorized> AuthorizedDataSet { get; set; }
        public List<SupervisorProfile> ProfileDataSet { get; set; }
        public List<SupervisorLinks> LinksDataSet { get; set; }

        public SupervisorData(List<Authorized> d1, List<SupervisorProfile> d2, List<SupervisorLinks> d3)
        {
            this.AuthorizedDataSet = d1;
            this.ProfileDataSet = d2;
            this.LinksDataSet = d3;
        }
    }

    public class SupervisorChecklistHeader
    {
        public string dataset { get; set; }
        public string header { get; set; }
    }

    public class SupervisorChecklistProfile
    {
        public string dataset { get; set; }
        public string full_name { get; set; }
        public string email_address { get; set; }
        public string net_id { get; set; }
    }

    public class SupervisorChecklistSection
    {
        public string dataset { get; set; }
        public string level { get; set; }
        public string section_name { get; set; }
        public string met_unmet { get; set; }
        public string met_text { get; set; }
        public string unmet_text { get; set; }
        public string required { get; set; }
    }

    public class SupervisorChecklistData
    {
        public List<Authorized> AuthorizedDataSet { get; set; }
        public List<SupervisorChecklistHeader> ChecklistHeaderDataSet { get; set; }
        public List<SupervisorChecklistProfile> ChecklistProfileDataSet { get; set; }
        public List<SupervisorChecklistSection> ChecklistSectionDataSet { get; set; }

        public SupervisorChecklistData(List<Authorized> d1, List<SupervisorChecklistHeader> d2, List<SupervisorChecklistProfile> d3, List<SupervisorChecklistSection> d4)
        {
            this.AuthorizedDataSet = d1;
            this.ChecklistHeaderDataSet = d2;
            this.ChecklistProfileDataSet = d3;
            this.ChecklistSectionDataSet = d4;
        }
    }

}