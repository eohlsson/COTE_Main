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
    public class SupervisorPortalController : Controller
    {
        //
        // GET: /Portals/SupervisorPortal/

        [Authorize]
        public ActionResult Index()
        {

            using (var db = new SupervisorPortalContext())
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
                cmd.CommandText = "[dbo].[get_portal_supervisor]";
                cmd.Parameters.Add(new SqlParameter("@netid", username));
  
                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var authorized = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Authorized>(reader);

                    List<Authorized> authorizedData = authorized.ToList();

                    // Move to second result set and read profile
                    reader.NextResult();
                    var profile = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<SupervisorProfile>(reader);


                    List<SupervisorProfile> profileData = profile.ToList();

                    // Move to third result set and read links
                    reader.NextResult();
                    var links = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<SupervisorLinks>(reader);

                    List<SupervisorLinks> linksData = links.ToList();

                    SupervisorData SupervisorModel = new SupervisorData(authorizedData, profileData, linksData);

                    List<SupervisorData> viewModelList = new List<SupervisorData>();
                    viewModelList.Add(SupervisorModel);
                    return View(viewModelList); 
                
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
                cmd.CommandText = "[dbo].[get_portal_supervisor_checklist]";
                cmd.Parameters.Add(new SqlParameter("@username", username));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var authorized = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Authorized>(reader);

                    List<Authorized> authorizedData = authorized.ToList();

                    // Move to second result set and read header
                    reader.NextResult();
                    var header = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<SupervisorChecklistHeader>(reader);


                    List<SupervisorChecklistHeader> headerData = header.ToList();

                    // Move to third result set and read profile
                    reader.NextResult();
                    var profile = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<SupervisorChecklistProfile>(reader);


                    List<SupervisorChecklistProfile> profileData = profile.ToList();

                    reader.NextResult();

                    // Section results

                    List<SupervisorChecklistSection> checklistData = new List<SupervisorChecklistSection>();

                    string dataset;
                    string section_name;
                    string level;
                    string met_unmet;
                    string met_text;
                    string unmet_text;
                    string required;

                    SupervisorChecklistSection checklist;
                    var checklists = new List<SupervisorChecklistSection> { };

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

                            checklist = new SupervisorChecklistSection { dataset = dataset, section_name = section_name, level = level, met_unmet = met_unmet, met_text = met_text, unmet_text = unmet_text, required = required };
                            checklists.Add(checklist);
                        }

                        reader.NextResult();
                    }

                    SupervisorChecklistData SupervisorModel = new SupervisorChecklistData(authorizedData, headerData, profileData, checklists);

                    List<SupervisorChecklistData> viewModelList = new List<SupervisorChecklistData>();
                    viewModelList.Add(SupervisorModel);
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

    }
}
