using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using FormProcessor.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Collections.Specialized;


namespace CoTE.Areas.FormProcessor.Controllers
{
    public class IndexController : Controller
    {

        bool debug = true;
        public string username = "";

        public List<Form_Data> viewModelList { get; set; }

        //
        // GET: /Index/

        public ActionResult Index_Old()
        {

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                // Get cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string username = "";

                if (cookie.Values.Count > 0)
                {
                    username = cookie.Value;
                }

                // Check if Authenticated_user is set if so use it
                if (username != "")
                {
                    Session["Username"] = username;
                }
                else
                {
                    Session["Username"] = HttpContext.User.Identity.Name;
                }
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            string param_form_id = HttpContext.Request["form_id"];
            string param_index_id = HttpContext.Request["index_id"];
            string param_param1 = HttpContext.Request["param1"];
            string param_param2 = HttpContext.Request["param2"];
            string page = HttpContext.Request["page"];

            // FRM_PROC.dbo.FRM_GetParamData_rs 
            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Check for Insert permissions
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT frm_proc.DBO.fn_getPermissionInsert_v5('" + Session["Username"].ToString() + "', " + param_form_id + ") as Frm_Level_Permissions";
                  
              try
                {

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "Start Permissions Check on Index: " + cmd.CommandText); }

                    db.Database.Connection.Open();

                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var insert_permissions = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Level_Insert_Permissions>(reader);

