using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CoTE.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("UserDB")
        {
        }
    }

    public class CoTEDB : DbContext
    {
        public CoTEDB()
            : base("UserDB")
        {
        }
    }

    public class StoredProcedure
    {
    }

    public class UserProfile
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string email { get; set; }
        public string person_type { get; set; }
    }

    public class ErrorMessage
    {
        public int intErrorCode { get; set; }
        public string strErrorMessage { get; set; }
    }

    public class PortalRedirect
    {
        public string URL { get; set; }
    }

    public class Authorized
    {
        public string dataset { get; set; }
        public string result { get; set; }
    }
}