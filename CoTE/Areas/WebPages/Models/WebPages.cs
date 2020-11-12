using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CoTE.Areas.WebPages.Models
{
    public class WebPageContext : DbContext
    {
        public WebPageContext()
            : base("UserDB")
        {
        }
    }

    public class PeopleDisplay
    {
        public string dataset { get; set; }
        public string role { get; set; }
        public string committee_role { get; set; }
        public string UIUC_ProgramName { get; set; }
        public string full_name { get; set; }
        public string job_title { get; set; }
        public string office_address { get; set; }
        public string office_phone { get; set; }
        public string net_id { get; set; }
        public string email { get; set; }
    }

    public class PeopleDisplay_Data
    {
        public List<PeopleDisplay> PeopleDisplayDataSet { get; set; }

        public PeopleDisplay_Data(List<PeopleDisplay> d1)
        {
            this.PeopleDisplayDataSet = d1;
        }
    }

    public class FacilityList
    {
        public string district { get; set; }
        public string facility { get; set; }
        public string url { get; set; }
    }

    public class FacilityList_Data
    {
        public List<FacilityList> FacilityListDataSet { get; set; }

        public FacilityList_Data(List<FacilityList> d1)
        {
            this.FacilityListDataSet = d1;
        }
    }

    public class WebPage
    {
        public Int64 page_id { get; set; }
        public string page_level { get; set; }
        public string title { get; set; }
        public string section { get; set; }
        public string html { get; set; }
        public string left_menu { get; set; }
        public bool draft { get; set; }
        public bool checked_out { get; set; }
    }

    public class WebPage_Data
    {
        public List<WebPage> WebPageDataSet { get; set; }

        public WebPage_Data(List<WebPage> d1)
        {
            this.WebPageDataSet = d1;
        }
    }
}