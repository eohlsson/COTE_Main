using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using System.DirectoryServices;
using CoTE.Models;
using System.IO; 
using System.Net.Mail; 
using System.Net.Mime;
using System.Web.Security;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using System.Net.Http;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Security.Cryptography; 

namespace CoTE.Controllers
{
 
    public class AccountController : Controller
    {
        public static string returnUrl { get; set; }
        public static string portal { get; set; }
        public static string authenticated_user { get; set; }
        
        private UsersContext db = new UsersContext();

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
        [ValidateInput(false)]
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
                Session["ReturnUrl"] = returnURL.Replace("&amp;", "&");
                Response.Cookies["ReturnURL"].Value = returnURL.Replace("&amp;", "&");
            }

            Session["portal"] = (string)Request.QueryString["portal"] ?? "";
            Response.Cookies["portal"].Value = Request.QueryString["portal"];
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

            // Check to see if they are returning with a token
                // Unlocking account
                // Setting up account

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
                        Response.Cookies["authenticated_user"].Value = username;
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
                        err.strErrorMessage = @"Your account is not setup, please click <font color='white'><a href=/dotnet/account.aspx/SendAccountEmails?EmailType=1&MembershipCount=" + membershiplist.Count.ToString() + "&username=" + username + @">here</a></font> to have your account confirmation sent to you.";

                    }
                    else if (WebSecurity.IsAccountLockedOut(username, 3, 3600) == true) // Is the user locked out?
                    {
                        err.strErrorMessage = @"Your Account is Locked Out, please click <a href=/dotnet/account.aspx/SendAccountEmails?EmailType=2&MembershipCount=" + membershiplist.Count.ToString() + "&username=" + username + @">here</a> to have your account unlock code sent to you.";
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
                }

            }

            //  If they have passed all other test then login
            if (success)
            {
                
                // Write to WEB_AUTHENTICATED_USERS table
                var db = new CoTEDB();
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

            Session["portal"] = (string)Request.QueryString["portal"] ?? "";
            ViewBag.portal = (string)Session["portal"];
            ViewBag.ErrorMessage = err.strErrorMessage;
            return View();
        }

        [HttpGet]
        public ActionResult Retrieve() 
        {
            return View();
        }

        [HttpPost]
        public ActionResult Retrieve(FormCollection form)
        {

            ErrorMessage err = new ErrorMessage();
            string username = form["username"];

            //Foil injection SQL
            username.Replace(" ", "");
            username.Replace("'", "");
            username.Replace("=", "");
            username.Replace("*", "");
            username.Replace(";", "");


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

                    if (username == "")
                    {
                        err.strErrorMessage = "Email Address field cannot be left blank";
                    }
                    else if (userlist.Count == 0)    // Check if user exists in the database
                    {
                        err.strErrorMessage = "Your email is not in our database, please contact your Placement Coordinator to confirm your information";
                    }
                    else if ((membershiplist.Count == 0) || (!WebSecurity.IsConfirmed(username)))  // Check if the user exists in simplemembership
                    {
                        err.strErrorMessage = @"Your account is not setup, please click <a href=/dotnet/account.aspx/SendAccountEmails?EmailType=1&MembershipCount=" + membershiplist.Count.ToString() + "&username=" + username + @">here</a> to have your account confirmation sent to you.";
                    }
                    else if (WebSecurity.IsConfirmed(username) == true)  // User already confirmed
                    {
                        err.strErrorMessage = "You have already confirmed your account";
                    }
            }

            ViewBag.ErrorMessage = err.strErrorMessage;
            return View();
        }

        [HttpGet]
        public ActionResult Confirm()
        {
            string confirmationtype = Request["type"];
            string username = Request["username"];
            string token = Request["token"];
            var u = new UsersContext();

            ViewBag.Username = username;
            ViewBag.Token = token;
            ViewBag.ConfirmationType = confirmationtype;

            switch (confirmationtype)
            {
                case "1": // Confirmation:  Initial account setup

                    var userquery = "SELECT * "
                        + "FROM cte.dbo.webpages_membership "
                        + "WHERE confirmationtoken = "
                        + "'" + token + "'";
                    var userlist = u.Database.SqlQuery<UserProfile>(userquery).ToList<UserProfile>();

                    if (token != "")
                    {
                        if (userlist.Count == 0)  //No user found for token
                        {
                            ViewBag.ErrorMessage = "No User Found for the supplied Token";
                            ViewBag.ShowFields = false;
                            //Log error and email
                        }
                        else if (WebSecurity.IsConfirmed(username) == true)  // User already confirmed
                        {
                            ViewBag.ErrorMessage = "You have already confirmed your account";
                            ViewBag.ShowFields = false;
                        }
                        else if (WebSecurity.ConfirmAccount(token) == true)
                        {
                            ViewBag.ShowFields = true;
                            WebSecurity.Login(username, "4s&7yaOff");
                            ViewBag.InformationMessage = "Your account has been confirmed.  Please select a password below, then click on the Submit button below.";
                        }

                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Invalid Request, No Account token was Passed in";
                        ViewBag.ShowFields = false;
                        //Log error and email
                    }

                    break;
                case "2": // Confirmation:  Account unlock
                        if (WebSecurity.IsAccountLockedOut(username, 3, 3600) == true)
                        {
                            // ADD CALL TO ACCOUNT_UNLOCK stored procedure
                            db.Database.SqlQuery<StoredProcedure>("exec cte.dbo.account_unlock {0}", username).ToList();

                            //Membership.GetUser(username).UnlockUser();
                        }
                        //usr.UnlockUser(username);
                        ViewBag.ShowFields = false;
                        ViewBag.InformationMessage = "Your Account has been Unlocked, please click <a href='/dotnet/webpages/webpage.aspx#UserPortals'>here</a> to login or <a href=/dotnet/account.aspx/resetpassword>here</a> to reset your password";
                    break;
                case "3": // Confirmation:  Reset password

                    if (WebSecurity.GetUserIdFromPasswordResetToken(token) != 0)
                    {
                        ViewBag.ShowFields = true;
                        ViewBag.InformationMessage = "To change your password, fill in their fields below and click on the Submit button below.";
                        //Add unlock check box for confirmationtype 2 & 3
                    }
                    else
                    {
                        ViewBag.ShowFields = false;
                        ViewBag.ErrorMessage = "No user found for the password token that was passed in.";
                    }

                    break;
            }

            return View();

        }

        [HttpPost]
        public ActionResult Confirm(FormCollection form)
        {

            if (form["password"] != form["confirm_password"])
            {
                ViewBag.ShowFields = true;
                ViewBag.ErrorMessage = "Password values do not match, please reenter";
            }
            else if (form["password"].Length < 6)  //Password must be greater than 6 characters
            {
                ViewBag.ShowFields = true;
                ViewBag.ErrorMessage = "Password must be greater than 6 characters, please enter a new password";
            }

            if (ViewBag.ErrorMessage == null)
            {
                switch (form["confirmationtype"])
                {
                    case "1":  // Initial account confirmation
                        {
                            WebSecurity.ChangePassword(WebSecurity.CurrentUserName, "4s&7yaOff", form["password"]);
                            ViewBag.ShowFields = false;
                            ViewBag.InformationMessage = "Password Successfully Changed. Please click <a href='/dotnet/webpages/webpage.aspx#UserPortals'>here</a> to login";
                            break;
                        }
                    case "2":  // Unlock account
                        {

                            break;
                        }
                    case "3":  // Password reset
                        {
                            WebSecurity.ResetPassword(form["token"], form["password"]);
                            ViewBag.ShowFields = false;
                            ViewBag.InformationMessage = "Password Successfully Changed. Please click <a href='/dotnet/webpages/webpage.aspx#UserPortals'>here</a> to login";
                            break;
                        }
                }
            }

            return View();
        }
        
        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(FormCollection form)
        {
            string username = form["email"];

            // Check if user is valid
            using (var u = new UsersContext())
            {
                var userquery = "SELECT * "
                    + "FROM cte.dbo.v_logins V"
                    + "	INNER JOIN webpages_Membership M ON M.UserId = V.id "
                    + "WHERE username = "
                    + "'" + username +"'";
                var userlist = u.Database.SqlQuery<UserProfile>(userquery).ToList<UserProfile>();

                if (userlist.Count == 0)
                {
                    ViewBag.ErrorMessage = "No username found for this email address";
                }
                else // Call SendAccountEmails if it is
                {
                    Response.Redirect("/dotnet/account.aspx/SendAccountEmails?EmailType=3&MembershipCount=0&username=" + username);
                }
            }

            return View();

        }

        [Authorize]
        public ActionResult LoginSuccess()
        {
            
                            // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string username = cookie.Value;
                
                // Check to see if they specified a portal
                var cookie2 = HttpContext.Request.Cookies["portal"];
                string portal = "";
                if (cookie2 != null)
                {
                    portal = cookie2.Value;
                }

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[get_portal]";
                cmd.Parameters.Add(new SqlParameter("@username", username));
                cmd.Parameters.Add(new SqlParameter("@portal", portal));
 
                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var urls = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<PortalRedirect>(reader);

                    foreach (var url in urls)
                    {
                        Response.Redirect(url.URL);
                    }
  
                }
                finally
                {
                    db.Database.Connection.Close();
                }
        
            return View();

        }

        public ActionResult SendAccountEmails(string EmailType, string membershipcount, string username)
        {
            string token = "";
            var u = new UsersContext();

            //EmailType 
            switch (EmailType)
            {
                // 1 = Confirm Account
                case "1":
                    if (membershipcount == "0")
                    {

                        // Double-check in case someone refreshes page
                        if (WebSecurity.IsConfirmed(username) == true)  // User already confirmed
                        {
                            ViewBag.ErrorMessage = "You have already confirmed your account";
                            return View();
                        }

                        token = WebSecurity.CreateAccount(username, "4s&7yaOff", true);
                        Console.WriteLine(token);
                        u.Database.SqlQuery<UserProfile>("EXECUTE cte.dbo.login_notifications @emailtype = {0}, @username = {1}, @token = {2}", "1", username, token).ToList();
                    }
                    else
                    {
                        u.Database.SqlQuery<UserProfile>("EXECUTE cte.dbo.login_notifications @emailtype = {0}, @username = {1}, @token = {2}", "1", username, null).ToList();
                    }                    
                    break;

                // 2 = Unlock Account
                case "2":
                   token = WebSecurity.GeneratePasswordResetToken(username);
                   u.Database.SqlQuery<UserProfile>("EXECUTE cte.dbo.login_notifications @emailtype = {0}, @username = {1}, @token = {2}", "2", username, token).ToList();
                   break;

                // 3 = Reset Password
                case "3":
                    token = WebSecurity.GeneratePasswordResetToken(username);
                    u.Database.SqlQuery<UserProfile>("EXECUTE cte.dbo.login_notifications @emailtype = {0}, @username = {1}, @token = {2}", "3", username, token).ToList();
                    break;
            }

            return View();

        }

        [HttpGet]
        [Authorize]
        public ActionResult Impersonate()
        {

            var cookie = HttpContext.Request.Cookies["authenticated_user"];
            string username = cookie.Value;

            ViewBag.Username = username;

            List<SelectListItem> portals = new List<SelectListItem>();

            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                try
                {

                    db.Database.Connection.Open();

                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandType = CommandType.Text;

                    // Load Value Lists
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "CTE.dbo.get_portal_program_impersonate";
                    cmd.Parameters.Add(new SqlParameter("@netid", username));

                    string portal;
                    string portal_name;

                    portals.Add(new System.Web.Mvc.SelectListItem { Text = "--Select a Portal--", Value = "" });

                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {

                            portal = reader[0].ToString();
                            portal_name = reader[1].ToString();

                            // Create field string
                            portals.Add(new SelectListItem { Text = portal_name, Value = portal });

                        }

                    }

                }
                catch (Exception e)
                {
                    //Redirect to an error page
                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }
            
            
