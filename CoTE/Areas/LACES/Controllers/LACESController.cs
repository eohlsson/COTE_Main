using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoTE.Areas.LACES.Models;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Data;
using System.Web.Security;
using System.Data.Entity; 

namespace CoTE.Areas.LACES.Controllers
{
    public class LACESController : Controller
    {

        private LACES_Context db = new LACES_Context();

        //
        // GET: /LACES/LACES/

        [Authorize]
        public ActionResult Index()
        {
            Database.SetInitializer<LACES_Context>(null);

            using (var db = new LACES_Context())
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
                cmd.CommandText = "[dbo].[get_portal_LACES]";
                cmd.Parameters.Add(new SqlParameter("@netid", username));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var authorized = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<LACES_Authorized>(reader);

                    List<LACES_Authorized> authorizedData = authorized.ToList();

                    // Move to third result set and read links
                    reader.NextResult();
                    var links = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<LACES_Links>(reader);

                    List<LACES_Links> linksData = links.ToList();

                    LACES_Data LACES_Model = new LACES_Data(authorizedData, linksData);

                    List<LACES_Data> viewModelList = new List<LACES_Data>();
                    viewModelList.Add(LACES_Model);
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
