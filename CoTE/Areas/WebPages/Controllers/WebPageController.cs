using CoTE.Areas.WebPages.Models;
using FormProcessor.Models;
using CoTE.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace CoTE.Areas.WebPages.Controllers
{

    public class WebPageController : Controller
    {
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

        //
        // GET: /WebPages/WebPage/

        [HttpGet, ValidateInput(false)]
        public ActionResult Index()
        {
            string returnURL = "";
            string portal = "";

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

            portal = (string)Request.QueryString["Portal"] ?? "";
            if (portal != "")
            {
                Session["portal"] = portal;
                ViewBag.Portal = portal;
            }

            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Index(FormCollection form)
        {
            CoTE.Models.ErrorMessage err = new CoTE.Models.ErrorMessage();
            string username = form["username"];
            string password = form["password"];
            string token = form["token"];
            string auth_type = "";
            bool success = false;

            // Remove all session variables
            Session.RemoveAll();

            //Foil injection SQL
            username.Replace(" ", "");
            username.Replace("'", "");
            username.Replace("=", "");
            username.Replace("*", "");
            username.Replace(";", "");

            // Set the portal, if it hasn't already been set in the query string
            string portal = form["portal"];
        
            /*
            string portal = form["portal_redirect"] ?? form["portal"];
            if (portal == "")
            {
                portal = form["portal"];
            }
            */

             
            // If they are an AD user, authencate them using AD
            if (!username.Contains("@"))
            {
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
                    Session["portal"] = portal;
                    Response.Cookies["authenticated_user"].Value = username;
                    Response.Cookies["portal"].Value = portal;
                    Response.Cookies["session_id"].Value = GenerateRandomNumber();
                }
            }
            else
            {
                // Check to see if they have an account, but it isn't setup
                using (var u = new UsersContext())
                {
                    var userquery = "SELECT * "
                        + "FROM cte.dbo.v_LOGINS "
                        + "WHERE username = "
                        + "'" + username + "'";
                    var userlist = u.Database.SqlQuery<UserProfile>(userquery).ToList<UserProfile>();

                    var membershipquery = "SELECT * "
                        + "FROM cte.dbo.webpages_membership "
                        + "WHERE userid = "
                        + "'" + WebSecurity.GetUserId(username) + "'";

                    var membershiplist = u.Database.SqlQuery<UserProfile>(membershipquery).ToList<UserProfile>();

                    if (userlist.Count == 0)    // Check if user exists in the database
                    {
                        err.strErrorMessage = "You do not have an account";
                    }
                    else if ((membershiplist.Count == 0) || (!WebSecurity.IsConfirmed(username)))  // Check if the user exists in simplemembership
                    {
                        err.strErrorMessage = @"Your account is not setup, please click <a style='color:white;' href=/dotnet/account.aspx/SendAccountEmails?EmailType=1&MembershipCount=" + membershiplist.Count.ToString() + "&username=" + username + @">here</a> to have your account confirmation sent to you.";

                    }
                    else if (WebSecurity.IsAccountLockedOut(username, 3, 3600) == true) // Is the user locked out?
                    {
                        err.strErrorMessage = @"Your Account is Locked Out, please click <font color='white'><a href=/dotnet/account.aspx/SendAccountEmails?EmailType=2&MembershipCount=" + membershiplist.Count.ToString() + "&username=" + username + @">here</a></font> to have your account unlock code sent to you.";
                    }
                    else if (!WebSecurity.Login(username, password, false))  // See if the user has the correct password
                    {
                        err.strErrorMessage = "Incorrect Password";
                    }

                }

                if (err.strErrorMessage == null)
                {
                    auth_type = "MS";
                    success = true;
                    //authenticated_user = User.Identity.Name;
                    Session["authenticated_user"] = username;
                    Response.Cookies["authenticated_user"].Value = username;
                    Response.Cookies["session_id"].Value = GenerateRandomNumber();
                    Response.Cookies["portal"].Value = portal;
                    Session["portal"] = portal;
                }

            }

            //  If they have passed all other test then login
            if (success)
            {

                // Write to WEB_AUTHENTICATED_USERS table
                var db = new CoTE.Models.CoTEDB();
                db.Database.SqlQuery<StoredProcedure>("Insert into WEB_Authenticated_Sessions (username, token, start_time, ip_address, logged_out, authentication_type, user_agent) values ({0}, {1}, {2}, {3}, {4}, {5}, {6})", username, token, DateTime.Now.ToString(), Request.ServerVariables["REMOTE_ADDR"], false, auth_type, Request.UserAgent.ToString()).ToList();

                //string URL = (string)Session["ReturnURL"] ?? "";
                var cookie = HttpContext.Request.Cookies["ReturnURL"];
                if (cookie == null)
                {
                    Response.Redirect("/dotnet/account.aspx/loginsuccess");
                }
                else
                {
                    Response.Redirect(cookie.Value.ToString());
                }
            }

            ViewBag.Portal = portal;
            ViewBag.Location = form["location"];
            ViewBag.ErrorMessage = err.strErrorMessage;
            return View();
        }
        
        [HttpGet]
        public ActionResult Certification_Application()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Certification_Application(FormCollection form)
        {
            if (form["answer"] == "Yes")
            {
                return Redirect("/dotnet/formprocessor/index.aspx/index?form_id=319&index_id=391");
            }

            ViewBag.Answer = form["answer"];

            return View();
        }

        public ActionResult Sign_Rec()
        {

            string strRecord_Id = HttpContext.Request["record_id"];
            string username = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            using (var db = new WebPageContext())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[portal_program_sign_rec]";
                cmd.Parameters.Add(new SqlParameter("@record_id", strRecord_Id));
                cmd.Parameters.Add(new SqlParameter("@UserName", username));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                }
                catch (Exception e)
                {
                    //Redirect to an error page
                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                    return Redirect("/dotnet/FormProcessor/Error.aspx/HttpError");
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }

            return Redirect("/dotnet/formprocessor/index.aspx/index?form_id=322&index_id=392");
        }

        public ActionResult ReportList()
        {
            string username = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                //username = HttpContext.User.Identity.Name;
                
                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string cookie_username = cookie.Value;

                // Check if Authenticated_user is set if so use it
                if (cookie_username != "")
                {
                    username = cookie_username;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }

            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.Username = username;

            return View();
        }

        public ActionResult ReportList_Annual()
        {
            string username = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                //username = HttpContext.User.Identity.Name;

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string cookie_username = cookie.Value;

                // Check if Authenticated_user is set if so use it
                if (cookie_username != "")
                {
                    username = cookie_username;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }

            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.Username = username;

            return View();
        }
        
        [HttpGet]
        public ActionResult TestingResults()
        {
            string username = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.Username = username;

            return View(); 
        }

        [HttpPost]
        public ActionResult TestingResults(FormCollection form)
        {
            string username = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.Username = username;
            
            //Get search field
            ViewBag.UIN = form["uin"];

            return View();
        }

        [HttpGet]
        public ActionResult EmailList() // For emailing coops based on course
        {
            string username = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            // Initialize ViewBag.Course
            ViewBag.Course = new string[10];
            for (int i = 0; i < 10; i++)
            {
                ViewBag.Course[i] = "";
            }

            ViewBag.Username = username; 
            
            return View();
        }

        [HttpPost]
        public ActionResult EmailList(FormCollection form) // For emailing coops based on course
        {

            string username = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.Username = username;
            
            //Get search field
            ViewBag.Term = form["term"];
            ViewBag.Course = new string [10];
            ViewBag.Course[0] = form["course1"];
            ViewBag.Course[1] = form["course2"];
            ViewBag.Course[2] = form["course3"];
            ViewBag.Course[3] = form["course4"];
            ViewBag.Course[4] = form["course5"];
            ViewBag.Course[5] = form["course6"];
            ViewBag.Course[6]= form["course7"];
            ViewBag.Course[7]= form["course8"];
            ViewBag.Course[8]= form["course9"];
            ViewBag.Course[9] = form["course10"];

            return View();
        }

        [HttpGet]
        public ActionResult FacilityList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FacilityList(FormCollection form)
        {

            //Get search field
            ViewBag.SearchTerm = form["search"];

            return View();
        }

        public ActionResult PeopleDisplay()
        {

            string strGroup = HttpContext.Request["group"];
            ViewBag.Group = strGroup;

            switch (strGroup)
            {
                case "Staff":
                    ViewBag.Title = "Council on Teacher Education Staff";
                    break;
                case "PC":
                    ViewBag.Title = "Clinical Experiences Program Coordinators";
                    break;
                case "EC":
                    ViewBag.Title = "Council on Teacher Education Executive Committee";
                    break;
                case "AC":
                    ViewBag.Title = "Council Area-of-Specialization Committees";
                    break;
            }

            using (var db = new WebPageContext())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[WEB_PEOPLE_DISPLAY]";
                cmd.Parameters.Add(new SqlParameter("@group", strGroup));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Move to first result set and read coop data
                    var people = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<PeopleDisplay>(reader);

                    List<PeopleDisplay> PeopleDisplayData = people.ToList();

                    PeopleDisplay_Data PeopleDisplayModel = new PeopleDisplay_Data(PeopleDisplayData);

                    List<PeopleDisplay_Data> viewModelList = new List<PeopleDisplay_Data>();
                    viewModelList.Add(PeopleDisplayModel);
                    return View(viewModelList);

                }
                finally
                {
                    db.Database.Connection.Close();
                }
            } 
            
            return View();
        }

        [HttpGet, ValidateInput(false)]
        public ActionResult WebPage()
        {

            string webpage_sql = "";
            string page_level = HttpContext.Request["page_level"] ?? "";
            string page_id = HttpContext.Request["page_id"] ?? "";
            string webpage_id = "";
            string page_type = "";

            ViewBag.page_level = page_level;

            using (var db = new WebPageContext())
            {
                try
                {

                    // If using Code First we need to make sure the model is built before we open the connection
                    // This isn't required for models created with the EF Designer
                    db.Database.Initialize(force: false);

                    //Make sure that page level is decimal
                    //Match m = null;
                    //Regex re = new Regex(@"\D");

                    // Create a SQL command to execute the sproc
                    //if (page_level != "" && re.Match(page_level).Success)
                    if (page_level != "")
                        {
                        page_type = "WS";
                        webpage_id = page_level;
                    }
                    //else if (re.Match(page_id).Success)
                    else 
                    {
                        page_type = "FP";
                        webpage_id = page_id;
                    }

                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "CTE.dbo.webpage_display";
                    cmd.Parameters.Add(new SqlParameter("page_type", page_type));
                    cmd.Parameters.Add(new SqlParameter("id", webpage_id));

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Move to first result set and read coop data
                    var webpage = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<WebPage>(reader);

                    List<WebPage> WebPageData = webpage.ToList();

                    WebPage_Data WebPageModel = new WebPage_Data(WebPageData);

                    List<WebPage_Data> viewModelList = new List<WebPage_Data>();
                    viewModelList.Add(WebPageModel);
                    return View(viewModelList);

                }
                catch (Exception e)
                {
                    //Redirect to an error page
                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                    return Redirect("~/FormProcessor/Error.aspx/HttpError");
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }

            return View();
        }

        [HttpGet]
        public ActionResult WebPage_Edit ()
        {
            //Get record_id
            string record_id = HttpContext.Request["record_id"] ?? "";

            string webpage_url = "";

            using (var db = new WebPageContext())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CTE.DBO.webpage_edit";
                cmd.Parameters.Add(new SqlParameter("@record_id", record_id));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        webpage_url = reader[0].ToString();
                    }

                    reader.Close();

                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }

            return Redirect(webpage_url);

        }
        public ActionResult SearchResults()
        {
            string strSearchWord = HttpContext.Request["q"];
            ViewBag.searchword = strSearchWord;

            return View();
        }

        [HttpGet]
        public ActionResult EmailLists() // For emailing everyone
        {
            string username = "";

            // Get username from cookie
            var cookie = HttpContext.Request.Cookies["authenticated_user"];
            string cookie_username = cookie.Value;

            // Check if Authenticated_user is set if so use it
            if (cookie_username != "")
            {
                username = cookie_username;
            }
            else
            {
                username = HttpContext.User.Identity.Name;
            }

            // Initialize ViewBag.Course
            ViewBag.Condition = "";
            ViewBag.Username = username;

            return View();
        }

        [HttpPost]
        public ActionResult EmailLists(FormCollection form) // For emailing everyone
        {

            string username = "";

            // Get username from cookie
            var cookie = HttpContext.Request.Cookies["authenticated_user"];
            string cookie_username = cookie.Value;

            // Check if Authenticated_user is set if so use it
            if (cookie_username != "")
            {
                username = cookie_username;
            }
            else
            {
                username = HttpContext.User.Identity.Name;
            }


            ViewBag.Username = username;

            //Get search field
            ViewBag.Audience = form["audience"];
            ViewBag.List_id = form["condition"];
            ViewBag.Program = form["program"];
            ViewBag.Term = form["term"];

            return View();
        }

        [HttpGet]
        public ActionResult DragNDrop()
        {
            string strSurveyID = HttpContext.Request["survey_id"];
            ViewBag.survey_id = strSurveyID;

            return View();
        }

        [HttpPost]
        public ActionResult DragNDrop(FormCollection form)
        {
            ViewBag.survey_id = form["survey_id"];
            return View();
        }

        [HttpGet, ValidateInput(false)]
        public JsonResult DragNDropSave(string fieldnames, string fieldvalues, string insertparams)
        {
            string insert_statement;

            if (fieldnames.Length > 0)
            {
                insert_statement = "";
                insert_statement = "Insert into cte.dbo.get_survey_year1 (" + fieldnames + ") values (" + fieldvalues + ")";

                using (var db = new WebPageContext())
                {
                    // If using Code First we need to make sure the model is built before we open the connection
                    // This isn't required for models created with the EF Designer
                    db.Database.Initialize(force: false);

                    db.Database.Connection.Open();

                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = insert_statement;
                    var insert_command = cmd.ExecuteReader();
                    insert_command.Close();
                }
            }

            bool result = true;

            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }
    
    }
}
