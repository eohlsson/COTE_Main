using CoTE.Areas.SCE.Models;
using CoTE.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CoTE.Areas.SCE.Controllers
{
    public class IndexController : Controller
    {
        //
        // GET: /SCE/Index/

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

        [Authorize]
        public ActionResult Index()
        {
            using (var db = new SCE_Context())
            {

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string username = cookie.Value;

                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[get_portal_SCE]";
                cmd.Parameters.Add(new SqlParameter("@netid", username));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var authorized = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<SCE_Authorized>(reader);

                    List<SCE_Authorized> authorizedData = authorized.ToList();

                    // Move to third result set and read links
                    reader.NextResult();
                    var links = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<SCE_Links>(reader);

                    List<SCE_Links> linksData = links.ToList();

                    SCE_Data SCE_Model = new SCE_Data(authorizedData, linksData);

                    List<SCE_Data> viewModelList = new List<SCE_Data>();
                    viewModelList.Add(SCE_Model);
                    return View(viewModelList);

                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }

            return View();

        }

        public ActionResult approve()
        {
            string strRecord_Id = HttpContext.Request["record_id"];
            string username = "";
            string sql_statement = "";

            sql_statement = "Update cte.dbo.SCE_COOP_APPLICATION set sce_approved = '1' where coop_application_id = " + strRecord_Id;

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            using (var db = new SCE_Context())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = sql_statement;

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

            return Redirect("/dotnet/formprocessor/index.aspx/index?form_id=362&index_id=505");
        }

        public ActionResult Send_PC_Emails()
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
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            using (var db = new SCE_Context())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "cte.dbo.SCE_PC_NOTIFICATION";
                cmd.Parameters.Add(new SqlParameter("@facility_id", strRecord_Id));

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

            return View();
        }

        [HttpGet]
        public ActionResult PC_Approval(string token)
        {

            string url = "";
            string username = "";

            // Call the stored procedure
            using (var db = new SCE_Context())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "cte.dbo.SCE_GET_PC_URL";
                cmd.Parameters.Add(new SqlParameter("@token", token));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var authorized = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<SCE_Authorized>(reader);

                    foreach (var user in authorized)
                    {
                        username = user.user_name;
                    }

                    // Read URLs from the second result set
                    reader.NextResult();
                    var urls = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<PortalRedirect>(reader);

                    foreach (var approval_url in urls)
                    {
                        url = approval_url.URL;
                    }

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

                if (username != "")
                {
                    FormsAuthentication.SetAuthCookie(username, false);
                    Session["authenticated_user"] = username;
                    Response.Cookies["authenticated_user"].Value = username;
                    Response.Cookies["session_id"].Value = GenerateRandomNumber();
                    Session["index_url"] = url;

                    // Write to WEB_AUTHENTICATED_USERS table
                    db.Database.SqlQuery<StoredProcedure>("Insert into WEB_Authenticated_Sessions (username, token, start_time, ip_address, logged_out, authentication_type) values ({0}, {1}, {2}, {3}, {4}, {5})", username, "", DateTime.Now.ToString(), Request.ServerVariables["REMOTE_ADDR"], false, "MS").ToList();

                }

                // Return the new URL
                return Redirect(url);

            }
        }

    }
}
