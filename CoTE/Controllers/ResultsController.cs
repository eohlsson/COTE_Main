using System;
using FormProcessor.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Configuration;
using System.Dynamic;
using System.Collections;
using System.Net.Http.Headers;

namespace CoTE.Controllers
{
    [RoutePrefix("api/Results")]
    public class ResultsController : ApiController
    {

        [HttpGet, Route("GetResults/{form_id}/{index_id}")]
        public IHttpActionResult GetResults(string form_id, string index_id)
        {
            //List<tblProduct> productList = null;
            int recordsTotal = 0;
            //var resultsList = new List<results> { };
            var resultsList = new List<dynamic>();
            string error_message = "";

            CookieHeaderValue cookie = Request.Headers.GetCookies("authenticated_user").FirstOrDefault();
            string cookiename = "index_filter_" + index_id.ToString();

            string where_clause = cookie[cookiename].Values["**sql**"];
            if (where_clause != null)
            {
                where_clause = where_clause.Replace("|", "%");
                where_clause = where_clause.Replace("`", "+");
            }
            else
            {
                where_clause = "";
            }

            string username = cookie["authenticated_user"].Value;
            
            string action_column_mapping = "";
            action_column_mapping = cookie[cookiename].Values["**action_column_mapping**"];
            if (action_column_mapping != null)
            {
                action_column_mapping = action_column_mapping.Replace("||", "=");
                action_column_mapping = action_column_mapping.Replace("@@", "&");
                action_column_mapping = action_column_mapping.Replace("~~", ";");
            }

            string action_column_HTML = "";
            action_column_HTML = cookie[cookiename].Values["**action_column_HTML**"];
            if (action_column_HTML != null)
            {
                action_column_HTML = action_column_HTML.Replace("||", "=");
                action_column_HTML = action_column_HTML.Replace("@@", "&");
                action_column_HTML = action_column_HTML.Replace("~~", ";");
            }

            int[] column_width_Array; 
            string column_widths = "";
            column_widths = cookie[cookiename].Values["**column_widths**"];
            if (column_widths != null && column_widths != "")
            {
                column_width_Array = column_widths.Split('|').Select(x => int.Parse(x)).ToArray();
            }
            else
            {
                column_width_Array = null;
            }

            var columns = new List<UIGrid> { };
            UIGrid single_column;

            //int RecordID = 0;
            string strRecordID = "";
            string PlacementTitle = "";
            string Type = "";
            string Student = "";
            string Course = "";
            string Term = "";
            string Program = "";
            string StartDate = "";
            //string ActionColumn = "";
            string strAction = "";
            Boolean blnUpdate = false;
            Boolean blnDelete = false;
            Boolean blnView = false;
            Boolean blnAdd = false;

            int column_width = 0;


        using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);


                // Create a SQL command to execute the sproc

                String ConnString = ConfigurationManager.ConnectionStrings["UserDb"].ConnectionString;
                SqlConnection conn = new SqlConnection(ConnString);
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "FRM_PROC.dbo.FRM_GetIndex_rs_No_Count";
                cmd.Parameters.Add("user_name", username);
                cmd.Parameters.Add("formId", form_id);
                cmd.Parameters.Add("indexId", index_id);
                cmd.Parameters.Add("Where_Clause", where_clause);
                cmd.Parameters.Add("Order_By", null);
                cmd.Parameters.Add("page", 1);

