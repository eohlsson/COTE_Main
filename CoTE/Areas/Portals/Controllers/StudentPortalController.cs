using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoTE.Areas.Portals.Models;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Data;
using System.Web.Security; 

namespace CoTE.Areas.Portals.Controllers
{
    public class StudentPortalController : Controller
    {
        //
        // GET: /Portals/StudentPortal/

        [Authorize]
        public ActionResult Index()
        {
            bool beta = false;

            using (var db = new StudentPortalContext())
            {

                // Prevent people from using the old version
                //return Redirect("~/portals/studentportal.aspx/checklist");

                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string username = cookie.Value;

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[get_portal_Student]";
                cmd.Parameters.Add(new SqlParameter("@netid", username));
                //cmd.Parameters.Add(new SqlParameter("@netIDforAdmin", null));
  
                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var authorized = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<StudentAuthorized>(reader);

                    List<StudentAuthorized> authorizedData = authorized.ToList();

                    // Check to see if the beta flag is set
                    beta = authorizedData[0].beta;

                    // Skip deficiency results
                    reader.NextResult();
                    reader.NextResult();
                    reader.NextResult();
                    reader.NextResult();
                    reader.NextResult();
                    reader.NextResult();

                    // Deficiency Results
                    reader.NextResult();
                    var deficiency = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<StudentDeficiency>(reader);


                    List<StudentDeficiency> deficiencyData = deficiency.ToList();

                    // Move to second result set and read profile
                    reader.NextResult();
                    var profile = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<StudentProfile>(reader);


                    List<StudentProfile> profileData = profile.ToList();

                    // CAPS results
                    reader.NextResult();
                    var cap = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<StudentCAP>(reader);


                    List<StudentCAP> CAPData = cap.ToList();



                    // Move to third result set and read links
                    reader.NextResult();
                    var links = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<StudentLinks>(reader);

                    List<StudentLinks> linksData = links.ToList();

                    StudentData StudentModel = new StudentData(authorizedData, deficiencyData, profileData, CAPData, linksData);

                    List<StudentData> viewModelList = new List<StudentData>();
                    viewModelList.Add(StudentModel);

                    // If the beta flag is set go to the new portal page
                    if (beta)
                    {
                        return Redirect("~/portals/studentportal.aspx/checklist");
                    }
                    else
                    {
                        return View(viewModelList);
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

            return View();

        }

        [Authorize]
        public ActionResult Checklist()
        {

            using (var db = new StudentPortalContext())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string username = cookie.Value;

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[get_portal_student_checklist_test]";
                cmd.Parameters.Add(new SqlParameter("@netid", username));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var authorized = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<StudentAuthorized>(reader);

                    List<StudentAuthorized> authorizedData = authorized.ToList();

                    // Move to second result set and read header
                    reader.NextResult();
                    var header = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<StudentChecklistHeader>(reader);


                    List<StudentChecklistHeader> headerData = header.ToList();
                    
                    // Move to third result set and read profile
                    reader.NextResult();
                    var profile = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<StudentChecklistProfile>(reader);


                    List<StudentChecklistProfile> profileData = profile.ToList();

                    reader.NextResult();

                    // Section results

                    List<StudentChecklistSection> checklistData = new List<StudentChecklistSection>();

                    string dataset;
                    string section_name;
                    string level;
                    string met_unmet;
                    string met_text;
                    string unmet_text;
                    string required;

                    StudentChecklistSection checklist;
                    var checklists = new List<StudentChecklistSection> { };
                    
                    while (reader.HasRows)
                    {

                        while (reader.Read())
                        {
                            dataset = reader[0].ToString();
                            section_name = reader[1].ToString();
                            level = reader[2].ToString();
                            met_unmet = reader[3].ToString();
                            met_text = reader[4].ToString();
                            unmet_text = reader[5].ToString();
                            required = reader[6].ToString();

                            checklist = new StudentChecklistSection { dataset = dataset, section_name = section_name, level = level, met_unmet = met_unmet, met_text = met_text, unmet_text = unmet_text, required = required };
                            checklists.Add(checklist);
                        }

                        reader.NextResult();
                    }

                    StudentChecklistData StudentModel = new StudentChecklistData(authorizedData, headerData, profileData, checklists);

                    List<StudentChecklistData> viewModelList = new List<StudentChecklistData>();
                    viewModelList.Add(StudentModel);
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

        [Authorize]
        public ActionResult beta_set()
        {
            // Get username from cookie
            var cookie = HttpContext.Request.Cookies["authenticated_user"];
            string username = cookie.Value;

            using (var db = new StudentPortalContext())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[beta_set]";
                cmd.Parameters.Add(new SqlParameter("@portal", "student"));
                cmd.Parameters.Add(new SqlParameter("@username", username));

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

            return Redirect("/dotnet/portals/studentportal.aspx/checklist");
        }

    }
}
