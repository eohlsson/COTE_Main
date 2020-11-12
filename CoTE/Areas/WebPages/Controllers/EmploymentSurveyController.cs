using CoTE.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CoTE.Areas.WebPages.Controllers
{
    public class EmploymentSurveyController : Controller
    {
        //
        // GET: /WebPages/EmploymentSurvey/

        [HttpGet]
        public ActionResult Index()
        {

            // Check the step
            string step = HttpContext.Request["step"];
            string lastname = "";
            string birthdate = "";
            lastname = HttpContext.Request["lastname"];
            birthdate = HttpContext.Request["birthdate"];
            string survey_id = HttpContext.Request["survey_id"];
            string url = "";

            //if the step is 0 get the net_id from the view, otherwise call the stored procedure and go to the URL returned
            if (step == null || step == "0")
            {
                return View();
            }
            else
            {
                // Call the stored procedure
                using (var db = new CoTEDB())
                {
                    db.Database.Initialize(force: false);

                    // Create a SQL command to execute the sproc
                    var cmd = db.Database.Connection.CreateCommand();

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "cte.dbo.get_followup_url_lname_bday";
                    cmd.Parameters.Add(new SqlParameter("@survey_id", survey_id));
                    cmd.Parameters.Add(new SqlParameter("@step", step));
                    cmd.Parameters.Add(new SqlParameter("@lname", lastname));
                    cmd.Parameters.Add(new SqlParameter("@bday", birthdate));

                    try
                    {

                        db.Database.Connection.Open();
                        // Run the sproc 
                        var reader = cmd.ExecuteReader();

                        // Read Authorized from the first result set
                        var urls = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<PortalRedirect>(reader);

                        foreach (var followup_url in urls)
                        {
                            url = followup_url.URL;
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


                    // Return the new URL
                    //url = "http://www.cote.illinois.edu";
                    return Redirect(url);
                }

            }
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            string lastname = "";
            string birthdate = "";
            string url = "";

            // Get NetID from FormCollection
            lastname = form["lastname"];
            birthdate = form["birthdate"];

            // Call stored procedure
            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "cte.dbo.get_followup_url_lname_bday";
                cmd.Parameters.Add(new SqlParameter("@survey_id", -1));
                cmd.Parameters.Add(new SqlParameter("@step", 2));
                cmd.Parameters.Add(new SqlParameter("@lname", lastname));
                cmd.Parameters.Add(new SqlParameter("@bday", birthdate));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var urls = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<PortalRedirect>(reader);

                    foreach (var followup_url in urls)
                    {
                        url = followup_url.URL;
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

                // Redirect to URL from stored procedure
                return Redirect(url);

            }
 
        }

        [HttpGet]
        public ActionResult Year1_5()
        {

            // Check the step
            string step = HttpContext.Request["step"];
            string iein = "";
            iein = HttpContext.Request["iein"];
            string survey_id = HttpContext.Request["survey_id"];
            string url = "";

            //if the step is 0 get the net_id from the view, otherwise call the stored procedure and go to the URL returned
            if (step == null || step == "0")
            {
                return View();
            }
            else
            {
                // Call the stored procedure
                using (var db = new CoTEDB())
                {
                    db.Database.Initialize(force: false);

                    // Create a SQL command to execute the sproc
                    var cmd = db.Database.Connection.CreateCommand();

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "cte.dbo.get_followup_year1_5_url";
                    cmd.Parameters.Add(new SqlParameter("@survey_id", survey_id));
                    cmd.Parameters.Add(new SqlParameter("@step", step));
                    cmd.Parameters.Add(new SqlParameter("@iein", iein));

                    try
                    {

                        db.Database.Connection.Open();
                        // Run the sproc 
                        var reader = cmd.ExecuteReader();

                        // Read Authorized from the first result set
                        var urls = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<PortalRedirect>(reader);

                        foreach (var followup_url in urls)
                        {
                            url = followup_url.URL;
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


                    // Return the new URL
                    //url = "http://www.cote.illinois.edu";
                    return Redirect(url);
                }

            }
        }

        [HttpPost]
        public ActionResult Year1_5 (FormCollection form)
        {
            string iein = "";
            string url = "";

            // Get NetID from FormCollection
            iein = form["iein"];

            // Call stored procedure
            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "cte.dbo.get_followup_year1_5_url";
                cmd.Parameters.Add(new SqlParameter("@survey_id", -1));
                cmd.Parameters.Add(new SqlParameter("@step", 2));
                cmd.Parameters.Add(new SqlParameter("@iein", iein));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var urls = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<PortalRedirect>(reader);

                    foreach (var followup_url in urls)
                    {
                        url = followup_url.URL;
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

                // Redirect to URL from stored procedure
                return Redirect(url);

            }

        }
        [HttpGet]
        public ActionResult Year2()
        {

            // Check the step
            string step = HttpContext.Request["step"];
            string iein = "";
            iein = HttpContext.Request["iein"];
            string survey_id = HttpContext.Request["survey_id"];
            string url = "";

            //if the step is 0 get the net_id from the view, otherwise call the stored procedure and go to the URL returned
            if (step == null || step == "0")
            {
                return View();
            }
            else
            {
                // Call the stored procedure
                using (var db = new CoTEDB())
                {
                    db.Database.Initialize(force: false);

                    // Create a SQL command to execute the sproc
                    var cmd = db.Database.Connection.CreateCommand();

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "cte.dbo.get_followup_year2_url";
                    cmd.Parameters.Add(new SqlParameter("@survey_id", survey_id));
                    cmd.Parameters.Add(new SqlParameter("@step", step));
                    cmd.Parameters.Add(new SqlParameter("@iein", iein));

                    try
                    {

                        db.Database.Connection.Open();
                        // Run the sproc 
                        var reader = cmd.ExecuteReader();

                        // Read Authorized from the first result set
                        var urls = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<PortalRedirect>(reader);

                        foreach (var followup_url in urls)
                        {
                            url = followup_url.URL;
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


                    // Return the new URL
                    //url = "http://www.cote.illinois.edu";
                    return Redirect(url);
                }

            }
        }

        [HttpPost]
        public ActionResult Year2(FormCollection form)
        {
            string iein = "";
            string url = "";

            // Get NetID from FormCollection
            iein = form["iein"];

            // Call stored procedure
            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "cte.dbo.get_followup_year2_url";
                cmd.Parameters.Add(new SqlParameter("@survey_id", -1));
                cmd.Parameters.Add(new SqlParameter("@step", 2));
                cmd.Parameters.Add(new SqlParameter("@iein", iein));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var urls = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<PortalRedirect>(reader);

                    foreach (var followup_url in urls)
                    {
                        url = followup_url.URL;
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

                // Redirect to URL from stored procedure
                return Redirect(url);

            }

        }
    }
}