                    foreach (var permission in insert_permissions)
                    {
                        ViewBag.insert_permissions = permission.Frm_Level_Permissions.ToString();
                    }

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "End Permissions Check on Index: " + cmd.CommandText); }

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

                // Create a SQL command to get the Form data
                cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FRM_PROC.dbo.FRM_GetParamData_rs";
                cmd.Parameters.Add(new SqlParameter("@form_id", param_form_id));
                cmd.Parameters.Add(new SqlParameter("@username", Session["Username"].ToString()));
                cmd.Parameters.Add(new SqlParameter("@index", "true"));
                cmd.Parameters.Add(new SqlParameter("@index_id", param_index_id));
                cmd.Parameters.Add(new SqlParameter("@detail", "false"));

                if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "Start: " + cmd.CommandText + " @form_id=" + param_form_id + ", @username=" + Session["Username"].ToString() + ", @index='True', @index_id=" + param_index_id + ", @detail='False'"); }

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var permissions = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Level_Permissions>(reader);

                    List<Form_Level_Permissions> permissionsData = permissions.ToList();

                    // Move to second result set and read profile
                    reader.NextResult();
                    var form = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Form>(reader);


                    List<Form_Form> formData = form.ToList();

                    // Move to third result set and read links
                    reader.NextResult();
                    var index = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Index>(reader);

                    List<Form_Index> indexData = index.ToList();

                    // Trap for view that doesn't exist
                    if (indexData.Where(index_check => index_check.index_id.ToString() == param_index_id.ToString()).Count() == 0)
                    {
                        //Redirect to an error page
                        throw new HttpException(404, "View not found");
                    }

                    // Move to fourth result set and read links
                    reader.NextResult();
                    var relation = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Relations>(reader);

                    List<Form_Relations> relationData = relation.ToList();

                    // Move to fifth result set and read links
                    reader.NextResult();
                    var question = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Question>(reader);

                    List<Form_Question> questionData = question.ToList();

                    // Move to sixth result set and read links
                    reader.NextResult();
                    var section = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Section>(reader);

                    List<Form_Section> sectionData = section.ToList();

                    // Move to seventh result set and read links
                    reader.NextResult();
                    var wrapper = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Page_Wrapper>(reader);

                    List<Form_Page_Wrapper> wrapperData = wrapper.ToList();

                    // Move to eighth result set and read triggers
                    reader.NextResult();
                    var trigger = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Trigger>(reader);

                    List<Form_Trigger> triggerData = trigger.ToList();

                    // Skip ninth recordset
                    reader.NextResult();

                    // Skip tenth recordset
                    reader.NextResult();

                    // Move to eleventh result set and get footer data
                    reader.NextResult();
                    var footer = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Footer>(reader);

                    List<Form_Footer> footerData = footer.ToList();

                    reader.Close();
                    cmd.Parameters.Clear();

                    // Load Value Lists
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "FRM_PROC.dbo.FRM_GetValueListData_rs";
                    cmd.Parameters.Add(new SqlParameter("@form_id", param_form_id));
                    cmd.Parameters.Add(new SqlParameter("@username", Session["Username"].ToString()));
                    cmd.Parameters.Add(new SqlParameter("@TruncateSearchList", "1"));
                    cmd.Parameters.Add(new SqlParameter("@record_id", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@question_id", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@search_str", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@index", "true"));
                    cmd.Parameters.Add(new SqlParameter("@index_id", param_index_id));

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "Get ValueList Data: Start:" + cmd.CommandText + " @form_id=" + param_form_id + ", @username='" + Session["Username"].ToString() + "', @TruncateSearchList=0, @record_id=" + ViewBag.record_id + ", @question_id=null, @search_str=null, @index=false, @index_id=null"); }

                    int question_id = 0;
                    string value_list_name;
                    string null_value_name;
                    string input_name;
                    string input_value;
                    string drill_down_parent_value;

                    Form_Valuelists value_list;
                    var value_lists = new List<Form_Valuelists> { };

                    using (var reader2 = cmd.ExecuteReader())
                    {

                        while (reader2.Read())
                        {
                            Int32.TryParse(reader2[0].ToString(), out question_id);
                            value_list_name = reader2[1].ToString();
                            null_value_name = reader2[2].ToString();

                            //Add null value item
                            value_list = new Form_Valuelists { question_id = question_id, value_list_name = value_list_name, null_value_name = null_value_name, input_value = "", input_name = null_value_name, drill_down_parent_value = "" };
                            value_lists.Add(value_list);

                            reader2.NextResult();

                            while (reader2.Read())
                            {
                                input_value = reader2[0].ToString();
                                input_name = reader2[1].ToString();

                                // Not all value list have drill_down_parent_value
                                if (reader2.FieldCount > 2)
                                {
                                    drill_down_parent_value = reader2[2].ToString();
                                }
                                else
                                {
                                    drill_down_parent_value = "";
                                }

                                value_list = new Form_Valuelists { question_id = question_id, value_list_name = value_list_name, null_value_name = null_value_name, input_value = input_value, input_name = input_name, drill_down_parent_value = drill_down_parent_value };
                                value_lists.Add(value_list);
                            }

                            reader2.NextResult();

                        }
                    }

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "Get ValueList Data: End:" + cmd.CommandText + " @form_id=" + ViewBag.form_id + ", @username='" + Session["Username"].ToString() + "', @TruncateSearchList=0, @record_id=" + ViewBag.record_id + ", @question_id=null, @search_str=null, @index=false, @index_id=null"); }

                    Form_Data IndexModel = new Form_Data(permissionsData, formData, indexData, relationData, questionData, sectionData, wrapperData, triggerData, footerData, value_lists);

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "End: " + cmd.CommandText + " @form_id=" + param_form_id + ", @username=" + Session["Username"].ToString() + ", @index='True', @index_id=" + param_index_id + ", @detail='False'"); }

                    viewModelList = new List<Form_Data>();
                    viewModelList.Add(IndexModel);
                    
                    // Create the filter
                    string filter_sql = "";
                    string cookie_name = "index_filter_" + param_form_id.ToString();
                    HttpCookie myCookie = new HttpCookie(cookie_name);

                    if (HttpContext.Request["showall"] == "Show All" || HttpContext.Request["filter"] != "yes")
                    {
                        if (Response.Cookies[cookie_name] != null)
                        {
                            myCookie.Expires = DateTime.Now.AddDays(-1d);
                            Request.Cookies.Remove(cookie_name);
                            Response.Cookies.Add(myCookie);
                        }
                    }

                    if (HttpContext.Request["filter"] == "yes" && Request.Form.Keys.Count > 0 && HttpContext.Request["showall"] != "Show All")
                    {

                        // Store filter in session variable
                        if (Response.Cookies[cookie_name] != null)
                        {
                            myCookie.Expires = DateTime.Now.AddDays(-1d);
                            Request.Cookies.Remove(cookie_name);
                            Response.Cookies.Add(myCookie);
                        }
                        myCookie.Expires = DateTime.Now.AddDays(1d);
                        Response.Cookies.Add(myCookie);

                        ViewBag.filter_on = "yes";
                        for (int x = 0; x < Request.Form.Keys.Count - 1; x++)
                        {
                            if (Request.Form[x].Length > 0 && Request.Form.Keys[x].ToString() != "filter_sql")
                            {
                                if (Request.Form[x].Replace("'", "''").All(Char.IsDigit))
                                {
                                    filter_sql += " and " + Request.Form.Keys[x].ToString().Replace("'", "''") + " = '" + Request.Form[x].Replace("'", "''") + "'";
                                }
                                else
                                {
                                    filter_sql += " and " + Request.Form.Keys[x].ToString().Replace("'", "''") + " LIKE '%" + Request.Form[x].Replace("'", "''") + "%'";
                                }

                                ViewData[Request.Form.Keys[x].ToString().Replace("'", "''")] = Request.Form[x].Replace("'", "''");
                                myCookie[Request.Form.Keys[x].ToString().Replace("'", "''")] = Request.Form[x].Replace("'", "''");
                            }
                        }
                    }

                    if (filter_sql.Length > 0)
                    {
                        filter_sql = filter_sql.Substring(4);
                        myCookie["**sql**"] = filter_sql;
                    }

                    // Get session_id from cookie
                    var session_cookie = HttpContext.Request.Cookies["session_id"];
                    ViewBag.session_id = session_cookie.Value;
                    ViewBag.filter_sql = filter_sql;
                    ViewBag.Username = Session["Username"].ToString();
                    ViewBag.form_id = param_form_id;
                    ViewBag.index_id = param_index_id;
                    ViewBag.param1 = param_param1;
                    ViewBag.param2 = param_param2;
                    ViewBag.debug = debug;
                    ViewBag.page = page;
                    ViewBag.showall = HttpContext.Request["showall"];
                    
                    Session["viewModelList"] = viewModelList;

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

        public ActionResult TestGrid()
        {

            return View();
        }


        [HttpGet]
        public JsonResult FilterQuestion(string question_id, string question_value, string question_list, List<Form_Valuelists> completeData)
        {
            List<SelectListItem> new_values = new List<SelectListItem>();
            string field_name = "";
            int count = 0;

            List<Form_Data> ModelList = (List<Form_Data>)Session["viewModelList"];

            foreach (Form_Data h in ModelList)
            {

                // get question name and store it as first SelectListItem
                foreach (Form_Question fq in h.QuestionsDataSet.Where(questions => questions.question_id.ToString() == question_list))
                {
                    field_name = fq.data_location.ToString();
                    new_values.Add(new SelectListItem { Text = field_name, Value = "-1" });
                }

                var value_lists = new List<Form_Valuelists> { };
                value_lists = h.ValueListDataSet;

                        // Loop on Valuelist
                        foreach (Form_Valuelists item in value_lists.Where(values => values.question_id.ToString() == question_list && values.drill_down_parent_value == question_value))
                        {

                            // Set first SelectListItem to null value
                            if (count == 0)
                            {
                                new_values.Add(new SelectListItem { Text = item.null_value_name, Value = "" });
                            }

                            // Create field string
                            new_values.Add(new SelectListItem { Text = item.input_name, Value = item.input_value });

                            count++;
                        }

            }

            return Json(new SelectList(new_values, "Value", "Text"), JsonRequestBehavior.AllowGet);
 
        }

        [HttpGet]
        public JsonResult FilterSearchQuestion(string question_id, string search_value)
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

                    string record_id;
                    if (Session["record_id"] != null)
                    {
                        record_id = Session["record_id"].ToString();
                    }
                    else
                    {
                        record_id = "";
                    }

                    // Load Value Lists
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "FRM_PROC.dbo.FRM_GetValueListData_rs";
                    cmd.Parameters.Add(new SqlParameter("@form_id", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@username", Session["Username"].ToString()));
                    cmd.Parameters.Add(new SqlParameter("@TruncateSearchList", "1"));
                    cmd.Parameters.Add(new SqlParameter("@record_id", record_id));
                    cmd.Parameters.Add(new SqlParameter("@question_id", question_id));
                    cmd.Parameters.Add(new SqlParameter("@search_str", search_value));
                    cmd.Parameters.Add(new SqlParameter("@index", "false"));
                    cmd.Parameters.Add(new SqlParameter("@index_id", DBNull.Value));

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "Get ValueList Data: Start:" + cmd.CommandText + " @form_id=" + ViewBag.form_id + ", @username='" + username + "', @TruncateSearchList=0, @record_id=" + ViewBag.record_id + ", @question_id=null, @search_str=null, @index=false, @index_id=null"); }

                    string value_list_name;
                    string null_value_name;
                    string input_name;
                    string input_value;

                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            value_list_name = reader[1].ToString();
                            null_value_name = reader[2].ToString();

                            //Add null value item
                            new_values.Add(new SelectListItem { Text = null_value_name, Value = "" });

                            reader.NextResult();

                            while (reader.Read())
                            {

                                input_value = reader[0].ToString();
                                input_name = reader[1].ToString();

                                // Create field string
                                new_values.Add(new SelectListItem { Text = input_name, Value = input_value });

                            }

                            reader.NextResult();

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

        //***************************************************************************************************************************
        //*
        //*                                        TESTING
        //*
        //***************************************************************************************************************************

        public ActionResult Index()
        {
            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                // Get cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string username = "";

                if (cookie.Values.Count > 0)
                {
                    username = cookie.Value;
                }

                // Check if Authenticated_user is set if so use it
                if (username != "")
                {
                    Session["Username"] = username;
                }
                else
                {
                    Session["Username"] = HttpContext.User.Identity.Name;
                }
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            string param_form_id = HttpContext.Request["form_id"];
            string param_index_id = HttpContext.Request["index_id"];
            string param_param1 = HttpContext.Request["param1"];
            string param_param2 = HttpContext.Request["param2"];
            string page = HttpContext.Request["page"];

            // FRM_PROC.dbo.FRM_GetParamData_rs 
            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Check for Insert permissions
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT frm_proc.DBO.fn_getPermissionInsert_v5('" + Session["Username"].ToString() + "', " + param_form_id + ") as Frm_Level_Permissions";

                try
                {

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "Start Permissions Check on Index: " + cmd.CommandText); }

                    db.Database.Connection.Open();

                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var insert_permissions = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Level_Insert_Permissions>(reader);

                    foreach (var permission in insert_permissions)
                    {
                        ViewBag.insert_permissions = permission.Frm_Level_Permissions.ToString();
                    }

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "End Permissions Check on Index: " + cmd.CommandText); }

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

                // Create a SQL command to get the Form data
                cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FRM_PROC.dbo.FRM_GetParamData_rs";
                cmd.Parameters.Add(new SqlParameter("@form_id", param_form_id));
                cmd.Parameters.Add(new SqlParameter("@username", Session["Username"].ToString()));
                cmd.Parameters.Add(new SqlParameter("@index", "true"));
                cmd.Parameters.Add(new SqlParameter("@index_id", param_index_id));
                cmd.Parameters.Add(new SqlParameter("@detail", "false"));

                if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "Start: " + cmd.CommandText + " @form_id=" + param_form_id + ", @username=" + Session["Username"].ToString() + ", @index='True', @index_id=" + param_index_id + ", @detail='False'"); }

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Read Authorized from the first result set
                    var permissions = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Level_Permissions>(reader);

                    List<Form_Level_Permissions> permissionsData = permissions.ToList();

                    // Move to second result set and read profile
                    reader.NextResult();
                    var form = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Form>(reader);


                    List<Form_Form> formData = form.ToList();

                    // Move to third result set and read links
                    reader.NextResult();
                    var index = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Index>(reader);

                    List<Form_Index> indexData = index.ToList();

                    // Trap for view that doesn't exist
                    if (indexData.Where(index_check => index_check.index_id.ToString() == param_index_id.ToString()).Count() == 0)
                    {
                        //Redirect to an error page
                        throw new HttpException(404, "View not found");
                    }

                    // Move to fourth result set and read links
                    reader.NextResult();
                    var relation = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Relations>(reader);

                    List<Form_Relations> relationData = relation.ToList();

                    // Move to fifth result set and read links
                    reader.NextResult();
                    var question = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Question>(reader);

                    List<Form_Question> questionData = question.ToList();

                    // Move to sixth result set and read links
                    reader.NextResult();
                    var section = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Section>(reader);

                    List<Form_Section> sectionData = section.ToList();

                    // Move to seventh result set and read links
                    reader.NextResult();
                    var wrapper = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Page_Wrapper>(reader);

                    List<Form_Page_Wrapper> wrapperData = wrapper.ToList();

                    // Move to eighth result set and read triggers
                    reader.NextResult();
                    var trigger = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Trigger>(reader);

                    List<Form_Trigger> triggerData = trigger.ToList();

                    // Skip ninth recordset
                    reader.NextResult();

                    // Skip tenth recordset
                    reader.NextResult();

                    // Move to eleventh result set and get footer data
                    reader.NextResult();
                    var footer = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Form_Footer>(reader);

                    List<Form_Footer> footerData = footer.ToList();

                    reader.Close();
                    cmd.Parameters.Clear();

                    // Load Value Lists
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "FRM_PROC.dbo.FRM_GetValueListData_rs";
                    cmd.Parameters.Add(new SqlParameter("@form_id", param_form_id));
                    cmd.Parameters.Add(new SqlParameter("@username", Session["Username"].ToString()));
                    cmd.Parameters.Add(new SqlParameter("@TruncateSearchList", "1"));
                    cmd.Parameters.Add(new SqlParameter("@record_id", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@question_id", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@search_str", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@index", "true"));
                    cmd.Parameters.Add(new SqlParameter("@index_id", param_index_id));

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "Get ValueList Data: Start:" + cmd.CommandText + " @form_id=" + param_form_id + ", @username='" + Session["Username"].ToString() + "', @TruncateSearchList=0, @record_id=" + ViewBag.record_id + ", @question_id=null, @search_str=null, @index=false, @index_id=null"); }

                    int question_id = 0;
                    string value_list_name;
                    string null_value_name;
                    string input_name;
                    string input_value;
                    string drill_down_parent_value;

                    Form_Valuelists value_list;
                    var value_lists = new List<Form_Valuelists> { };

                    using (var reader2 = cmd.ExecuteReader())
                    {

                        while (reader2.Read())
                        {
                            Int32.TryParse(reader2[0].ToString(), out question_id);
                            value_list_name = reader2[1].ToString();
                            null_value_name = reader2[2].ToString();

                            //Add null value item
                            value_list = new Form_Valuelists { question_id = question_id, value_list_name = value_list_name, null_value_name = null_value_name, input_value = "", input_name = null_value_name, drill_down_parent_value = "" };
                            value_lists.Add(value_list);

                            reader2.NextResult();

                            while (reader2.Read())
                            {
                                input_value = reader2[0].ToString();
                                input_name = reader2[1].ToString();

                                // Not all value list have drill_down_parent_value
                                if (reader2.FieldCount > 2)
                                {
                                    drill_down_parent_value = reader2[2].ToString();
                                }
                                else
                                {
                                    drill_down_parent_value = "";
                                }

                                value_list = new Form_Valuelists { question_id = question_id, value_list_name = value_list_name, null_value_name = null_value_name, input_value = input_value, input_name = input_name, drill_down_parent_value = drill_down_parent_value };
                                value_lists.Add(value_list);
                            }

                            reader2.NextResult();

                        }
                    }

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "Get ValueList Data: End:" + cmd.CommandText + " @form_id=" + ViewBag.form_id + ", @username='" + Session["Username"].ToString() + "', @TruncateSearchList=0, @record_id=" + ViewBag.record_id + ", @question_id=null, @search_str=null, @index=false, @index_id=null"); }

                    Form_Data IndexModel = new Form_Data(permissionsData, formData, indexData, relationData, questionData, sectionData, wrapperData, triggerData, footerData, value_lists);

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "End: " + cmd.CommandText + " @form_id=" + param_form_id + ", @username=" + Session["Username"].ToString() + ", @index='True', @index_id=" + param_index_id + ", @detail='False'"); }

                    viewModelList = new List<Form_Data>();
                    viewModelList.Add(IndexModel);

                    // Create the filter
                    string filter_sql = "";
                    string cookie_name = "index_filter_" + param_form_id.ToString();
                    HttpCookie myCookie = new HttpCookie(cookie_name);

                    if (HttpContext.Request["showall"] == "Show All" || HttpContext.Request["filter"] != "yes" || Request.Form["Submit"] == "Remove Filter")
                    {
                        if (Response.Cookies[cookie_name] != null)
                        {
                            myCookie.Expires = DateTime.Now.AddDays(-1d);
                            Request.Cookies.Remove(cookie_name);
                            Response.Cookies.Add(myCookie);
                        }
                    }

                    if (HttpContext.Request["filter"] == "yes" && Request.Form.Keys.Count > 0 && HttpContext.Request["showall"] != "Show All" && Request.Form["Submit"] != "Remove Filter")
                    {

                        // Store filter in session variable
                        if (Response.Cookies[cookie_name] != null)
                        {
                            myCookie.Expires = DateTime.Now.AddDays(-1d);
                            Request.Cookies.Remove(cookie_name);
                            Response.Cookies.Add(myCookie);
                        }
                        myCookie.Expires = DateTime.Now.AddDays(1d);
                        Response.Cookies.Add(myCookie);

                        ViewBag.filter_on = "yes";
                        for (int x = 0; x < Request.Form.Keys.Count - 1; x++)
                        {
                            if (Request.Form[x].Length > 0 && Request.Form.Keys[x].ToString() != "filter_sql" && Request.Form.Keys[x].ToString() != "showall")
                            {

                                // Get question type
                                string question_type = "";
                                foreach (var filter_question in IndexModel.QuestionsDataSet.Where(filter_question => filter_question.data_location == Request.Form.Keys[x]))
                                {
                                    question_type = filter_question.input_type;
                                }

                                if (Request.Form[x].Replace("'", "''").All(Char.IsDigit) || question_type == "select")
                                {
                                    filter_sql += " and " + Request.Form.Keys[x].ToString().Replace("'", "''") + " = '" + Request.Form[x].Replace("'", "''") + "'";
                                }
                                else
                                {
                                    filter_sql += " and " + Request.Form.Keys[x].ToString().Replace("'", "''") + " LIKE '%" + Request.Form[x].Replace("'", "''") + "%'";
                                }

                                ViewData[Request.Form.Keys[x].ToString().Replace("'", "''")] = Request.Form[x].Replace("'", "''");
                                myCookie[Request.Form.Keys[x].ToString().Replace("'", "''")] = Request.Form[x].Replace("'", "''");
                            }
                        }
                    }

                    if (filter_sql.Length > 0)
                    {
                        filter_sql = filter_sql.Substring(4);
                        myCookie["**sql**"] = filter_sql;
                    }


                    // Get session_id from cookie
                    var session_cookie = HttpContext.Request.Cookies["session_id"];
                    ViewBag.session_id = session_cookie.Value;
                    ViewBag.filter_sql = filter_sql;
                    ViewBag.Username = Session["Username"].ToString();
                    ViewBag.form_id = param_form_id;
                    ViewBag.index_id = param_index_id;
                    ViewBag.param1 = param_param1;
                    ViewBag.param2 = param_param2;
                    ViewBag.debug = debug;
                    ViewBag.page = page;
                    ViewBag.showall = HttpContext.Request["showall"];

                    Session["viewModelList"] = viewModelList;

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