                int column_count = 0;

                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        foreach (DataRow row in dt.Rows)
                        {
                            var obj = (IDictionary<string, object>)new ExpandoObject();
                            foreach (DataColumn column in dt.Columns)
                            {
                                // Set default column width
                                if (column_width_Array != null && column_count > 0 && column_count < column_width_Array.Length)
                                {
                                    column_width = column_width_Array[column_count];
                                }
                                else
                                {
                                    column_width = 100;
                                }

                                // Store values for Add, View, Update, and Delete to determine the Actions column
                                switch (column.ColumnName)
                                {
                                    case "Add":
                                        if (row[column.ColumnName].ToString() == "True")
                                        {
                                            blnAdd = true;
                                        }
                                        break;
                                    case "Update":
                                        if (row[column.ColumnName].ToString() == "True")
                                        {
                                            blnUpdate = true;
                                        }
                                        break;
                                    case "Delete":
                                        if (row[column.ColumnName].ToString() == "True")
                                        {
                                            blnDelete = true;
                                        }
                                        break;
                                    case "View":
                                        if (row[column.ColumnName].ToString() == "True")
                                        {
                                            blnView = true;
                                        }
                                        break;
                                    case "record_id":
                                        break;
                                    case "RecordID":
                                        strRecordID = row[column.ColumnName].ToString();
                                        break;
                                    default:
                                        obj.Add(column.ColumnName, row[column.ColumnName].ToString());

                                        if (recordsTotal == 0)
                                        {
                                            single_column = new UIGrid { field = column.ColumnName, displayName = column.ColumnName, cellTemplate = "<div class='ui-grid-row-style' ng-bind-html=\"COL_FIELD | trusted\"></div>", minWidth = 100, width = column_width };
                                            columns.Add(single_column);
                                        }
                                        break;
                                }

                                column_count++;

                            }

                            Hashtable Link_Names = new Hashtable();
                            Hashtable Link_Address = new Hashtable();

                            // Check to see if there are column mappings and set them up if there are
                            if (action_column_mapping != null && action_column_mapping != "")
                            {
                                foreach (string mapping_group in action_column_mapping.Split(new char[] { '|' }))
                                {
                                    string[] mapping = mapping_group.Split(new char[] { '^' });
                                    string link = mapping[0];
                                    string link_name = mapping[1];
                                    string link_address = mapping[2];

                                    // Only replace first and last []
                                    link = link.Substring(1, link.Length - 2);
                                    link_name = link_name.Substring(1, link_name.Length - 2);
                                    link_address = link_address.Substring(1, link_address.Length - 2);

                                    // Replace [record_id] with record_id
                                    // Replace [user_name] with username
                                    link_address = link_address.Replace("[record_id]", strRecordID);
                                    link_address = link_address.Replace("[username]", username);

                                    Link_Names.Add(link, link_name);
                                    Link_Address.Add(link, link_address);

                                }
                            }

                            // Create actions columns based on Add, View, Update, and Delete
                            // Apply columns mapping if there are any
                            strAction = "";
                            if (blnUpdate == true)
                            {
                                if (Link_Names.ContainsKey("Edit"))
                                {
                                    if (Link_Address["Edit"] != "")
                                    {
                                        strAction += "&nbsp;&nbsp;&nbsp;<a class='grid-action' href='" + Link_Address["Edit"].ToString() + "'>";
                                    }
                                    else
                                    {
                                        strAction += "&nbsp;&nbsp;&nbsp;<a class='grid-action' href='/dotnet/FormProcessor/form.aspx/index?form_id=" + form_id + "&action=edit&record_id=" + strRecordID + "&amp;" + username + "'>";
                                    }

                                    if (Link_Names["Edit"] != "")
                                    {
                                        strAction += Link_Names["Edit"].ToString() + "</a>&nbsp;&nbsp;&nbsp;";
                                    }
                                    else
                                    {
                                        strAction += "Edit</a>&nbsp;&nbsp;&nbsp;";
                                    }
                                }
                                else
                                {
                                    strAction += "&nbsp;&nbsp;&nbsp;<a class='grid-action' href='/dotnet/FormProcessor/form.aspx/index?form_id=" + form_id + "&action=edit&record_id=" + strRecordID + "&amp;" + username + "'>Edit</a>&nbsp;&nbsp;&nbsp;";
                                }
                            }

                            if (blnView == true)
                            {
                                if (Link_Names.ContainsKey("View"))
                                {
                                    if (Link_Address["View"] != "")
                                    {
                                        strAction += "&nbsp;&nbsp;&nbsp;<a class='grid-action' href='" + Link_Address["View"].ToString() + "'>";
                                    }
                                    else
                                    {
                                        strAction += "&nbsp;&nbsp;&nbsp;<a class='grid-action' href='/dotnet/FormProcessor/form.aspx/index?form_id=" + form_id + "&action=edit&record_id=" + strRecordID + "&amp;" + username + "'>";
                                    }

                                    if (Link_Names["View"] != "")
                                    {
                                        strAction += Link_Names["View"].ToString() + "</a>&nbsp;&nbsp;&nbsp;";
                                    }
                                    else
                                    {
                                        strAction += "View</a>&nbsp;&nbsp;&nbsp;";
                                    }
                                }
                                else
                                {
                                    strAction += "&nbsp;&nbsp;&nbsp;<a class='grid-action' href='/dotnet/FormProcessor/form.aspx/index?form_id=" + form_id + "&action=view&record_id=" + strRecordID + "&amp;" + username + "'>View</a>&nbsp;&nbsp;&nbsp;";
                                }
                            }

                            if (blnDelete == true)
                            {
                                if (Link_Names.ContainsKey("Delete"))
                                {
                                    if (Link_Address["Delete"] != "")
                                    {
                                        strAction += "&nbsp;&nbsp;&nbsp;<a class='grid-action' href='" + Link_Address["Delete"].ToString() + "'>";
                                    }
                                    else
                                    {
                                        strAction += "&nbsp;&nbsp;&nbsp;<a class='grid-action' href='#' onclick='delete_record(" + form_id + "," + strRecordID + ",\"" + username + "\");'";
                                    }

                                    if (Link_Names["Delete"] != "")
                                    {
                                        strAction += Link_Names["Delete"].ToString() + "</a>&nbsp;&nbsp;&nbsp;";
                                    }
                                    else
                                    {
                                        strAction += "Delete</a>&nbsp;&nbsp;&nbsp;";
                                    }
                                }
                                else
                                {
                                    strAction += "&nbsp;&nbsp;&nbsp;<a class='grid-action' href='#' onclick='delete_record(" + form_id + "," + strRecordID + ",\"" + username + "\");'>Delete</a>&nbsp;&nbsp;&nbsp;";
                                }

                            }

                            // If it exists and there is no action_column_mapping, replace with action_column_HTML 
                            if (action_column_HTML != null && action_column_HTML != "" && action_column_mapping == "")
                            {
                                // Replace [] parameters with values
                                strAction = action_column_HTML.ToString().Replace("[record_id]", strRecordID);
                                strAction = strAction.Replace("[username]", username);
                            }

                            obj.Add("Action", strAction);

                            //result.Add(obj);

                            // Reset permissions
                            blnAdd = false;
                            blnUpdate = false;
                            blnView = false;
                            blnDelete = false;

                            //ActionColumn = "<a href='http://www.cote.illinois.edu'>Link</a>";
                            //obj.Add("Action", ActionColumn);

                            if (recordsTotal == 0)
                            {
                                single_column = new UIGrid { field = "Action", exporterSuppressExport = true, displayName = "Action", cellTemplate = "<div class='ui-grid-row-style' ng-bind-html=\"COL_FIELD | trusted\"></div>", headerCellTemplate = "<div ng-style='{ height: col.headerRowHeight }' ng-repeat='col in renderedColumns' ng-class='col.colIndex()' class='ngHeaderCell' ng-header-cell></div>", minWidth = 200, width = 200 };
                                columns.Add(single_column);
                            }

                            resultsList.Add(obj);
                            recordsTotal++;

                        }
                    }
                }
                catch (Exception e)
                {
                    //Redirect to an error page
                    error_message = e.Message;
                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                    return Redirect("https://cte-s.education.illinois.edu/dotnet/FormProcessor/Error.aspx/HttpError");
                }
                finally
                {
                    db.Database.Connection.Close();
                }

            }

            return Json(new
            {
                error_message,
                recordsTotal,
                columns,
                resultsList
            });
        }
    }
}
