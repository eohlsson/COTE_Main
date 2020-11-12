using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Globalization;

namespace CoTE.Areas.Portals.Models
{
    public class EvaluationsContext : DbContext
    {
        public EvaluationsContext()
            : base("UserDB")
        {
        }

        public DbSet<Danielson> Danielsons { get; set; }
 
    }

    [Table("CTE_STU_EVAL_DANIELSON")]
    public class Danielson
    {
        [Key]
        public int cte_stu_eval_id { get; set; }
        [Required]
        public int? cte_stu_id { get; set; }
        public int? uiuc_program_id { get; set; }
        public int? evaluator { get; set; }
        [Required]
        [DateRange("2010/12/01", "")]
        public DateTime? evaluation_date { get; set; }
        [Required]
        public string placement_type { get; set; }
        public string position { get; set; }
        public string eval_type { get; set; }
        public string domain1_a { get; set; }
        public string domain1_b { get; set; }
        public string domain1_c { get; set; }
        public string domain1_d { get; set; }
        public string domain1_e { get; set; }
        public string domain1_f { get; set; }
        public string domain1_g { get; set; }
        public string domain1_comments { get; set; }
        public string domain2_a { get; set; }
        public string domain2_b { get; set; }
        public string domain2_c { get; set; }
        public string domain2_d { get; set; }
        public string domain2_e { get; set; }
        public string domain2_comments { get; set; }
        public string domain3_a { get; set; }
        public string domain3_b { get; set; }
        public string domain3_c { get; set; }
        public string domain3_d { get; set; }
        public string domain3_e { get; set; }
        public string domain3_f { get; set; }
        public string domain3_g { get; set; }
        public string domain3_h { get; set; }
        public string domain3_comments { get; set; }
        public string domain4_a { get; set; }
        public string domain4_b { get; set; }
        public string domain4_c { get; set; }
        public string domain4_d { get; set; }
        public string domain4_e { get; set; }
        public string domain4_f { get; set; }
        public string domain4_g { get; set; }
        public string domain4_comments { get; set; }
        public bool signature { get; set; }
        public string summary { get; set; }
        public Int16? total_score { get; set; }
        public string created_by { get; set; }
        public DateTime? created_date { get; set; }
        public string modified_by { get; set; }
        public DateTime? modified_date { get; set; }
    }

    public class DateRangeAttribute : ValidationAttribute
    {
        private const string DateFormat = "yyyy/MM/dd";
        private const string DefaultErrorMessage =
     "'{0}' must be a date between {1:d} and {2:d}.";

        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }

        public DateRangeAttribute(string minDate, string maxDate)
            : base(DefaultErrorMessage)
        {

            // If blank assume current date
            if (maxDate.ToString() == "")
            {
                maxDate = DateTime.Now.AddYears(1).ToString("yyyy/MM/dd");
            }
    
            MinDate = ParseDate(minDate);
            MaxDate = ParseDate(maxDate);
        }

        public override bool IsValid(object value)
        {
            if (value == null || !(value is DateTime))
            {
                return true;
            }
            DateTime dateValue = (DateTime)value;
            return MinDate <= dateValue && dateValue <= MaxDate;
        }
        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
     ErrorMessageString,
                name, MinDate, MaxDate);
        }

        private static DateTime ParseDate(string dateValue)
        {
            return DateTime.ParseExact(dateValue, DateFormat,
     CultureInfo.InvariantCulture);
        }
    }
}