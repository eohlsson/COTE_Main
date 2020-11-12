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
    public class CoopPortalController : Controller
    {
        //
        // GET: /Portals/CoopPortal/


        [Authorize]
        public ActionResult Index()
        {

            using (var db = new CoopPortalContext())
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
                cmd.CommandText = "[dbo].[get_portal_coop]";
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
                        .Translate<CoopAuthorized>(reader);

                    List<CoopAuthorized> authorizedData = authorized.ToList();

                    // Move to second result set and read profile
                    reader.NextResult();
                    var profile = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<CoopProfile>(reader);


                    List<CoopProfile> profileData = profile.ToList();

                    // Move to third result set and read links
                    reader.NextResult();
                    var links = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<CoopLinks>(reader);

                    List<CoopLinks> linksData = links.ToList();

                    // Move to fourth result set and skip
                    reader.NextResult();

                    // Move to third result set and read links
                    reader.NextResult();
                    var agreement = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<CoopAgreement>(reader);

                    List<CoopAgreement> agreementData = agreement.ToList();

                    CoopData CoopModel = new CoopData(authorizedData, profileData, linksData, agreementData);

                    List<CoopData> viewModelList = new List<CoopData>();
                    viewModelList.Add(CoopModel);
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
        public ActionResult EOC()
        {
            string waiver_id = Request.QueryString["record_id"];

            using (var db = new CoopPortalContext())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[portal_coop_eoc]";
                cmd.Parameters.Add(new SqlParameter("@waiver_id", waiver_id));
                cmd.Parameters.Add(new SqlParameter("@netid", HttpContext.User.Identity.Name));
 
                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var authorized = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<CoopAuthorized>(reader);

                    List<CoopAuthorized> authorizedData = authorized.ToList();

                    // Move to second result set and read coop data
                    reader.NextResult();
                    var coop = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<EOC_COOP>(reader);


                    List<EOC_COOP> EOCCoopData = coop.ToList();

                    // Move to third result set and read student data
                    reader.NextResult();
                    var students = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<EOC_STUDENT>(reader);

                    List<EOC_STUDENT> EOCStudentData = students.ToList();

                    EOC_Data EOCModel = new EOC_Data(authorizedData, EOCCoopData, EOCStudentData);

                    List<EOC_Data> viewModelList = new List<EOC_Data>();
                    viewModelList.Add(EOCModel);
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
                cmd.CommandText = "[dbo].[get_portal_coop_checklist]";
                cmd.Parameters.Add(new SqlParameter("@username", username));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var authorized = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<CoopAuthorized>(reader);

                    List<CoopAuthorized> authorizedData = authorized.ToList();

                    // Move to second result set and read header
                    reader.NextResult();
                    var header = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<CoopChecklistHeader>(reader);


                    List<CoopChecklistHeader> headerData = header.ToList();

                    // Move to third result set and read profile
                    reader.NextResult();
                    var profile = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<CoopChecklistProfile>(reader);


                    List<CoopChecklistProfile> profileData = profile.ToList();

                    reader.NextResult();

                    // Section results

                    List<CoopChecklistSection> checklistData = new List<CoopChecklistSection>();

                    string dataset;
                    string section_name;
                    string level;
                    string met_unmet;
                    string met_text;
                    string unmet_text;
                    string required;

                    CoopChecklistSection checklist;
                    var checklists = new List<CoopChecklistSection> { };

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

                            checklist = new CoopChecklistSection { dataset = dataset, section_name = section_name, level = level, met_unmet = met_unmet, met_text = met_text, unmet_text = unmet_text, required = required };
                            checklists.Add(checklist);
                        }

                        reader.NextResult();
                    }

                    CoopChecklistData CoopModel = new CoopChecklistData(authorizedData, headerData, profileData, checklists);

                    List<CoopChecklistData> viewModelList = new List<CoopChecklistData>();
                    viewModelList.Add(CoopModel);
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
                cmd.Parameters.Add(new SqlParameter("@portal", "coop"));
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

            return Redirect("/dotnet/portals/coopportal.aspx/checklist");
        }

        [Authorize]
        public ActionResult PDU()
        {
            return View();
        }

        [Authorize]
        public ActionResult EOC_PDU()
        {
            string pdu_id = Request.QueryString["record_id"];


            // Get username from cookie
            var cookie = HttpContext.Request.Cookies["authenticated_user"];
            string username = cookie.Value;
            
            using (var db = new CoopPortalContext())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[portal_coop_eoc_pdu]";
                cmd.Parameters.Add(new SqlParameter("@pdu_id", pdu_id));
                cmd.Parameters.Add(new SqlParameter("@netid", username));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var authorized = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<CoopAuthorized>(reader);

                    List<CoopAuthorized> authorizedData = authorized.ToList();

                    // Move to second result set and read coop data
                    reader.NextResult();
                    var coop = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<EOC_PDU>(reader);


                    List<EOC_PDU> EOCCoopData = coop.ToList();


                    EOC_PDU_Data EOC_PDU_Model = new EOC_PDU_Data(authorizedData, EOCCoopData);

                    List<EOC_PDU_Data> viewModelList = new List<EOC_PDU_Data>();
                    viewModelList.Add(EOC_PDU_Model);
                    return View(viewModelList);

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
