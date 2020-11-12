using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using CoTE.Models;
using System.DirectoryServices;
using WebMatrix.WebData;
using System.Web.Security;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;

namespace CoTE.Controllers
{
    public class COELoginController : Controller
    {
        //
        // GET: /COELogin/

        public static string returnUrl { get; set; }
        public static string portal { get; set; }
        public static string authenticated_user { get; set; }

        private EdUsersContext db = new EdUsersContext();

        static string GenerateRandomNumber()
        {
            byte[] byt = new byte[4];
            RNGCryptoServiceProvider rngCrypto =
                new RNGCryptoServiceProvider();
            rngCrypto.GetBytes(byt);
            int randomNumber = (BitConverter.ToInt32(byt, 0));
            if (randomNumber > 0)
            {
                randomNumber = randomNumber * -1;
            }

            return randomNumber.ToString();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login() // http://localhost:25903/dotnet/account.aspx/login
        {
            string returnURL = "";

            // Logout, before logging in
            DirectoryEntry ent = new DirectoryEntry("LDAP://ad.uillinois.edu");
            ent.Close();
            WebSecurity.Logout();

            // Remove all session variables
            Session.RemoveAll();


            // Remove cookies
            HttpCookie aCookie;
            string cookieName;
            int limit = Request.Cookies.Count;
            for (int i = 0; i < limit; i++)
            {
                cookieName = Request.Cookies[i].Name;
                aCookie = new HttpCookie(cookieName);
                aCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(aCookie);
            }

            returnURL = (string)Request.QueryString["ReturnUrl"] ?? "";
            if (returnURL != "")
            {
                Session["returnUrl"] = returnURL.Replace("&amp;", "&");
            }

            Session["portal"] = Request.QueryString["portal"];
            //portal = Request.QueryString["portal"];
            ViewBag.portal = (string)Session["portal"] ?? "";

            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection form)
        {

            ErrorMessage err = new ErrorMessage();
            string username = form["username"];
            string password = form["password"];
            string token = form["token"];
            string auth_type = "";
            bool success = false;

            //Foil injection SQL
            username.Replace(" ", "");
            username.Replace("'", "");
            username.Replace("=", "");
            username.Replace("*", "");
            username.Replace(";", "");

            // Check to see if they are in the active directory 
            try
            {
                DirectoryEntry ent = new DirectoryEntry("LDAP://ad.uillinois.edu", username, password);
                FormsAuthentication.SetAuthCookie(username, false);
                Console.WriteLine(ent.Name);
            }
            catch (DirectoryServicesCOMException)
            {
                err.strErrorMessage = "Incorrect Active Directory Password";
            }

            if (err.strErrorMessage == null)
            {
                auth_type = "AD";
                success = true;
                //authenticated_user = username;
                Session["authenticated_user"] = username;
                Response.Cookies["authenticated_user"].Value = username;
                Response.Cookies["session_id"].Value = GenerateRandomNumber();
            }


            //  If they have passed all other test then login
            if (success)
            {

                // Write to WEB_AUTHENTICATED_USERS table
                var db = new CoTEDB();
                db.Database.SqlQuery<StoredProcedure>("Insert into WEB_Authenticated_Sessions (username, token, start_time, ip_address, logged_out, authentication_type) values ({0}, {1}, {2}, {3}, {4}, {5})", username, token, DateTime.Now.ToString(), Request.ServerVariables["REMOTE_ADDR"], false, auth_type).ToList();

                string URL = (string)Session["returnUrl"] ?? "";
                if (URL == "")
                {
                    ViewBag.username = username;
                    Response.Redirect("/dotnet/coelogin.aspx/coemenu");
                }
                else
                {
                    Response.Redirect(URL);
                }
            }

            ViewBag.ErrorMessage = err.strErrorMessage;
            return View();
        }


        [Authorize]
        [HttpGet]
        public ActionResult COEMenu()
        {
            return View();
        }
        
        public ActionResult Logout()
        {

            // Remove cookies
            HttpCookie aCookie;
            string cookieName;
            int limit = Request.Cookies.Count;
            for (int i = 0; i < limit; i++)
            {
                cookieName = Request.Cookies[i].Name;
                aCookie = new HttpCookie(cookieName);
                aCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(aCookie);
            }

            // Remove all session variables
            Session.RemoveAll();

            DirectoryEntry ent = new DirectoryEntry("LDAP://ad.uillinois.edu");
            ent.Close();
            WebSecurity.Logout();
            return View();
        }

    }
}
