using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoTE.Areas.Portals.Models;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Configuration;
using System.Web.Helpers;
using FormProcessor.Models;
using System.Net.Http.Headers;
using System.Data.Entity.Infrastructure;

namespace CoTE.Areas.Portals.Controllers
{

    public class Logger
    {
        public static void Log(string logInfo, string username, Danielson Danielson)
        {
            Console.WriteLine(logInfo);

            if (logInfo.Contains("UPDATE"))
            {
                String ConnString = ConfigurationManager.ConnectionStrings["UserDb"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        string log_sql = "";
                        string danielson_log = "";

                        for (int x = 0; x < Danielson.GetType().GetProperties().Count(); x++)
                        {
                            danielson_log = danielson_log + "|" + Danielson.GetType().GetProperties()[x].Name + ":" + Danielson.GetType().GetProperties()[x].GetValue(Danielson);
                        }

                        log_sql = "INSERT INTO [LOGS].[dbo].[FRM_PROCESSOR_LOG] ([form_id],[record_id],[form_action],[error],[client_ip],[url],[sql_transaction],[description],[created_date],[created_by])" +
                                " VALUES (" +
                                "336,'" + Danielson.cte_stu_eval_id + "', 'Update', " + "'', '', '', '" + danielson_log.ToString().Replace("'", "''") + "','','" + DateTime.Now + "', '" + username + "')";
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.CommandText = log_sql;
                        var update_log = cmd.ExecuteReader();
                        update_log.Close();
                    }
                }
            }
        }
    }

    public class EvaluationsController : Controller
    {

        private EvaluationsContext db = new EvaluationsContext();

        //
        // GET: /Portals/Evaluations/

        public ActionResult Index()
        {
            // Send people to home page, because they shouldn't be going here
            Response.Redirect("http://www.cote.illinois.edu");
            return View();
        }

        // GET: /Danielson
        public ActionResult Danielson(string username, int FormID, int RecordID, string form_action)
        {
            string errormessage = "";
            bool authorized = false;
            string referrer = "";

            if (HttpContext.Request.UrlReferrer == null)
            {
                Session["referrer"] = "";
            }
            else
            {
                referrer = (string)HttpContext.Request.UrlReferrer.ToString() ?? "";
                Session["referrer"] = referrer;
                
            }

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {

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
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            // FRM_PROC.dbo.FRM_GetParamData_rs 
            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Check for Insert permissions
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FRM_PROC.dbo.FRM_GetPermittedRecordID";
                cmd.Parameters.Add(new SqlParameter("@user_name", username));
                cmd.Parameters.Add(new SqlParameter("@table_name", "CTE.DBO.V_CTE_STU_EVAL_DANIELSON"));
                cmd.Parameters.Add(new SqlParameter("@Return_field", "cte_stu_eval_id"));
                cmd.Parameters.Add(new SqlParameter("@where_clause", "CTE.DBO.V_CTE_STU_EVAL_DANIELSON.cte_stu_eval_id = " + RecordID));

                //cmd.CommandType = CommandType.Text;
                //cmd.CommandText = "SELECT frm_proc.DBO.fn_getPermissionUpdate_v5('" + username + "', " + FormID + ") as Frm_Level_Permissions";

                try
                {

                    db.Database.Connection.Open();

                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var insert_permissions = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Record_Level_Permissions>(reader);

                    foreach (var permission in insert_permissions)
                    {
                        ViewBag.insert_permissions = permission.Update.ToString();
                        if (permission.Update.ToString() == "True")
                        {
                            authorized = true;
                        }
                        if (permission.View.ToString() == "True")
                        {
                            authorized = true;
                            if (form_action == "View")
                            {
                                ViewBag.read_only = "True";
                            }
                        }

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
            }

            if (authorized)
            {

                ViewBag.Username = username;

                Database.SetInitializer<EvaluationsContext>(null);
                Danielson Danielson = db.Danielsons.Find(RecordID);
                if (Danielson != null)  // If record exists send data to view
                {
                    ViewBag.Action = "Edit";
                    //Check to see if they have permissions to edit the record
                    return View(Danielson);
                }
                else // If no record exists send nothing to view
                {
                    ViewBag.Action = "Add";
                    return View();
                }
            }
            else // Not authorized send to access denied
            {
                return Redirect("/access_denied.html");
            }
            
            //Redirect to an error page
            Elmah.ErrorSignal.FromCurrentContext().Raise(new NotImplementedException("Error in Danielson Evaluation " + errormessage));
            return Redirect("~/FormProcessor/Error.aspx/HttpError");
 
            // Old error page
            //Response.Redirect("/dotnet/portals/evaluations.aspx/ErrorPage");
            //return View();

        }

        // POST: /Danielson
        [HttpPost, ValidateInput(false)]
        public ActionResult Danielson(Danielson Danielson, string Action, string Username)
        {

            string referrer = "";

            if (Session["referrer"] != null)
            {
                referrer = Session["referrer"].ToString();
            }

            // If referrer is blank redirect to home page
            if (referrer == "")
            {
                referrer = "/dotnet/account.aspx/loginsuccess";
            }

            if (Action == "Add") //Adding
            {
                using (EvaluationsContext db = new EvaluationsContext())
                {

                    if (ModelState.IsValid)
                    {
                        db.Danielsons.Add(Danielson);

                        db.SaveChanges();

                        return Redirect(referrer);
                    }

                }
            }
            else // Edit
            {
                if (ModelState.IsValid)
                {
                    using (EvaluationsContext db = new EvaluationsContext())
                    {
                        db.Database.Log = logInfo => Logger.Log(logInfo, Username, Danielson);
                        db.Entry(Danielson).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                    return Redirect(referrer);
                }
 
            }

            ViewBag.Username = Username;
            ViewBag.Action = Action;

            return View(Danielson);
        }


        public ActionResult ErrorPage()
        {
            return View();
        }

    }

}    

