using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CoTE.Areas.SCE.Models
{
    public class SCE_Context : DbContext
    {
        public SCE_Context()
            : base("UserDB")
        {
        }
    }

    public class SCE_Authorized
    {
        public string dataset { get; set; }
        public string user_name { get; set; }
    }

    public class SCE_Links
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

    public class SCE_Data
    {
        public List<SCE_Authorized> AuthorizedDataSet { get; set; }
        public List<SCE_Links> LinksDataSet { get; set; }

        public SCE_Data(List<SCE_Authorized> d1, List<SCE_Links> d2)
        {
            this.AuthorizedDataSet = d1;
            this.LinksDataSet = d2;
        }
    }
}