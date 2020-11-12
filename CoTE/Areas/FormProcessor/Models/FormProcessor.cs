using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace FormProcessor.Models
{

    public class CoTEDB : DbContext
    {
        public CoTEDB()
            : base("UserDB")
        {
        }

        public DataTable FetchPeopleData()
        {
            DataTable people = new DataTable();
            String ConnString = ConfigurationManager.ConnectionStrings["UserDb"].ConnectionString;
            //
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "WEB_PEOPLE_DISPLAY";
                    cmd.Parameters.Add("group", "STAFF");

                    // fill the parameter collection
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable schemaTable = reader.GetSchemaTable();
                        foreach (DataRow dr in schemaTable.Rows)
                        {
                            DataColumn column = new DataColumn();
                            column.Caption = dr["ColumnName"].ToString();
                            column.ColumnName = dr["ColumnName"].ToString();
                            people.Columns.Add(column);
                        }
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // create new DataRow
                                DataRow dr = people.NewRow();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    dr[i] = reader.IsDBNull(i) ? String.Empty : reader.GetValue(i).ToString();
                                }
                                people.Rows.Add(dr);
                            }
                        }
                    }
                }

            }
            return people;
        }

        /*
        public DataTable FetchIndexData()
        {
            DataTable index = new DataTable();
            String ConnString = ConfigurationManager.ConnectionStrings["UserDb"].ConnectionString;
            //
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "FRM_PROC.dbo.FRM_GetIndex_rs_No_Count";
                    cmd.Parameters.Add("user_name", "eohlsson");
                    cmd.Parameters.Add("formID", 189);
                    cmd.Parameters.Add("indexId", 165);
                    cmd.Parameters.Add("Where_Clause", "[End Date] > DATEADD(day, 10, [Start Date]) ");
                    cmd.Parameters.Add("Order_By", "term_cd desc, student");
                    cmd.Parameters.Add("page", 1);

                    // fill the parameter collection
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        index.Load(reader);

                    }
                }

            }
            return index;
        }
    */
    
    }

    public class FormProcessor
    {
    }

    public class Form_Level_Permissions
    {
        public string Frm_Level_Permissions { get; set; }
    }

    public class Form_Level_Insert_Permissions
    {
        public Boolean Frm_Level_Permissions { get; set; }
    }

    public class Record_Level_Permissions
    {
        public string RecordID { get; set; }
        public string Update { get; set; }
        public string Delete { get; set; }
        public string View { get; set; }
        public string Add { get; set; }
    }

    public class Form_Form
    {
        public int form_id { get; set; }
        public string form_name { get; set; }
        public string form_title { get; set; }
        public string development_stage { get; set; }
        public string functioning_version { get; set; }
        public bool display_anchors { get; set; }
        public bool floating_submit { get; set; }
        public bool hide_cancel { get; set; }
        public byte result_option { get; set; }
        public string result_email { get; set; }
        public string primary_table { get; set; }
        public bool anonymous_access { get; set; }
        public bool asterisks_in_required_questions { get; set; }
        public string record_name { get; set; }
        public string record_name_plural { get; set; }
        public string associated_site_name { get; set; }
        public bool sensitive_data { get; set; }
        public int? service_id { get; set; }
        public bool index_is_add_update_response { get; set; }
        public Int16? page_length { get; set; }
        public string expression { get; set; }
        public string manager_name { get; set; }
        public string manager_phone { get; set; }
        public string manager_email { get; set; }
        public int? parent_service_id { get; set; }
        public string service_status { get; set; }
        public string service_status_description { get; set; }
        public string service_name { get; set; }
        public string manager_groups { get; set; }
        public string description_public { get; set; }
        public string document_publicly { get; set; }
        public string URL_Stem { get; set; }
        public string documentation_url { get; set; }
        public bool? error_logging { get; set; }
        public string error_email_notify { get; set; }
        public string error_displayhtml_contact { get; set; }
        public int page_id { get; set; }
        public string page_page_xslt { get; set; }
        public string html_head_xslt { get; set; }
        public string html_head_additions_xslt { get; set; }
        public string menu_xslt { get; set; }
        public string header_xslt { get; set; }
        public string submit_button_text { get; set; }
        public string layout { get; set; }
        public string breadcrumb { get; set; }
        public string footer_xslt { get; set; }
        public string logo_image { get; set; }
        public string DeveloperUnitID { get; set; }
        public string ManagerUnitID { get; set; }
        public string javascript_onload { get; set; }
    }

    public class Form_Index
    {
        public int index_id { get; set; }
        public int form_id { get; set; }
        public bool? default_index { get; set; }
        public string index_menu_link_name { get; set; }
        public string index_item_xslt { get; set; }
        public string index_directions { get; set; }
        public string record_index_select { get; set; }
        public string record_index_select_fields { get; set; }
        public string record_index_where { get; set; }
        public string record_index_order_by { get; set; }
        public string expression { get; set; }
        public int? section_id { get; set; }
        public bool? default_to_emptyset { get; set; }
        public string action_column_HTML { get; set; }
        public string action_column_mapping { get; set; }
        public int? rows_per_page { get; set; }
        public string breadcrumb { get; set; }
        public string column_widths { get; set; }
    }

    public class Form_Relations
    {
        public int relation_id { get; set; }
        public int form_id { get; set; }
        public string db_name { get; set; }
        public string db_owner { get; set; }
        public string table_name { get; set; }
        public string permissions_mask { get; set; }
        public bool? soft_delete { get; set; }
        public string record_id_field { get; set; }
        public bool? record_id_auto { get; set; }
        public string record_id_insert_param { get; set; }
        public bool? one_to_many { get; set; }
        public string select_from_clause { get; set; }
        public string select_where_clause { get; set; }
        public string select_order_by_clause { get; set; }
        public string primary_key_insert_columns { get; set; }
        public string primary_key_insert_values { get; set; }
        public Int16? entity_max { get; set; }
        public Int16? entity_max_add { get; set; }
        public bool? delete_when_parent_deleted { get; set; }
        public bool? add_when_parent_added { get; set; }
        public string entity_name { get; set; }
        public string entity_name_plural { get; set; }
        public DateTime created_date { get; set; }
        public string created_by { get; set; }
        public DateTime? modified_date { get; set; }
        public string modified_by { get; set; }
    }

    public class Form_Question
    {
        public int question_idformxquestion_id { get; set; }
        public int question_id { get; set; }
        public int form_id { get; set; }
        public int? section_id { get; set; }
        public int? index_column_order { get; set; }
        public int? question_order { get; set; }
        public int? question_column { get; set; }
        public int? question_row { get; set; }
        public int? question_colspan { get; set; }
        public string question_orientation { get; set; }
        public string column_width { get; set; }
        public int? limiting_question_id { get; set; }
        public string question_name { get; set; }
        public string question { get; set; }
        public string question_trailer { get; set; }
        public string help { get; set; }
        public string data_area { get; set; }
        public string data_type { get; set; }
        public string data_location { get; set; }
        public string file_path { get; set; }
        public string file_types { get; set; }
        public bool nullable { get; set; }
        public bool validation { get; set; }
        public string input_default { get; set; }
        public string question_xslt { get; set; }
        public string input_xslt { get; set; }
        public string input_type { get; set; }
        public string radio_list_orientation { get; set; }
        public int? value_list_id { get; set; }
        public bool? value_list_allow_other { get; set; }
        public bool? value_list_allow_multiple { get; set; }
        public bool? value_list_standard_search { get; set; }
        public string value_drilldown_question_id { get; set; }
        public bool? multivalues_in_related_table { get; set; }
        public Int16? input_size { get; set; }
        public Int16? input_maxlength { get; set; }
        public Int16? input_rows { get; set; }
        public Int16? input_cols { get; set; }
        public string list_item_xslt { get; set; }
        public string question_HTML { get; set; }
        public string public_use { get; set; }
        public int? function_id { get; set; }
        public string error_xslt { get; set; }
        public string function_name { get; set; }
        public string expression { get; set; }
        public string db_name { get; set; }
        public string medium { get; set; }
        public string validation_error_xslt { get; set; }
        public string type { get; set; }
    }

    public class Form_Section
    {
        public int section_id { get; set; }
        public string section_type { get; set; }
        public int form_id { get; set; }
        public Int16 section_order { get; set; }
        public string section_xslt { get; set; }
        public string section_xslt_2nd_pass { get; set; }
        public string section_title { get; set; }
        public string section_footer_xslt { get; set; }
        public string section_header_xslt { get; set; }
        public string hide_when { get; set; }
        public int? one_to_many_relation_id { get; set; }
        public string question_html_structure { get; set; }
        public int? open_to_form { get; set; }
        public string open_to_form_url { get; set; }
        public bool? requires_data { get; set; }
    }

    public class Form_Page_Wrapper
    {
        public int page_wrapper_id { get; set; }
        public string page_action { get; set; }
        public string form_xslt { get; set; }
        public string directions_xslt { get; set; }
    }

    public class Form_Trigger
    {
        public int trigger_id { get; set; }
        public int form_id { get; set; }
        public string triggering_action { get; set; }
        public string trigger_type { get; set; }
        public string trigger_source { get; set; }
        public Int16? trigger_order { get; set; }
    }

    public class Form_Footer
    {
        public string created_by { get; set; }
        public DateTime created_date { get; set; }
        public string modified_by { get; set; }
        public DateTime modified_date { get; set; }
    }

    public class Form_Valuelists
    {
        public int question_id { get; set; }
        public string value_list_name { get; set; }
        public string null_value_name { get; set; }
        public string input_value { get; set; }
        public string input_name { get; set; }
        public string drill_down_parent_value { get; set; }
        public bool? dynamic_sql { get; set; }
    }

    public class Table_Permissions
    {
        public string result { get; set; }
    }


    public class ErrorMessage
    {
        public int intErrorCode { get; set; }
        public string strErrorMessage { get; set; }
    }

    public class Form_Data
    {
        public List<Form_Level_Permissions> PermissionsDataSet { get; set; }
        public List<Form_Form> FormDataSet { get; set; }
        public List<Form_Index> IndexDataSet { get; set; }
        public List<Form_Relations> RelationsDataSet { get; set; }
        public List<Form_Question> QuestionsDataSet { get; set; }
        public List<Form_Section> SectionsDataSet { get; set; }
        public List<Form_Page_Wrapper> WrapperDataSet { get; set; }
        public List<Form_Trigger> TriggerDataSet { get; set; }
        public List<Form_Footer> FooterDataSet { get; set; }
        public List<Form_Valuelists> ValueListDataSet { get; set; }

        public Form_Data(List<Form_Level_Permissions> d1, List<Form_Form> d2, List<Form_Index> d3, List<Form_Relations> d4, List<Form_Question> d5, List<Form_Section> d6, List<Form_Page_Wrapper> d7, List<Form_Trigger> d8, List<Form_Footer> d9, List<Form_Valuelists> d10)
        {
            this.PermissionsDataSet = d1;
            this.FormDataSet = d2;
            this.IndexDataSet = d3;
            this.RelationsDataSet = d4;
            this.QuestionsDataSet = d5;
            this.SectionsDataSet = d6;
            this.WrapperDataSet = d7;
            this.TriggerDataSet = d8;
            this.FooterDataSet = d9;
            this.ValueListDataSet = d10;
        }

    }

    public class UIGrid
    {
        public string field { get; set; }
        public string displayName { get; set; }
        public string cellTemplate { get; set; }
        public string headerCellTemplate { get; set; }
        public Boolean exporterSuppressExport { get; set; }
        public int minWidth { get; set; }
        public int width { get; set; }
    }

}