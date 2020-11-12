using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CoTE.Areas.LACES.Models
{
    public class LACES_Context : DbContext
    {
        public LACES_Context()
            : base("UserDB")
        {
        }

        public DbSet<Student> Students { get; set; }

    }

    public class LACES_Authorized
    {
        public string dataset { get; set; }
        public string user_name { get; set; }
    }

    public class LACES_Links
    {
        public string dataset { get; set; }
        public int entry_id { get; set; }
        public string section_type { get; set; }
        public string short_title { get; set; }
        public string heading { get; set; }
        public string link { get; set; }
        public int? link_order { get; set; }
        public string link_group { get; set; }
        public int? link_level { get; set; }
        public string role_code { get; set; }
        public bool not_in_flag { get; set; }
    }

    public class LACES_Data
    {
        public List<LACES_Authorized> AuthorizedDataSet { get; set; }
        public List<LACES_Links> LinksDataSet { get; set; }

        public LACES_Data(List<LACES_Authorized> d1, List<LACES_Links> d2)
        {
            this.AuthorizedDataSet = d1;
            this.LinksDataSet = d2;
        }
    }

    public class email_merge_fields
    {
        public string field_name_letter { get; set; }
        public string field_name_db { get; set; }
        public Boolean group_by { get; set; }
        public Boolean recipient { get; set; }
        public string group_key { get; set; }
        public string delimiter { get; set; }
        public string dataset_sql { get; set; }
        public string dataset_name { get; set; }
    }

    public class PDH_EVENT_EOC
    {
        public string full_name { get; set; }
        public string event_title { get; set; }
        public string event_coordinator { get; set; }
        public string dates { get; set; }
        public string pdus { get; set; }
        public string eoc_date { get; set; }
        public string IEIN { get; set; }
    }

    public class PDH_EVENT_EOC_Data
    {
        public List<PDH_EVENT_EOC> EOCDataSet { get; set; }

        public PDH_EVENT_EOC_Data(List<PDH_EVENT_EOC> d1)
        {
            this.EOCDataSet = d1;
        }
    }


    [Table("CTE_STU_DEMO")]
    public class Student
    {
        [Key]
        public int cte_stu_id { get; set; }
        [Required]
        public string ssn { get; set; }
 
    }

    public class PDH_EVENT_OOS_EOC
    {
        public string full_name { get; set; }
        public string event_title { get; set; }
        public string event_coordinator { get; set; }
        public string dates { get; set; }
        public string pdus { get; set; }
        public string eoc_date { get; set; }
    }

    public class PDH_EVENT_OOS_EOC_Data
    {
        public List<PDH_EVENT_OOS_EOC> OOSEOCDataSet { get; set; }

        public PDH_EVENT_OOS_EOC_Data(List<PDH_EVENT_OOS_EOC> d1)
        {
            this.OOSEOCDataSet = d1;
        }
    }
}