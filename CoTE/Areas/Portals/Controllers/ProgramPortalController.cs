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
    public class ProgramPortalController : Controller
    {
        //
        // GET: /Portals/ProgramPortal/

        [Authorize]
        public ActionResult Index()
        {

            using (var db = new ProgramPortalContext())
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
                cmd.CommandText = "[dbo].[get_portal_Program]";
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
                        .Translate<ProgramAuthorized>(reader);

                    List<ProgramAuthorized> authorizedData = authorized.ToList();

                    // Move to second result set and get deficiencies
                    reader.NextResult();
                    var deficiency = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<ProgramDeficiency>(reader);

                    List<ProgramDeficiency> deficiencyData = deficiency.ToList();
                    
                    // Move to third result set and read profile
                    reader.NextResult();
                    var profile = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<ProgramProfile>(reader);


                    List<ProgramProfile> profileData = profile.ToList();

                    // Move to fourth result set and read links
                    reader.NextResult();
                    var links = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<ProgramLinks>(reader);

                    List<ProgramLinks> linksData = links.ToList();

                    ProgramData ProgramModel = new ProgramData(authorizedData, deficiencyData, profileData, linksData);

                    List<ProgramData> viewModelList = new List<ProgramData>();
                    viewModelList.Add(ProgramModel);
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
