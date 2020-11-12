using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CoTE.Areas.Portals.Models
{
    public class ProgramPortalContext : DbContext
    {
        public ProgramPortalContext()
            : base("UserDB")
        {
        }
    }

    public class ProgramAuthorized 
    {
        public string dataset { get; set; }
        public string user_name { get; set; }
    }

    public class ProgramDeficiency
    {
        public string dataset { get; set; }
        public string deficiency { get; set; }
    }

    public class ProgramProfile
    {
        public string dataset { get; set; }	
        public int curr_time_rpts { get; set; }
        public int num_incomplete_time_rpts { get; set; }
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

    public class ProgramLinks
    {
        public string dataset  { get; set; }	
        public int entry_id  { get; set; }
        public string section_type	 { get; set; }
        public string short_title { get; set; }
        public string heading	 { get; set; }
        public string link	 { get; set; }
        public string link_group { get; set; }
        public int? link_level { get; set; }
        public int? link_order { get; set; }
        public string UIUC_Program_ID { get; set; }
        public string UIUC_ProgramGroup	 { get; set; }
        public bool not_in_flag { get; set; }
        public string short_heading { get; set; }
    }

    public class ProgramData
    {
        public List<ProgramAuthorized> AuthorizedDataSet { get; set; }
        public List<ProgramDeficiency> DeficiencyDataSet { get; set; }
        public List<ProgramProfile> ProfileDataSet { get; set; }
        public List<ProgramLinks> LinksDataSet { get; set; }

        public ProgramData(List<ProgramAuthorized> d1, List<ProgramDeficiency> d2, List<ProgramProfile> d3, List<ProgramLinks> d4)
        {
            this.AuthorizedDataSet = d1;
            this.DeficiencyDataSet = d2;
            this.ProfileDataSet = d3;
            this.LinksDataSet = d4;
        }
    }

}