/*
            portals.Add(new SelectListItem { Text = "--Select a Portal--", Value = "" });

            portals.Add(new SelectListItem { Text = "SCE", Value = "SCE" });

            portals.Add(new SelectListItem { Text = "Program Portal", Value = "program" });

            portals.Add(new SelectListItem { Text = "Cooperating Personnel Portal", Value = "coop" });

            portals.Add(new SelectListItem { Text = "Student Portal", Value = "student" });

//            portals.Add(new SelectListItem { Text = "Student Portal (Beta)", Value = "studentbeta" });

            portals.Add(new SelectListItem { Text = "Supervisor Portal", Value = "supervisor" });
*/
 
            ViewBag.PortalType = portals;

            List<SelectListItem> users = new List<SelectListItem>();
            ViewBag.Users = users;

            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Impersonate(FormCollection form)
        {
            bool authorized = false;

            var cookie = HttpContext.Request.Cookies["authenticated_user"];
            string username = cookie.Value;

            List<SelectListItem> users = new List<SelectListItem>();

            List<SelectListItem> portals = new List<SelectListItem>();

            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                try
                {

                    db.Database.Connection.Open();

                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandType = CommandType.Text;

                    // Load Value Lists
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "CTE.dbo.get_portal_program_impersonate";
                    cmd.Parameters.Add(new SqlParameter("@netid", username));

                    string portal;
                    string portal_name;

                    portals.Add(new System.Web.Mvc.SelectListItem { Text = "--Select a Portal--", Value = "" });

                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {

                            portal = reader[0].ToString();
                            portal_name = reader[1].ToString();

                            // Create field string
                            portals.Add(new SelectListItem { Text = portal_name, Value = portal });

                        }

                    }

                }
                catch (Exception e)
                {
                    //Redirect to an error page
                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }

            ViewBag.PortalType = portals;
            ViewBag.Users = users;
            
            if (form["Users"] == null || form["Users"] == "")
            {
                ViewBag.ErrorMessage = "Username cannot be left blank";

                return View();
            }

            // Check if authorized
            using (var db = new CoTEDB())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[portal_impersonate]";
                cmd.Parameters.Add(new SqlParameter("@impersonatee", form["Users"]));
                cmd.Parameters.Add(new SqlParameter("@impersonator", username));
                cmd.Parameters.Add(new SqlParameter("@portal", form["PortalType"]));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var authorizedDataset = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Authorized>(reader);

                    foreach (Authorized authority in authorizedDataset)
                    {
                        if (authority.result.ToString() == "true")
                        {
                            authorized = true;
                        }
                    }

                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }

            // Redirect if so
            if (authorized == true)
            {

                var db = new CoTEDB();
                db.Database.SqlQuery<StoredProcedure>("Insert into WEB_Authenticated_Sessions (username, token, start_time, ip_address, logged_out, authentication_type) values ({0}, {1}, {2}, {3}, {4}, {5})", form["Users"] + " as " + username, "", DateTime.Now.ToString(), Request.ServerVariables["REMOTE_ADDR"], false, "I").ToList();

                Response.Cookies["authenticated_user"].Value = form["Users"];

                switch (form["PortalType"])
                {
                    case "program":
                        return Redirect("/dotnet/portals/programportal.aspx");
                        //Response.Redirect("/dotnet/portals/programportal.aspx");
                        break;
                    case "coop":
                        return Redirect("/dotnet/portals/coopportal.aspx/checklist");
                        //return Redirect("/dotnet/portals/coopportal.aspx");
                        break;
                    case "student":
                        return Redirect("/dotnet/portals/studentportal.aspx/checklist");
                        break;
                    case "supervisor":
                        return Redirect("/dotnet/portals/supervisorportal.aspx/checklist");
                        break;
                    case "SCE":
                        return Redirect("/dotnet/sce/index.aspx");
                        break;
                }
            }

            // Return to view if not authorized

            ViewBag.ErrorMessage = "Username does not exist or you are not authorized to view this user";

            return View();
        }

        [ValidateInput(false)]
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

        [HttpGet]
        public JsonResult GetImpersonatees(string impersonator, string portal)
        {
            List<SelectListItem> new_values = new List<SelectListItem>();

            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                try
                {

                    db.Database.Connection.Open();

                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandType = CommandType.Text;

                    // Load Value Lists
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "CTE.dbo.portal_impersonate_fill";
                    cmd.Parameters.Add(new SqlParameter("@impersonator", impersonator));
                    cmd.Parameters.Add(new SqlParameter("@portal", portal));

                    string value_list_name;
                    string null_value_name;
                    string input_name;
                    string input_value;

                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {

                            input_value = reader[0].ToString();
                            input_name = reader[1].ToString();

                            // Create field string
                            new_values.Add(new SelectListItem { Text = input_name, Value = input_value });

                        }

                    }

                }
                catch (Exception e)
                {
                    //Redirect to an error page
                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }

            return Json(new SelectList(new_values, "Value", "Text"), JsonRequestBehavior.AllowGet);

        }
    }

}
