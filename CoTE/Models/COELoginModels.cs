using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CoTE.Models
{
    public class EdUsersContext : DbContext
    {
        public EdUsersContext()
            : base("EdDB")
        {
        }
    }

    public class EdDB : DbContext
    {
        public EdDB()
            : base("EdDB")
        {
        }
    }

    public class COEErrorMessage
    {
        public int intErrorCode { get; set; }
        public string strErrorMessage { get; set; }
    }

    public class COEPortalRedirect
    {
        public string URL { get; set; }
    }

    public class COEAuthorized
    {
        public string dataset { get; set; }
        public string result { get; set; }
    }
}