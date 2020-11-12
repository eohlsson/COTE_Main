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
using System.Configuration;
using System.Text.RegularExpressions;
using Elmah;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;

namespace CoTE.Areas.FormProcessor.Controllers
{

    public class ElmahHandledErrorLoggerFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            // Log only handled exceptions, because all other will be caught by ELMAH anyway.
            if (context.ExceptionHandled)
                ErrorSignal.FromCurrentContext().Raise(context.Exception);
        }
    }

    public class FormController : Controller
    {

        bool debug = true;
        bool no_exec = false;

        //public variables for common fields
        public string global_UserName { get; set; }
        public string global_Param1 { get; set; }
        public string global_Param2 { get; set; }
        public string global_record_id { get; set; }
        public string global_form_id { get; set; }
        public string global_form_action { get; set; }
        public List<Form_Data> viewModelList { get; set; }

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

        static List<Form_Data> BuildViewModel(string form_id, string record_id, string calling_id, string username)
        {


            List<Form_Data> viewModelListFill = new List<Form_Data>();

            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FRM_PROC.dbo.FRM_GetParamData_rs";
                cmd.Parameters.Add(new SqlParameter("@form_id", form_id));
                cmd.Parameters.Add(new SqlParameter("@username", username));
                cmd.Parameters.Add(new SqlParameter("@index", "false"));
                cmd.Parameters.Add(new SqlParameter("@detail", "true"));
                cmd.Parameters.Add(new SqlParameter("@record_id", record_id));
                cmd.Parameters.Add(new SqlParameter("@calling_id", calling_id));

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

                    // Move to eighth result set and read links
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
                    cmd.Parameters.Add(new SqlParameter("@form_id", form_id));
                    cmd.Parameters.Add(new SqlParameter("@username", username));
                    cmd.Parameters.Add(new SqlParameter("@TruncateSearchList", "0"));
                    cmd.Parameters.Add(new SqlParameter("@record_id", record_id));
                    cmd.Parameters.Add(new SqlParameter("@question_id", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@search_str", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@index", "false"));
                    cmd.Parameters.Add(new SqlParameter("@index_id", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@calling_id", calling_id));

                    int question_id = 0;
                    string value_list_name;
                    string null_value_name;
                    string input_name;
                    string input_value;
                    string drill_down_parent_value;
                    string dynamic_sql;

                    Form_Valuelists value_list;
                    var value_lists = new List<Form_Valuelists> { };

                    using (var reader2 = cmd.ExecuteReader())
                    {

                        while (reader2.Read())
                        {
                            Int32.TryParse(reader2[0].ToString(), out question_id);
                            value_list_name = reader2[1].ToString();
                            null_value_name = reader2[2].ToString();

                            // Not all value list have dynamic_sql
                            if (reader2.FieldCount > 3)
                            {
                                dynamic_sql = reader2[3].ToString();
                                if (dynamic_sql == "")
                                {
                                    dynamic_sql = "false";

                                }
                            }
                            else
                            {
                                dynamic_sql = "false";
                            } 

                            //Add null value item
                            value_list = new Form_Valuelists { question_id = question_id, value_list_name = value_list_name, null_value_name = null_value_name, input_value = "", input_name = null_value_name, drill_down_parent_value = "", dynamic_sql = Convert.ToBoolean(dynamic_sql) };
                            value_lists.Add(value_list);

                            reader2.NextResult();

                            while (reader2.Read() && value_list_name != "") 
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

                                value_list = new Form_Valuelists { question_id = question_id, value_list_name = value_list_name, null_value_name = null_value_name, input_value = input_value, input_name = input_name, drill_down_parent_value = drill_down_parent_value, dynamic_sql = Convert.ToBoolean(dynamic_sql) };
                                value_lists.Add(value_list);
                            }

                            value_list_name = "";
                            reader2.NextResult();

                        }
                    }

                    Form_Data IndexModel = new Form_Data(permissionsData, formData, indexData, relationData, questionData, sectionData, wrapperData, triggerData, footerData, value_lists);

                    viewModelListFill.Add(IndexModel);

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
            
            return viewModelListFill;

        }

        static List<SelectListItem> BuildDynamicSQLValueList(string question_value, string question_list, string null_value_name, string username, List<SelectListItem> new_values)
        {
            //List<SelectListItem> new_values = new List<SelectListItem>();

            int count = 0;

            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FRM_PROC.dbo.FRM_GetDynamicValueList";
                cmd.Parameters.Add(new SqlParameter("@question_id", question_list));
                cmd.Parameters.Add(new SqlParameter("@value", question_value));
                cmd.Parameters.Add(new SqlParameter("@username", username));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        // Set first SelectListItem to null value
                        if (count == 0)
                        {
                            new_values.Add(new SelectListItem { Text = null_value_name, Value = "" });
                        }

                        // Create field string
                        new_values.Add(new SelectListItem { Text = reader[1].ToString(), Value = reader[0].ToString() });

                        count++;
                    }

                    reader.Close();
                    cmd.Parameters.Clear();

                    

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

            return new_values;

        }

        //
        // GET: /Form/

        public ActionResult DateTest()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Index()
        {

            // Get parameters from querystring
            string param_form_id = (string)HttpContext.Request["form_id"] ?? "";
            string param_record_id = (string)HttpContext.Request["record_id"] ?? "";
            string param_action = (string)HttpContext.Request["action"] ?? "";
            string param_subform = (string)HttpContext.Request["subform"] ?? "";
            string param_calling_form = (string)HttpContext.Request["calling_form"] ?? "";
            string param_calling_id = (string)HttpContext.Request["calling_id"] ?? "";

            // Determine if the form is anonymous
            bool anonymous_form;
            using (var context = new CoTEDB())
            {
                var anonymous = context.Database.SqlQuery<bool>(
                                   "Select anonymous_access from FRM_PROC.dbo.FRM_FORM where form_id = " + param_form_id).ToList();
                anonymous_form = anonymous[0];
            }

            // Anonymous users can only add so reset if not add
            if (anonymous_form == true && param_action.ToUpper() != "ADD")
            {
                anonymous_form = false;
            }

            // Set username, if not logged, force to login screen
            var cookie = HttpContext.Request.Cookies["authenticated_user"];
            string username = "";

            if ((HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity") || anonymous_form == true)
            {
                if (anonymous_form == true)
                {
                    username = "anonymous";
                    Session["index_url"] = "http://www.cote.illinois.edu";
                    Response.Cookies["session_id"].Value = GenerateRandomNumber();
                }
                else
                {
                    username = cookie.Value;
                }
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            //string username = HttpContext.User.Identity.ToString();
            //string username = "eohlsson";
            Session["form_id"] = param_form_id;
            Session["form_action"] = param_action;

            string index_url = (string)Session["index_url"] ?? "";

            // Set global variables
            if (index_url == "")
            {
                if (HttpContext.Request.UrlReferrer != null)
                {
                    index_url = (string)HttpContext.Request.UrlReferrer.ToString() ?? "";
                }

                Session["index_url"] = index_url;

                // If there is no index_url return to loginsuccess page
                if (index_url == "" && anonymous_form != true)
                {
                    Response.Redirect("/dotnet/webpages/webpage.aspxsuccess");
                }

            }
            Session["Username"] = username;
            Session["global_param1"] = HttpContext.Request["param1"];
            Session["global_param2"] = HttpContext.Request["param2"];
            Session["record_id"] = param_record_id;
            Session["calling_id"] = param_calling_id;
            Session["calling_form"] = param_calling_form;

            // FRM_PROC.dbo.FRM_GetParamData_rs 
            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();


                // Check for action
                if (param_action == "delete") // Delete
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "FRM_PROC.dbo.FRM_Delete_Record_v5";
                    cmd.Parameters.Add(new SqlParameter("@form_id", param_form_id));
                    cmd.Parameters.Add(new SqlParameter("@record_id", param_record_id));

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
                        return Redirect("~/FormProcessor/Error.aspx/HttpError");
                    }
                    finally
                    {
                        db.Database.Connection.Close();
                    }

                    return Redirect(Session["index_url"].ToString());

                }
                else // View, Edit, Add
                {

                    if (debug) { Helpers.Helpers.WriteToLog(username, Request.ServerVariables["REMOTE_ADDR"], "viewModelList fill: Start"); }

                    viewModelList = BuildViewModel(param_form_id, param_record_id, param_calling_id, username);
                    Session["viewModelList"] = viewModelList;

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "viewModelList fill: End"); }

                    // Fill the ViewBag
                    ViewBag.form_id = param_form_id;
                    ViewBag.record_id = param_record_id;
                    ViewBag.action = param_action;
                    ViewBag.debug = debug;
                    ViewBag.no_exec = no_exec;
                    ViewBag.subform = param_subform;
                    ViewBag.calling_form = param_calling_form;
                    ViewBag.calling_id = param_calling_id;
                    ViewBag.Username = username;
                    ViewBag.param1 = Session["global_param1"];
                    ViewBag.param2 = Session["global_param2"];

                    var cookie2 = HttpContext.Request.Cookies["session_id"];
                    ViewBag.session_id = cookie2.Value;

                    return View(viewModelList);

                }
            }

            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Index(FormCollection form, string action, IEnumerable<HttpPostedFileBase> userFiles)
        {
            string record_id_field = "";
            string insert_values = "";
            string insert_fields = "";
            string update_sql = "";
            string sql_statement = "";
            string where_clause = "";
            string identity_id = "";
            bool validated = true;
            string session_id;
            string username;
            string form_id;
            string record_id;
            string param1;
            string param2;
            string calling_form;
            string calling_id;
            int file_count = 0;
            
            Dictionary<String, String> form_errors = new Dictionary<string, string>();

            //****************************
            //** Start Saving form (NEW) *
            //****************************

            // Check to see if form has timed out, if it has use form variables 
            if (Session["viewModelList"] == null)
            {
                session_id = form["session_id"];
                username = form["username"];
                form_id = form["form_id"];
                record_id = form["record_id"];
                calling_id = form["calling_id"];
                calling_form = form["calling_form"];
                param1 = form["param1"];
                param2 = form["param2"];
                viewModelList = BuildViewModel(form_id, record_id, calling_id, username);
                Session["viewModelList"] = viewModelList;
            }
            else
            {
                var cookie2 = HttpContext.Request.Cookies["session_id"];
                session_id = (string)cookie2.Value ?? "";
                username = Session["username"].ToString();
                form_id = Session["form_id"].ToString();
                record_id = Session["record_id"].ToString();
                calling_id = (string)Session["calling_id"] ?? "";
                calling_form = (string)Session["calling_form"] ?? "";
                param1 = (string)Session["global_param1"] ?? "";
                param2 = (string)Session["global_param2"] ?? "";
            }

            foreach (Form_Data h in (List<Form_Data>)Session["viewModelList"])
            {

                // Get relation
                foreach (var relation in h.RelationsDataSet.Where(relation => relation.one_to_many == false))
                {

                    // Initialized fields
                    sql_statement = "";
                    update_sql = "";
                    insert_fields = "";
                    insert_values = "";

                    where_clause = relation.select_where_clause;
                    where_clause = where_clause.Replace("[record_id]", form["record_id"].ToString());
                    where_clause = where_clause.Replace("[calling_id]", form["calling_id"].ToString());
                    where_clause = where_clause.Replace("[calling_form]", form["calling_form"].ToString());
                    where_clause = where_clause.Replace("[user_name]", form["Username"].ToString());

                    record_id_field = relation.record_id_field.ToString();

                    // Get questions
                    foreach (var question in h.QuestionsDataSet.Where(question => question.data_location.Substring(0, question.data_location.LastIndexOf(".") + 1) == relation.table_name.ToString() + "." && question.input_type != "display"))
                    {
                        string field_name = "";
                        string field_value = "";

                        field_name = question.data_location.ToString();
                        field_value = (string)form[field_name] ?? "";

                        // Set fields in case we need to go back with an error
                        ViewData[field_name.ToString()] = field_value;

                        //******************
                        // Validate fields - start
                        //******************
                        Regex letters = new Regex("^[a-zA-Z ]+$:");
                        Regex numbers = new Regex("[0-9]{1}(.[0-9]{1,3})?");

                        // required
                        if (question.nullable == false && field_value == "")
                        {
                            validated = false;
                            form_errors.Add(question.data_location, " cannot be left blank");

                        }

                        if (question.nullable == false && question.input_type == "checkbox" && field_value.ToString() == "false")
                        {
                            validated = false;
                            form_errors.Add(question.data_location, " cannot be left blank");

                        }

                        // null date
                        if ((question.type == "date" || question.input_type == "date") && (field_value == "01-01-1900" || field_value == "0001-01-01" || field_value == ""))
                        {
                            //reset to null value
                            field_value = "'~~null~~'";

                        }

                        // number
                        decimal test_number;
                        string test_field = field_value;
                        if (question.data_type == "number" && !Decimal.TryParse(test_field, out test_number) && field_value != "")
                        {
                            validated = false;
                            form_errors.Add(question.data_location, " must be numbers only");

                        }

                        // alpha
                        if (question.data_type == "alpha" && !letters.IsMatch(field_value))
                        {
                            validated = false;
                            form_errors.Add(question.data_location, " must be letters only");

                        }

                        // alnum,areacode,boolean,currency,date,datetime,email,file,file_db,file_path,fourdigityear,id,integer,percent,phone,state2char,text,text_raw,UIUCADUsername,xhtml,zip


                        //******************
                        // Validate fields - end
                        //******************


                        // Don't save blank (non-boolean) field during add 
                        if (field_value.Length > 0 || question.data_type == "boolean" || question.data_type == "file_db" || action == "edit")
                        {
                            // Check for form action
                            if (action == "add") // Insert
                            {
                                insert_fields += ", " + field_name;

                                switch (question.data_type)
                                {
                                    case "boolean":
                                        if (field_value == "true,false" || field_value == "on")
                                        {
                                            insert_values += ", 'true'";
                                        }
                                        else
                                        {
                                            insert_values += ", 'false'";
                                        }
                                        break;
                                    case "file_db":
                                        {
                                            HttpPostedFileBase hpf = Request.Files[file_count] as HttpPostedFileBase;
                                            Stream s = hpf.InputStream;
                                            byte[] appData = new byte[hpf.ContentLength + 1];
                                            if (hpf.FileName != "")
                                            {
                                                s.Read(appData, 0, hpf.ContentLength);
                                                insert_fields += ", " + field_name + "_filename, " + field_name + "_mime";
                                                insert_values += ", cast('" + Convert.ToBase64String(appData) + "' as varchar(max))";
                                                insert_values += ", '" + hpf.FileName.ToString().Replace("'", "''") + "'";
                                                insert_values += ", '" + System.Web.MimeMapping.GetMimeMapping(hpf.FileName).ToString() + "'";
                                            }
                                            else
                                            {
                                                insert_values += ", null";
                                            }

                                            file_count++;

                                        }
                                        break;
                                    case "email":
                                    case "text":
                                    case "date":
                                        insert_values += ", '" + field_value.ToString().Replace("'", "''") + "'";
                                        break;
                                    default:
                                        insert_values += ", " + field_value.ToString().Replace("'", "''");
                                        break;
                                }

                            }
                            else if (action == "edit") // Update
                            {

                                switch (question.data_type)
                                {
                                    case "boolean":
                                        if (field_value == "true,false" || field_value == "on")
                                        {
                                            update_sql += ", " + field_name + " = 'true'";
                                        }
                                        else
                                        {
                                            update_sql += ", " + field_name + " = 'false'";
                                        }
                                        break;
                                    case "file_db":
                                        {
                                            int filecount;
                                            filecount = 0;

                                            HttpPostedFileBase hpf = Request.Files[file_count] as HttpPostedFileBase;
                                            Stream s = hpf.InputStream;
                                            byte[] appData = new byte[hpf.ContentLength + 1];
                                            s.Read(appData, 0, hpf.ContentLength);
                                            if (field_name == Request.Files.AllKeys[filecount].ToString() && Convert.ToBase64String(appData).Length > 4)
                                            {
                                                update_sql += ", " + field_name + "_filename = '" + hpf.FileName.ToString() + "'";
                                                update_sql += ", " + field_name + "_mime = '" + System.Web.MimeMapping.GetMimeMapping(hpf.FileName).ToString() + "'";
                                                update_sql += ", " + field_name + " = '" + Convert.ToBase64String(appData) + "'";
                                            }
                                            file_count++;
                                        }
                                        break;
                                    case "file_path":
                                        {
                                            foreach (string file in Request.Files)
                                            {
                                                field_value = "path" + "filename";
                                                update_sql += ", " + field_name + " = '" + field_value + ")'";
                                                //field_value = appData;
                                            }
                                        }
                                        break;
                                    case "text":
                                    case "date":
                                        if (field_value == "")
                                        {
                                            update_sql += ", " + field_name + " = null ";
                                        }
                                        else
                                        {
                                            update_sql += ", " + field_name + " = '" + field_value.ToString().Replace("'", "''") + "'";
                                        }
                                        break;
                                    default:
                                        if (field_value == "")
                                        {
                                            update_sql += ", " + field_name + " = null ";
                                        }
                                        else
                                        {
                                            update_sql += ", " + field_name + " = " + field_value.ToString().Replace("'", "''");
                                        }
                                        break;
                                }

                            }
                        }

                    }


                    // Make sure that none of the subforms are being edited
                    if (form["grid_action"] != "")
                    {
                        validated = false;
                        // Determine which subform is active
                        // Don't add if already has an error
                        if (!form_errors.ContainsKey(form["grid_action_section"]))
                        {
                            form_errors.Add(form["grid_action_section"], " is still being edited.  You must click the Save or Update link before saving the form.");
                        }
                    }

                    // Check for sections that require records
                    foreach (var section in h.SectionsDataSet.Where(section => section.requires_data == true))
                    {
                        foreach (var required_relation in h.RelationsDataSet.Where(required_relation => required_relation.relation_id == section.one_to_many_relation_id))
                        {
                            int record_count = 0;

                            foreach (var section_record in form.AllKeys.Where(section_record => section_record.Contains(required_relation.table_name + ".")))
                            {
                                record_count++;
                            }

                            if (record_count <= 1)
                            {
                                validated = false;
                                form_errors.Add(section.section_title, " section must contain at least one record.");
                            }
                        }
                    }

                    // If validation is passed saved the data
                    if (validated == true && (insert_fields != "" || update_sql != ""))
                    {

                        string primary_key_insert_column = "";
                        string primary_key_insert_values = "";

                        // Create SQL string
                        if (action == "add")
                        {
                            // Add insertparams
                            primary_key_insert_column = relation.primary_key_insert_columns;
                            primary_key_insert_values = relation.primary_key_insert_values;

                            if (!string.IsNullOrEmpty(primary_key_insert_values))
                            {
                                primary_key_insert_values = primary_key_insert_values.Replace("[param1]", param1);
                                primary_key_insert_values = primary_key_insert_values.Replace("[param2]", param2);
                                primary_key_insert_values = primary_key_insert_values.Replace("[record_id]", record_id);
                                primary_key_insert_values = primary_key_insert_values.Replace("[calling_id]", calling_id);
                                primary_key_insert_values = primary_key_insert_values.Replace("[calling_form]", calling_form);
                                primary_key_insert_values = primary_key_insert_values.Replace("[username]", username);
                            }

                            if (!string.IsNullOrEmpty(primary_key_insert_column))
                            {
                                primary_key_insert_column = ", " + primary_key_insert_column;
                                primary_key_insert_values = ", " + primary_key_insert_values;
                            }

                            // Add created_by and created_date               
                            insert_fields = insert_fields.Substring(2) + ", created_by, created_date " + primary_key_insert_column;
                            insert_values = insert_values.Substring(2) + ", '" + username + "', '" + DateTime.Now + "'" + primary_key_insert_values;

                            sql_statement = "Insert into " + relation.table_name.ToString() + " ( " + insert_fields + ") VALUES (" + insert_values + ")";
                        }
                        else
                        {
                            // Add modified_by and modified_date
                            update_sql += ", modified_by = '" + username + "', modified_date = '" + DateTime.Now + "'";

                            sql_statement = "Update " + relation.table_name.ToString() + " SET " + update_sql.Substring(2) + " WHERE " + where_clause.ToString();
                        }

                        // Removed "~~null~~" and replace with the word null
                        sql_statement = sql_statement.Replace("'~~null~~'", "null");
                        sql_statement = sql_statement.Replace("'null'", "null");
                        sql_statement = sql_statement.Replace("'null'", "null");

                        // Run statement
                        String ConnString = ConfigurationManager.ConnectionStrings["UserDb"].ConnectionString;

                        using (SqlConnection conn = new SqlConnection(ConnString))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = conn;
                                cmd.CommandType = System.Data.CommandType.Text;
                                cmd.CommandText = sql_statement;

                                if (debug) { Helpers.Helpers.WriteToLog(username, Request.ServerVariables["REMOTE_ADDR"], action + ": sql_statement " + sql_statement); }

                                if (!no_exec)
                                {
                                    var reader = cmd.ExecuteReader();
                                    reader.Close();
                                    cmd.Parameters.Clear();

                                    // If it is an Add and it is the primary table get the @@Identity
                                    if (action == "add")
                                    {
                                        foreach (var this_form in h.FormDataSet.Where(f => f.primary_table.ToString() == relation.table_name.ToString()))
                                        {
                                            // Select @@Identity
                                            string identity_sql = "";
                                            identity_id = "";
                                            identity_sql = "Select @@Identity;";
                                            cmd.Connection = conn;
                                            cmd.CommandType = System.Data.CommandType.Text;
                                            cmd.CommandText = identity_sql;

                                            if (debug) { Helpers.Helpers.WriteToLog(username, Request.ServerVariables["REMOTE_ADDR"], action + ": sql_statement " + identity_sql); }

                                            using (var reader3 = cmd.ExecuteReader())
                                            {

                                                while (reader3.Read())
                                                {
                                                    identity_id = reader3[0].ToString();
                                                }
                                            }

                                        }

                                    }


                                    // Write to log table
                                    string log_sql = "";
                                    log_sql = "INSERT INTO [LOGS].[dbo].[FRM_PROCESSOR_LOG] ([form_id],[record_id],[form_action],[error],[client_ip],[url],[sql_transaction],[description],[created_date],[created_by])" +
                                            " VALUES (" +
                                            form_id + "," + record_id + ", '" + action + "', " + "'', '" + Request.ServerVariables["REMOTE_ADDR"] + "', '" + Server.UrlEncode(HttpContext.Request.Url.OriginalString) + "', '" + sql_statement.ToString().Replace("'", "''") + "','','" + DateTime.Now + "', '" + username + "')";
                                    cmd.CommandType = System.Data.CommandType.Text;
                                    cmd.CommandText = log_sql;
                                    var update_log = cmd.ExecuteReader();
                                    update_log.Close();
                                }
                            }
                        }

                    }

                }

                // If add, then update one-to-many relationships
                if (action == "add" && validated == true)
                {

                    // Get relation
                    // foreach (var grid_relation in h.RelationsDataSet.Where(r => r.one_to_many == true && r.permissions_mask.Contains("add")))
                    foreach (var grid_relation in h.RelationsDataSet.Where(r => r.table_name != h.FormDataSet.First().primary_table.ToString() && r.permissions_mask.Contains("add")))
                    {

                        //if (h.SectionsDataSet.Where(s => s.one_to_many_relation_id == grid_relation.relation_id).Count() > 0)
                        //{
                        String ConnString = ConfigurationManager.ConnectionStrings["UserDb"].ConnectionString;

                        // Get new record_id
                        // Update record_ids from session_id to new record_id
                        using (SqlConnection conn = new SqlConnection(ConnString))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                string where_clause_record_id = "";

                                cmd.Connection = conn;

                                where_clause_record_id = grid_relation.select_where_clause.Replace("= '[record_id]'", "");
                                if (where_clause_record_id.IndexOf(" and ") > 0)
                                {
                                    where_clause_record_id = where_clause_record_id.Substring(0, where_clause_record_id.IndexOf(" and "));
                                }

                                // Account for anonymous forms 
                                if (record_id == "-1")
                                {
                                    sql_statement = "Update " + grid_relation.table_name + " set " + where_clause_record_id + " = " + identity_id + " where " + where_clause_record_id + " = '-1'";
                                }
                                else
                                {
                                    sql_statement = "Update " + grid_relation.table_name + " set " + where_clause_record_id + " = " + identity_id + " where " + where_clause_record_id + " = " + session_id;
                                }

                                cmd.CommandType = System.Data.CommandType.Text;
                                cmd.CommandText = sql_statement;

                                var reader = cmd.ExecuteReader();
                                reader.Close();
                                cmd.Parameters.Clear();

                                sql_statement = "";

                            }
                        }
                        //}
                    }
                }

            }

            //****************************
            //** End Saving form (NEW)   *
            //****************************

            // Form not validated, don't execute trigger
            if (form_errors.Count() == 0)
            {

                // Trigger
                int trigger_count = 0;
                string trigger_url = "";
                Form_Trigger trigger = new Form_Trigger();

                foreach (Form_Data h in (List<Form_Data>)Session["viewModelList"])
                {
                    //foreach (Form_Trigger t in h.TriggerDataSet)
                    foreach (Form_Trigger t in h.TriggerDataSet.Where(x => x.triggering_action == form["Action"].ToString()))
                        {
                        trigger_count++;
                        trigger = t;
                    }
                }

                // if it is a subform redirect it back to the calling form
                if (form["subform"] == "true")
                {
                    // Close window
                    return Content("<script type='text/javascript'>window.close();</script>");
                    //return Redirect("/dotnet/FormProcessor/form.aspx/index?form_id=" + form["callingform"] + "&action=edit&record_id=" + form["callingid"]);
                }

                // if no trigger return to index
                if (trigger_count == 0)
                {

                    string index_url = (string)Session["index_url"] ?? "";

                    // If there is no index URL then redirect to login
                    if (index_url == "")
                    {
                        index_url = "/dotnet/account.aspx/loginsuccess";
                    }

                    // Clear out all session variables
                    Session["index_url"] = "";
                    Session["form_id"] = "";
                    Session["record_id"] = "";
                    Session["calling_id"] = "";
                    Session["calling_form"] = "";
                    Session["global_param1"] = "";
                    Session["global_param2"] = "";
                    Session["viewModelList"] = "";
                    Session["index_url"] = "";

                    return Redirect(index_url);
                }
                else
                {
                    if (form["Action"].ToString() == trigger.triggering_action)
                    {
                        switch (trigger.trigger_type)
                        {
                            case "redirect":
                                {
                                    trigger_url = trigger.trigger_source;
                                    trigger_url = trigger_url.Replace("[param1]", param1);
                                    trigger_url = trigger_url.Replace("[param2]", param2);


                                    // Clear out all session variables
                                    Session["index_url"] = "";
                                    Session["form_id"] = "";
                                    Session["record_id"] = "";
                                    Session["calling_id"] = "";
                                    Session["calling_form"] = "";
                                    Session["global_param1"] = "";
                                    Session["global_param2"] = "";
                                    Session["viewModelList"] = "";

                                    return Redirect(trigger_url);
                                    break;
                                }
                            case "sql":
                                {
                                    string trigger_sql = "";

                                    using (var db = new CoTEDB())
                                    {
                                        // If using Code First we need to make sure the model is built before we open the connection
                                        // This isn't required for models created with the EF Designer
                                        db.Database.Initialize(force: false);

                                        // Create a SQL command to execute the sproc
                                        var cmd = db.Database.Connection.CreateCommand();
                                        trigger_sql = trigger.trigger_source.ToString();
                                        trigger_sql = trigger_sql.Replace("[param1]", param1);
                                        trigger_sql = trigger_sql.Replace("[param2]", param2);
                                        trigger_sql = trigger_sql.Replace("[record_id]", record_id);
                                        cmd.CommandType = System.Data.CommandType.Text;
                                        cmd.CommandText = trigger_sql;

                                        try
                                        {

                                            db.Database.Connection.Open();
                                            // Run the sproc 
                                            var reader = cmd.ExecuteReader();

                                            while (reader.Read())
                                            {
                                                trigger_url = reader[0].ToString();
                                            }

                                            reader.Close();

                                        }
                                        finally
                                        {
                                            db.Database.Connection.Close();
                                        }
                                    }


                                    // Clear out all session variables
                                    Session["index_url"] = "";
                                    Session["form_id"] = "";
                                    Session["record_id"] = "";
                                    Session["calling_id"] = "";
                                    Session["calling_form"] = "";
                                    Session["global_param1"] = "";
                                    Session["global_param2"] = "";
                                    Session["viewModelList"] = ""; 
                                    
                                    return Redirect(trigger_url);

                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                }
            }

            // Form not validated, return to view
            if (form_errors.Count() > 0)
            {
                ViewBag.form_errors = form_errors;
                ViewBag.form_id = form_id;
                ViewBag.record_id = record_id;
                ViewBag.session_id = session_id;
                ViewBag.calling_id = calling_id;
                ViewBag.calling_form = calling_form;
                ViewBag.action = action;
                ViewBag.debug = debug;
                ViewBag.no_exec = no_exec;
                return View((List<Form_Data>)Session["viewModelList"]);
            }
            else // No data submitted, return to index
            {
                string index_url = (string)Session["index_url"] ?? "";

                // If there is no index URL then redirect to login
                if (index_url == "")
                {
                    index_url = "/dotnet/account.aspx/loginsuccess";
                }

                // Clear out all session variables
                Session["index_url"] = "";
                Session["form_id"] = "";
                Session["record_id"] = "";
                Session["calling_id"] = "";
                Session["calling_form"] = "";
                Session["global_param1"] = "";
                Session["global_param2"] = "";
                Session["viewModelList"] = "";

                return Redirect(index_url);
            }

        }

        [HttpGet, ValidateInput(false)]
        public JsonResult AddGridRow(string section, string fieldnames, string fieldvalues, string insertparams)
        {
            string insertparamvalues;
            string param1 = (string)Session["global_param1"] ?? "";
            string param2 = (string)Session["global_param2"] ?? "";

            //Process file attachments if there are any
            while (fieldvalues.Contains("[attachment]"))
            {
                string attachment_name = "";
                string attachment_value = "";
                string filename = "";
                attachment_name = fieldvalues.Substring(fieldvalues.IndexOf("[attachment]") + 12);
                attachment_name = attachment_name.Substring(1, attachment_name.IndexOf("]") - 1);
                filename = attachment_name;
                attachment_name = "attachment_" + attachment_name;
                attachment_value = (string)Session[attachment_name] ?? "";
                fieldvalues = fieldvalues.Replace("[attachment][" + filename + "]", attachment_value);
            }

            insertparamvalues = insertparams;
            insertparamvalues = insertparamvalues.Replace("'[record_id]'", Session["record_id"].ToString());
            insertparamvalues = insertparamvalues.Replace("'[calling_id]'", Session["calling_id"].ToString());
            insertparamvalues = insertparamvalues.Replace("'[calling_form]'", Session["calling_form"].ToString());
            insertparamvalues = insertparamvalues.Replace("[param1]", param1);
            insertparamvalues = insertparamvalues.Replace("[param2]", param2);
            insertparamvalues = insertparamvalues.Replace("[username]", Session["Username"].ToString());
            //insertparamvalues = insertparamvalues.Replace("'", "''");
            //insertparamvalues = "'" + insertparamvalues + "'";

            // FRM_Insert_Grid_Row_rs_v5
            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FRM_PROC.dbo.FRM_Insert_Grid_Row_rs_v5";
                cmd.Parameters.Add(new SqlParameter("@section", section));
                cmd.Parameters.Add(new SqlParameter("@fieldnames", fieldnames));
                cmd.Parameters.Add(new SqlParameter("@fieldvalues", fieldvalues));
                cmd.Parameters.Add(new SqlParameter("@insertparamvalues", insertparamvalues));

                if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], cmd.CommandText + " @section= " + section + ", @fieldnames= " + fieldnames + ", @fieldvalues= " + fieldvalues + ", @insertparamvalues = " + insertparamvalues); }
  
                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    //if (!no_exec)
                    //{
                        var reader = cmd.ExecuteReader();
                    //}

                }
                catch (SqlException e)
                {
                    //Redirect to an error page
                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                    return Json(new
                    {
                        redirectUrl = Url.Action("Error", "HttpError", new { area = "FormProcessor" }),
                        isRedirect = true
                    },
                        JsonRequestBehavior.AllowGet
                    );
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }

            bool result = true;

            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, ValidateInput(false)]
        public JsonResult UpdateGridRow(string section, string update_sql, string record_id)
        {


            //Process file attachments if there are any
            while (update_sql.Contains("[attachment]"))
            {
                string attachment_name = "";
                string attachment_value = "";
                string filename = "";
                attachment_name = update_sql.Substring(update_sql.IndexOf("[attachment]") + 12);
                attachment_name = attachment_name.Substring(1, attachment_name.IndexOf("]") - 1);
                filename = attachment_name;
                attachment_name = "attachment_" + attachment_name;
                attachment_value = (string)Session[attachment_name] ?? "";
                update_sql = update_sql.Replace("[attachment][" + filename + "]", attachment_value);
            }

            // FRM_Update_Grid_Row_rs_v5
            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Replace escape characters in update_sql
                update_sql = update_sql.Replace("''''", "''");
                update_sql = update_sql.Replace("'''", "''");

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FRM_PROC.dbo.FRM_Update_Grid_Row_rs_v5";
                cmd.Parameters.Add(new SqlParameter("@section", section));
                cmd.Parameters.Add(new SqlParameter("@update_sql", update_sql));
                cmd.Parameters.Add(new SqlParameter("@record_id", record_id));

                if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], cmd.CommandText + " @section= " + section + ", @update_sql= " + update_sql + ", @record_id= " + record_id); }

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    if (!no_exec)
                    {
                        var reader = cmd.ExecuteReader();
                        int test = 0;
                    }

                }
                catch (Exception e)
                {
                    //Redirect to an error page
                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                    return Json(new
                    {
                        redirectUrl = Url.Action("Error", "HttpError", new { area = "FormProcessor" }),
                        isRedirect = true
                    },
                        JsonRequestBehavior.AllowGet
                    );
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }

            bool result = true;

            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DeleteGridRow(string section, string record_id)
        {

            // FRM_Delete_Grid_Row_rs_v5
            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FRM_PROC.dbo.FRM_Delete_Grid_Row_rs_v5";
                cmd.Parameters.Add(new SqlParameter("@section", section));
                cmd.Parameters.Add(new SqlParameter("@record_id", record_id));

                if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], cmd.CommandText + " @section= " + section + ", @record_id= " + record_id); }

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    if (!no_exec)
                    {
                        var reader = cmd.ExecuteReader();
                    }

                }
                catch (Exception e)
                {
                    //Redirect to an error page
                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                    return Json(new
                    {
                        redirectUrl = Url.Action("Error", "HttpError", new { area = "FormProcessor" }),
                        isRedirect = true
                    },
                        JsonRequestBehavior.AllowGet
                    );
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }

            bool result = true;

            return Json(new { result }, JsonRequestBehavior.AllowGet);
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

                // Check if dynamic sql
                foreach (Form_Valuelists item in value_lists.Where(values => values.question_id.ToString() == question_list && values.dynamic_sql.ToString() == "True"))
                {
                    new_values = BuildDynamicSQLValueList(question_value, question_list, item.null_value_name, Session["Username"].ToString(), new_values);
                }

            }

            return Json(new SelectList(new_values, "Value", "Text"), JsonRequestBehavior.AllowGet);

        }


        public FileContentResult GetFile(int question_id, int record_id, int form_id)
        {
            byte[] fileContent = null;
            string mimeType = ""; 
            string fileName = "";
            string sql_statement = "";

            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FRM_PROC.dbo.FRM_Get_File_SQL";
                cmd.Parameters.Add(new SqlParameter("@question_id", question_id));
                cmd.Parameters.Add(new SqlParameter("@record_id", record_id));
                cmd.Parameters.Add(new SqlParameter("@form_id", form_id));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    if (!no_exec)
                    {
                        var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            sql_statement = reader[0].ToString();
                        }

                        reader.Close();
                        cmd.Parameters.Clear();

                        // Load Value Lists
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.CommandText = sql_statement;

                        using (var reader2 = cmd.ExecuteReader())
                        {

                            while (reader2.Read())
                            {

                                fileContent = Convert.FromBase64String(reader2["FileContent"].ToString());
                                fileName = reader2["FileName"].ToString();
                                mimeType = reader2["FileMimeType"].ToString();
                                reader2.NextResult();

                            }

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

            //mimeType = "application/pdf";
            //mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            if (mimeType == "")
            {
                if (fileName.Contains(".msg"))
                {
                    mimeType = "application/vnd.ms-outlook";
                }
            }

            return File(fileContent, mimeType, fileName);

        }

        [HttpPost]
        public async Task<JsonResult> UploadFile(string id)
        {
            try
            {
                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    var attachment_name = "";
                 
                    // Store to session variable
                    HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
                    Stream s = hpf.InputStream;
                    byte[] appData = new byte[hpf.ContentLength + 1];
                    s.Read(appData, 0, hpf.ContentLength);
                    attachment_name = "attachment_" + hpf.FileName.ToString().Replace("'", "");
                    Session[attachment_name] = Convert.ToBase64String(appData);

                }
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }

            return Json("File uploaded successfully");
        }
        //************** TESTING AREA ************************
        //                                                   *
        //                                                   *
        //****************************************************
        [HttpGet]
        public ActionResult Index_Test()
        {

            // Get parameters from querystring
            string param_form_id = (string)HttpContext.Request["form_id"] ?? "";
            string param_record_id = (string)HttpContext.Request["record_id"] ?? "";
            string param_action = (string)HttpContext.Request["action"] ?? "";
            string param_subform = (string)HttpContext.Request["subform"] ?? "";
            string param_calling_form = (string)HttpContext.Request["calling_form"] ?? "";
            string param_calling_id = (string)HttpContext.Request["calling_id"] ?? "";

            // Determine if the form is anonymous
            bool anonymous_form;
            using (var context = new CoTEDB())
            {
                var anonymous = context.Database.SqlQuery<bool>(
                                   "Select anonymous_access from FRM_PROC.dbo.FRM_FORM where form_id = " + param_form_id).ToList();
                anonymous_form = anonymous[0];
            }

            // Anonymous users can only add so reset if not add
            if (anonymous_form == true && param_action.ToUpper() != "ADD")
            {
                anonymous_form = false;
            }

            // Set username, if not logged, force to login screen
            var cookie = HttpContext.Request.Cookies["authenticated_user"];
            string username = "";

            if ((HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity") || anonymous_form == true)
            {
                if (anonymous_form == true)
                {
                    username = "anonymous";
                    Session["index_url"] = "http://www.cote.illinois.edu";
                    Response.Cookies["session_id"].Value = GenerateRandomNumber();

                }
                else
                {
                    username = cookie.Value;
                }
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            //string username = HttpContext.User.Identity.ToString();
            //string username = "eohlsson";
            Session["form_id"] = param_form_id;
            Session["form_action"] = param_action;

            string index_url = (string)Session["index_url"] ?? "";

            // Set global variables
            if (index_url == "")
            {
                if (HttpContext.Request.UrlReferrer != null)
                {
                    index_url = (string)HttpContext.Request.UrlReferrer.ToString() ?? "";
                }

                Session["index_url"] = index_url;

                // If there is no index_url return to loginsuccess page
                if (index_url == "" && anonymous_form != true)
                {
                    Response.Redirect("/dotnet/webpages/webpage.aspxsuccess");
                }

            }
            Session["Username"] = username;
            Session["global_param1"] = HttpContext.Request["param1"];
            Session["global_param2"] = HttpContext.Request["param2"];
            Session["record_id"] = param_record_id;
            Session["calling_id"] = param_calling_id;
            Session["calling_form"] = param_calling_form;

            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();


                // Check for action
                if (param_action == "delete") // Delete
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "FRM_PROC.dbo.FRM_Delete_Record_v5";
                    cmd.Parameters.Add(new SqlParameter("@form_id", param_form_id));
                    cmd.Parameters.Add(new SqlParameter("@record_id", param_record_id));

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
                        return Redirect("~/FormProcessor/Error.aspx/HttpError");
                    }
                    finally
                    {
                        db.Database.Connection.Close();
                    }

                    return Redirect(Session["index_url"].ToString());

                }
                else // View, Edit, Add
                {

                    if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "viewModelList fill: Start"); }

                        viewModelList = BuildViewModel(param_form_id, param_record_id, param_calling_id, username);
                        Session["viewModelList"] = viewModelList;

                        if (debug) { Helpers.Helpers.WriteToLog(Session["Username"].ToString(), Request.ServerVariables["REMOTE_ADDR"], "viewModelList fill: End"); }

                        // Fill the ViewBag
                        ViewBag.form_id = param_form_id;
                        ViewBag.record_id = param_record_id;
                        ViewBag.action = param_action;
                        ViewBag.debug = debug;
                        ViewBag.no_exec = no_exec;
                        ViewBag.subform = param_subform;
                        ViewBag.calling_form = param_calling_form;
                        ViewBag.calling_id = param_calling_id;
                        ViewBag.Username = username;
                        ViewBag.param1 = Session["global_param1"];
                        ViewBag.param2 = Session["global_param2"];

                        var cookie2 = HttpContext.Request.Cookies["session_id"];
                        ViewBag.session_id = cookie2.Value;

                        return View(viewModelList);

                }
            }

            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Index_Test(FormCollection form, string action, IEnumerable<HttpPostedFileBase> userFiles)
        {
            string record_id_field = "";
            string insert_values = "";
            string insert_fields = "";
            string update_sql = "";
            string sql_statement = "";
            string where_clause = "";
            string identity_id = "";
            bool validated = true;
            string session_id;
            string username;
            string form_id;
            string record_id;
            string param1;
            string param2;
            string calling_form;
            string calling_id;
            Dictionary<String, String> form_errors = new Dictionary<string, string>();

            //****************************
            //** Start Saving form (NEW) *
            //****************************

            // Check to see if form has timed out, if it has use form variables 
            if (Session["viewModelList"] == null)
            {
                session_id = form["session_id"];
                username = form["username"];
                form_id = form["form_id"];
                record_id = form["record_id"];
                calling_id = form["calling_id"];
                calling_form = form["calling_form"];
                param1 = form["param1"];
                param2 = form["param2"];
                viewModelList = BuildViewModel(form_id, record_id, calling_id, username);
                Session["viewModelList"] = viewModelList;
            }
            else
            {
                session_id = (string)Session["session_id"] ?? "";
                username = Session["username"].ToString();
                form_id = Session["form_id"].ToString();
                record_id = Session["record_id"].ToString();
                calling_id = (string)Session["calling_id"] ?? "";
                calling_form = (string)Session["calling_form"] ?? "";
                param1 = (string)Session["global_param1"] ?? "";
                param2 = (string)Session["global_param2"] ?? "";
            }

            foreach (Form_Data h in (List<Form_Data>)Session["viewModelList"])
            {

                // Get relation
                foreach (var relation in h.RelationsDataSet.Where(relation => relation.one_to_many == false))
                {

                    // Initialized fields
                    sql_statement = "";
                    update_sql = "";
                    insert_fields = "";
                    insert_values = "";

                    where_clause = relation.select_where_clause;
                    where_clause = where_clause.Replace("[record_id]", form["record_id"].ToString());
                    where_clause = where_clause.Replace("[calling_id]", form["calling_id"].ToString());
                    where_clause = where_clause.Replace("[calling_form]", form["calling_form"].ToString());

                    record_id_field = relation.record_id_field.ToString();

                    // Get questions
                    foreach (var question in h.QuestionsDataSet.Where(question => question.data_location.Substring(0, question.data_location.LastIndexOf(".") + 1) == relation.table_name.ToString() + "." && question.input_type != "display"))
                    {
                        string field_name = "";
                        string field_value = "";

                        field_name = question.data_location.ToString();
                        field_value = (string)form[field_name] ?? "";

                        // Set fields in case we need to go back with an error
                        ViewData[field_name.ToString()] = field_value;

                        //******************
                        // Validate fields - start
                        //******************
                        Regex letters = new Regex("^[a-zA-Z ]+$");
                        Regex numbers = new Regex("[0-9]{1}(.[0-9]{1,3})?");

                        // required
                        if (question.nullable == false && field_value == "")
                        {
                            validated = false;
                            form_errors.Add(question.data_location, " cannot be left blank");

                        }

                        // null date
                        if ((question.type == "date" || question.input_type == "date") && (field_value == "01-01-1900" || field_value == "0001-01-01" || field_value == ""))
                        {
                            //reset to null value
                            field_value = "'~~null~~'";

                        }

                        // number
                        decimal test_number;
                        string test_field = field_value;
                        if (question.data_type == "number" && !Decimal.TryParse(test_field, out test_number) && field_value != "")
                        {
                            validated = false;
                            form_errors.Add(question.data_location, " must be numbers only");

                        }

                        // alpha
                        if (question.data_type == "alpha" && !letters.IsMatch(field_value))
                        {
                            validated = false;
                            form_errors.Add(question.data_location, " must be letters only");

                        }

                        // alnum,areacode,boolean,currency,date,datetime,email,file,file_db,file_path,fourdigityear,id,integer,percent,phone,state2char,text,text_raw,UIUCADUsername,xhtml,zip


                        //******************
                        // Validate fields - end
                        //******************


                        // Don't save blank (non-boolean) field during add 
                        if (field_value.Length > 0 || question.data_type == "boolean" || question.data_type == "file_db" || action == "edit")
                        {
                            // Check for form action
                            if (action == "add") // Insert
                            {
                                insert_fields += ", " + field_name;

                                switch (question.data_type)
                                {
                                    case "boolean":
                                        if (field_value == "true,false")
                                        {
                                            insert_values += ", 'true'";
                                        }
                                        else
                                        {
                                            insert_values += ", 'false'";
                                        }
                                        break;
                                    case "file_db":
                                        {
                                            foreach (string file in Request.Files)
                                            {
                                                HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
                                                Stream s = hpf.InputStream;
                                                byte[] appData = new byte[hpf.ContentLength + 1];
                                                s.Read(appData, 0, hpf.ContentLength);
                                                insert_fields += ", " + field_name + "_filename, " + field_name + "_mime";
                                                insert_values += ", cast('" + Convert.ToBase64String(appData) + "' as varchar(max))";
                                                insert_values += ", '" + hpf.FileName.ToString() + "'";
                                                insert_values += ", '" + System.Web.MimeMapping.GetMimeMapping(hpf.FileName).ToString() + "'";
                                            }

                                        }
                                        break;
                                    case "email":
                                    case "text":
                                    case "date":
                                        insert_values += ", '" + field_value.ToString().Replace("'", "''") + "'";
                                        break;
                                    default:
                                        insert_values += ", " + field_value.ToString().Replace("'", "''");
                                        break;
                                }

                            }
                            else if (action == "edit") // Update
                            {

                                switch (question.data_type)
                                {
                                    case "boolean":
                                        if (field_value == "true,false")
                                        {
                                            update_sql += ", " + field_name + " = 'true'";
                                        }
                                        else
                                        {
                                            update_sql += ", " + field_name + " = 'false'";
                                        }
                                        break;
                                    case "file_db":
                                        {
                                            foreach (string file in Request.Files)
                                            {
                                                HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
                                                Stream s = hpf.InputStream;
                                                byte[] appData = new byte[hpf.ContentLength + 1];
                                                s.Read(appData, 0, hpf.ContentLength);
                                                update_sql += ", " + field_name + "_filename = '" + hpf.FileName.ToString() + "'";
                                                update_sql += ", " + field_name + "_mime = '" + System.Web.MimeMapping.GetMimeMapping(hpf.FileName).ToString() + "'";
                                                update_sql += ", " + field_name + " = '" + Convert.ToBase64String(appData) + "'";
                                            }

                                        }
                                        break;
                                    case "file_path":
                                        {
                                            foreach (string file in Request.Files)
                                            {
                                                field_value = "path" + "filename";
                                                update_sql += ", " + field_name + " = '" + field_value + ")'";
                                                //field_value = appData;
                                            }
                                        }
                                        break;
                                    case "text":
                                    case "date":
                                        if (field_value == "")
                                        {
                                            update_sql += ", " + field_name + " = null ";
                                        }
                                        else
                                        {
                                            update_sql += ", " + field_name + " = '" + field_value.ToString().Replace("'", "''") + "'";
                                        }
                                        break;
                                    default:
                                        if (field_value == "")
                                        {
                                            update_sql += ", " + field_name + " = null ";
                                        }
                                        else
                                        {
                                            update_sql += ", " + field_name + " = " + field_value.ToString().Replace("'", "''");
                                        }
                                        break;
                                }

                            }
                        }

                    }

                    // If validation is passed saved the data
                    if (validated == true && (insert_fields != "" || update_sql != ""))
                    {

                        string primary_key_insert_column = "";
                        string primary_key_insert_values = "";

                        // Create SQL string
                        if (action == "add")
                        {
                            // Add insertparams
                            primary_key_insert_column = relation.primary_key_insert_columns;
                            primary_key_insert_values = relation.primary_key_insert_values;

                            if (!string.IsNullOrEmpty(primary_key_insert_values))
                            {
                                primary_key_insert_values = primary_key_insert_values.Replace("[param1]", param1);
                                primary_key_insert_values = primary_key_insert_values.Replace("[param2]", param2);
                                primary_key_insert_values = primary_key_insert_values.Replace("[record_id]", record_id);
                                primary_key_insert_values = primary_key_insert_values.Replace("[calling_id]", calling_id);
                                primary_key_insert_values = primary_key_insert_values.Replace("[calling_form]", calling_form);
                                primary_key_insert_values = primary_key_insert_values.Replace("[username]", username);
                            }

                            if (!string.IsNullOrEmpty(primary_key_insert_column))
                            {
                                primary_key_insert_column = ", " + primary_key_insert_column;
                                primary_key_insert_values = ", " + primary_key_insert_values;
                            }

                            // Add created_by and created_date               
                            insert_fields = insert_fields.Substring(2) + ", created_by, created_date " + primary_key_insert_column;
                            insert_values = insert_values.Substring(2) + ", '" + username + "', '" + DateTime.Now + "'" + primary_key_insert_values;

                            sql_statement = "Insert into " + relation.table_name.ToString() + " ( " + insert_fields + ") VALUES (" + insert_values + ")";
                        }
                        else
                        {
                            // Add modified_by and modified_date
                            update_sql += ", modified_by = '" + username + "', modified_date = '" + DateTime.Now + "'";

                            sql_statement = "Update " + relation.table_name.ToString() + " SET " + update_sql.Substring(2) + " WHERE " + where_clause.ToString();
                        }

                        // Removed "~~null~~" and replace with the word null
                        sql_statement = sql_statement.Replace("'~~null~~'", "null");
                        sql_statement = sql_statement.Replace("'null'", "null");
                        sql_statement = sql_statement.Replace("'null'", "null");

                        // Run statement
                        String ConnString = ConfigurationManager.ConnectionStrings["UserDb"].ConnectionString;

                        using (SqlConnection conn = new SqlConnection(ConnString))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = conn;
                                cmd.CommandType = System.Data.CommandType.Text;
                                cmd.CommandText = sql_statement;

                                if (debug) { Helpers.Helpers.WriteToLog(username, Request.ServerVariables["REMOTE_ADDR"], action + ": sql_statement " + sql_statement); }

                                if (!no_exec)
                                {
                                    var reader = cmd.ExecuteReader();
                                    reader.Close();
                                    cmd.Parameters.Clear();

                                    // If it is an Add and it is the primary table get the @@Identity
                                    if (action == "add")
                                    {
                                        foreach (var this_form in h.FormDataSet.Where(f => f.primary_table.ToString() == relation.table_name.ToString()))
                                        {
                                            // Select @@Identity
                                            string identity_sql = "";
                                            identity_id = "";
                                            identity_sql = "Select @@Identity;";
                                            cmd.Connection = conn;
                                            cmd.CommandType = System.Data.CommandType.Text;
                                            cmd.CommandText = identity_sql;

                                            if (debug) { Helpers.Helpers.WriteToLog(username, Request.ServerVariables["REMOTE_ADDR"], action + ": sql_statement " + identity_sql); }

                                            using (var reader3 = cmd.ExecuteReader())
                                            {

                                                while (reader3.Read())
                                                {
                                                    identity_id = reader3[0].ToString();
                                                }
                                            }

                                        }

                                    }


                                    // Write to log table
                                    string log_sql = "";
                                    log_sql = "INSERT INTO [LOGS].[dbo].[FRM_PROCESSOR_LOG] ([form_id],[record_id],[form_action],[error],[client_ip],[url],[sql_transaction],[description],[created_date],[created_by])" +
                                            " VALUES (" +
                                            form_id + "," + record_id + ", '" + action + "', " + "'', '" + Request.ServerVariables["REMOTE_ADDR"] + "', '" + Server.UrlEncode(HttpContext.Request.Url.OriginalString) + "', '" + sql_statement.ToString().Replace("'", "''") + "','','" + DateTime.Now + "', '" + username + "')";
                                    cmd.CommandType = System.Data.CommandType.Text;
                                    cmd.CommandText = log_sql;
                                    var update_log = cmd.ExecuteReader();
                                    update_log.Close();
                                }
                            }
                        }

                    }

                }

                // If add, then update one-to-many relationships
                if (action == "add" && validated == true)
                {

                    // Get relation
                    // foreach (var grid_relation in h.RelationsDataSet.Where(r => r.one_to_many == true && r.permissions_mask.Contains("add")))
                    foreach (var grid_relation in h.RelationsDataSet.Where(r => r.table_name != h.FormDataSet.First().primary_table.ToString() && r.permissions_mask.Contains("add")))
                    {

                        //if (h.SectionsDataSet.Where(s => s.one_to_many_relation_id == grid_relation.relation_id).Count() > 0)
                        //{
                        String ConnString = ConfigurationManager.ConnectionStrings["UserDb"].ConnectionString;

                        // Get new record_id
                        // Update record_ids from session_id to new record_id
                        using (SqlConnection conn = new SqlConnection(ConnString))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                string where_clause_record_id = "";

                                cmd.Connection = conn;

                                where_clause_record_id = grid_relation.select_where_clause.Replace("= '[record_id]'", "");
                                if (where_clause_record_id.IndexOf(" and ") > 0)
                                {
                                    where_clause_record_id = where_clause_record_id.Substring(0, where_clause_record_id.IndexOf(" and "));
                                }

                                sql_statement = "Update " + grid_relation.table_name + " set " + where_clause_record_id + " = " + identity_id + " where " + where_clause_record_id + " = " + session_id;
                                cmd.CommandType = System.Data.CommandType.Text;
                                cmd.CommandText = sql_statement;

                                var reader = cmd.ExecuteReader();
                                reader.Close();
                                cmd.Parameters.Clear();

                                sql_statement = "";

                            }
                        }
                        //}
                    }
                }

            }

            //****************************
            //** End Saving form (NEW)   *
            //****************************

            // Form not validated, don't execute trigger
            if (form_errors.Count() == 0)
            {

                // Trigger
                int trigger_count = 0;
                string trigger_url = "";
                Form_Trigger trigger = new Form_Trigger();

                foreach (Form_Data h in (List<Form_Data>)Session["viewModelList"])
                {
                    //foreach (Form_Trigger t in h.TriggerDataSet)
                    foreach (Form_Trigger t in h.TriggerDataSet.Where(r => r.triggering_action == form["form_action"].ToString()))
                        {
                        trigger_count++;
                        trigger = t;
                    }
                }

                // if it is a subform redirect it back to the calling form
                if (form["subform"] == "true")
                {
                    // Close window
                    return Content("<script type='text/javascript'>window.close();</script>");
                    //return Redirect("/dotnet/FormProcessor/form.aspx/index?form_id=" + form["callingform"] + "&action=edit&record_id=" + form["callingid"]);
                }

                // if no trigger return to index
                if (trigger_count == 0)
                {

                    string index_url = (string)Session["index_url"] ?? "";

                    // If there is no index URL then redirect to login
                    if (index_url == "")
                    {
                        index_url = "/dotnet/account.aspx/loginsuccess";
                    }

                    // Clear out all session variables
                    Session["index_url"] = "";
                    Session["form_id"] = "";
                    Session["record_id"] = "";
                    Session["calling_id"] = "";
                    Session["calling_form"] = "";
                    Session["global_param1"] = "";
                    Session["global_param2"] = "";
                    Session["viewModelList"] = "";
                    Session["index_url"] = "";

                    return Redirect(index_url);
                }
                else
                {
                    if (form["form_action"].ToString() == trigger.triggering_action)
                    {
                        switch (trigger.trigger_type)
                        {
                            case "redirect":
                                {
                                    trigger_url = trigger.trigger_source;
                                    trigger_url = trigger_url.Replace("[param1]", param1);
                                    trigger_url = trigger_url.Replace("[param2]", param2);


                                    // Clear out all session variables
                                    Session["index_url"] = "";
                                    Session["form_id"] = "";
                                    Session["record_id"] = "";
                                    Session["calling_id"] = "";
                                    Session["calling_form"] = "";
                                    Session["global_param1"] = "";
                                    Session["global_param2"] = "";
                                    Session["viewModelList"] = "";

                                    return Redirect(trigger_url);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                }
            }

            // Form not validated, return to view
            if (form_errors.Count() > 0)
            {
                ViewBag.form_errors = form_errors;
                ViewBag.form_id = form_id;
                ViewBag.record_id = record_id;
                ViewBag.session_id = session_id;
                ViewBag.calling_id = calling_id;
                ViewBag.calling_form = calling_form;
                ViewBag.action = action;
                ViewBag.debug = debug;
                ViewBag.no_exec = no_exec;
                return View((List<Form_Data>)Session["viewModelList"]);
            }
            else // No data submitted, return to index
            {
                string index_url = (string)Session["index_url"] ?? "";

                // If there is no index URL then redirect to login
                if (index_url == "")
                {
                    index_url = "/dotnet/account.aspx/loginsuccess";
                }

                // Clear out all session variables
                Session["index_url"] = "";
                Session["form_id"] = "";
                Session["record_id"] = "";
                Session["calling_id"] = "";
                Session["calling_form"] = "";
                Session["global_param1"] = "";
                Session["global_param2"] = "";
                Session["viewModelList"] = "";

                return Redirect(index_url);
            }
        }

    }

}
