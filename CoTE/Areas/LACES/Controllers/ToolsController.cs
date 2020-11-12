using CoTE.Areas.LACES.Models;
using CoTE.Models;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IKVM;
using OfficeOpenXml;
using System.Data.OleDb;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace CoTE.Areas.LACES.Controllers
{
    public class ToolsController : Controller
    {

        private LACES_Context db = new LACES_Context();

        //
        // GET: /LACES/Tools/

        [HttpGet]
        public ActionResult TestFileUpload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TestFileUpload(IEnumerable<HttpPostedFileBase> userFiles)
        {
 
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet,Authorize]
        public ActionResult Download_EDTPA()
        {
            return View();
        }

        [HttpPost,Authorize]
        public ActionResult Download_EDTPA (FormCollection formCollection)
        {
            int record_count = 0;
            string reported_date = "";

            if(Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];
 
                if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];

                    StreamReader srContent1 = new StreamReader(file.InputStream);

                    do
                    {

                        string strContent1 = string.Empty;

                        if (srContent1.BaseStream.CanRead)
                        {
                            record_count++;

                            strContent1 = srContent1.ReadLine();

                            if (strContent1 != null)
                            {

                                string[] line = strContent1.Split('|');
                                // Line format (Last_Name,First_Name,Middle_initial,Program,Last_5_SSN,DOB,Submitted_Date,Reporting_Date,Assessment,Program,Field,Code,Score_1,Score_2,Score_3,Score_4,Score_5,Score_6,Score_7,Score_8,Score_9,Score_10,Score_11,Score_12,Score_13,Score_14,Score_15,Score_16,Score_17,Score_18,Score_19,Score_20,Score_21,edTPA_Total_Score,Average_Total_Rubric_Score,Is_Prep_Inst_Code,Customer_Number)

                                // Get cte_stu_id using [4] last 5 ssn & [5] dob
                                string cte_stu_id = "";

                                using (var db = new CoTEDB())
                                {

                                    var cmd = db.Database.Connection.CreateCommand();
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = "SELECT cte_stu_id FROM CTE.DBO.CTE_STU_DEMO WHERE birth_date = '" + line[4].ToString() + "' and right(ssn, 5) = " + line[3].ToString();

                                    try
                                    {

                                        db.Database.Connection.Open();

                                        // Run the sproc 
                                        using (var reader = cmd.ExecuteReader())
                                        {

                                            while (reader.Read())
                                            {
                                                cte_stu_id = reader[0].ToString();
                                            }
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

                                // Create string using (zero based string)
                                // [5] submitted date
                                // [6] reported date
                                // [8] field code
                                // [9] - [29] scores
                                // [30] total score
                                // [31] average total rubric score
                                // [33] customer number
                                // [38] field specialty
                                // If cte_stu_id is null then insert as 0, need to check for 0 in cte_stu_id after completion
                                if (cte_stu_id == "")
                                {
                                    cte_stu_id = "0";
                                }

                                string sql_statement = " Insert into cte.dbo.CTE_STU_EVAL_EDTPA_PEARSON ([cte_stu_id],[submitted_date],[reported_date],[field_code],[score_1],[score_2],[score_3],[score_4],[score_5],[score_6],[score_7],[score_8],[score_9],[score_10],[score_11],[score_12],[score_13],[score_14],[score_15],[score_16],[score_17],[score_18],[score_19],[score_20],[score_21],[total_score],[average_rubric_score],[customer_number],[field_specialty]) VALUES (" + cte_stu_id + ",";

                                for (int i = 0; i < line.Length; i++)
                                {
                                    if (i == 8 || (i >= 9 && i <= 31))
                                    {
                                        if (line[i] == "")
                                        {
                                            sql_statement += "'0', ";
                                        }
                                        else
                                        {
                                            sql_statement += "'" + line[i].ToString() + "', ";
                                        }
                                    }
                                    else if (i == 5)
                                    {
                                        sql_statement += "'" + line[i].ToString() + "', ";
                                    }
                                    else if (i == 6)
                                    {
                                        reported_date = line[i].ToString();
                                        sql_statement += "'" + line[i].ToString() + "', ";
                                    }
                                    else if (i == 33)
                                    {
                                        sql_statement += "'" + line[i].ToString() + "', ";
                                    }
                                    else if (i == 38)
                                    {
                                        sql_statement += "'" + line[i].ToString() + "')";
                                    }

                                }

                                // Run SQL
                                using (var db = new CoTEDB())
                                {
                                    db.Database.Initialize(force: false);

                                    // Check for Insert permissions
                                    var cmd = db.Database.Connection.CreateCommand();
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = sql_statement;

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
                                }
                            }

                        }

                    } while (srContent1.BaseStream.CanRead && srContent1.EndOfStream == false);
 
                }
            }

            using (var db = new CoTEDB())
            {
                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[edTPA_Reports]";
                cmd.Parameters.Add(new SqlParameter("@test_date", reported_date));

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

            ViewBag.record_count = record_count;

            return View();
        }

        [HttpGet, Authorize]
        public ActionResult Download_NES()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult Download_NES(FormCollection formCollection)
        {
            int record_count = 0;
            string error_string = "";
            string admin_date = "";


            //**************************
            //*** LOAD SCORE REPORTS
            //**************************
            HttpPostedFileBase file = Request.Files["UploadedTestScoreFile"];

            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string fileName = file.FileName;
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];

                StreamReader srContent1 = new StreamReader(file.InputStream);

                do
                {

                    string strContent1 = string.Empty;
                    string insert_statement = "";
                    int subscore_count = 0;

                    if (srContent1.BaseStream.CanRead)
                    {
                        record_count++;

                        strContent1 = srContent1.ReadLine();

                        if (strContent1 != null)
                        {

                            string sql_statement = " INSERT INTO cte.dbo.cte_stu_nes_test (ssn, admin_date, test_code, status, scaled_score, scores_1, scores_2, scores_3, scores_4, scores_5, scores_6, scores_7, created_date, created_by) VALUES (";

                            // Create string using
                            // [0,9] ssn
                            insert_statement = sql_statement + strContent1.Substring(0, 9);

                            // [68,10] admin date
                            insert_statement = insert_statement + ", '" + strContent1.Substring(68, 10) + "'";
                            admin_date = strContent1.Substring(68, 10);

                            // [80,3] test code
                            insert_statement = insert_statement + "," + strContent1.Substring(80, 3) + " ";

                            // [85,1] status (P/F)
                            insert_statement = insert_statement + ",'" + strContent1.Substring(85, 1) + "'";

                            // [88,3] scaled score
                            if (strContent1.TrimEnd().Length > 88)
                            {
                                insert_statement = insert_statement + "," + strContent1.Substring(88, 3) + "";
                            }
                            else
                            {
                                insert_statement = insert_statement + ", null";
                            }

                            // [93,1] number of subscores
                            if (strContent1.TrimEnd().Length > 88)
                            {
                                subscore_count = Convert.ToInt32(strContent1.Substring(93, 1), 10);
                            }
                            else
                            {
                                subscore_count = 0;
                            }

                            // [95, 3] subscore 1
                            for (int i = 1; i <= 7; i++)
                            {
                                if (i <= subscore_count)
                                {
                                    insert_statement = insert_statement + ", " + strContent1.Substring(91 + (i * 3) + i, 3);
                                }
                                else
                                {
                                    insert_statement = insert_statement + ", null";
                                }
                            }

                            insert_statement = insert_statement + ", '" + DateTime.Now.ToShortDateString() + "','NES Import')";

                            // Run SQL
                            using (var db = new CoTEDB())
                            {
                                db.Database.Initialize(force: false);

                                // Check for Insert permissions
                                var cmd = db.Database.Connection.CreateCommand();
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = insert_statement;

                                try
                                {

                                    db.Database.Connection.Open();

                                    // Run the sproc 
                                    var reader = cmd.ExecuteReader();

                                }
                                catch (System.Data.SqlClient.SqlException e)
                                {
                                    // Catch duplicate records and display message
                                    if (e.Number == 2627)
                                    {
                                        error_string = error_string + strContent1 + "<br>";
                                        record_count--;
                                    }
                                    else
                                    {
                                        //Redirect to an error page
                                        Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                                        return Redirect("~/FormProcessor/Error.aspx/HttpError");

                                    }
                                }
                                finally
                                {
                                    db.Database.Connection.Close();
                                }
                            }
                        }

                    }

                } while (srContent1.BaseStream.CanRead && srContent1.EndOfStream == false);

            }

            //*****************************
            //*** LOAD STATEWIDE RESULTS
            //*****************************
            HttpPostedFileBase statewide_file = Request.Files["UploadedStatewideScoreFile"];
 
            if ((statewide_file != null) && (statewide_file.ContentLength > 0) && !string.IsNullOrEmpty(statewide_file.FileName))
            {

                String strPDF = "";
                String strTestingInformation = "";
                String strTestDate = "";
                String intTestNumber = "";
                String intNumberOfExaminees = "";
                String intNumberPassing = "";
                String intNumberNotPassing = "";
                String intStateScaled = "";
                String intStateSub1 = "";
                String intStateSub2 = "";
                String intStateSub3 = "";
                String intStateSub4 = "";
                String intStateSub5 = "";
                String intStateSub6 = "";
                String intStateSub7 = "";

                //PDDocument doc = PDDocument.load("u:/nes/" + statewide_file.FileName);
                PDDocument doc = PDDocument.load(new ikvm.io.InputStreamWrapper(statewide_file.InputStream));
                PDFTextStripper pdfStripper = new PDFTextStripper();
                strPDF = pdfStripper.getText(doc);

                if (strPDF.Contains("Tests Taken Through:"))
                {
                    //strTestDate = CDate(Mid(strPDF, InStr(1, strPDF, "Tests Taken Through:") + 20, InStr(1, strPDF, "Illinois Licensure Testing System") - InStr(1, strPDF, "Tests Taken Through:") - 20))
                    strTestDate = strPDF.Substring(strPDF.IndexOf("Tests Taken Through:") + 20, strPDF.IndexOf("Illinois Licensure Testing System") - strPDF.IndexOf("Tests Taken Through:") - 20);
                }
                else if (strPDF.IndexOf("Test Date:") > 0)
                {
                    //strTestDate = CDate(Mid(strPDF, InStr(1, strPDF, "Test Date:") + 10, InStr(1, strPDF, "Illinois Licensure Testing System") - InStr(1, strPDF, "Test Date:") - 10))
                    strTestDate = strPDF.Substring(strPDF.IndexOf("Test Date:") + 10, strPDF.IndexOf("Illinois Licensure Testing System") - strPDF.IndexOf("Test Date:") - 10);
                }
                else if (strPDF.IndexOf("Test Administration Period:") > 0)
                {
                    //strTestDate = Mid(strPDF, InStr(1, strPDF, "Test Administration Period:") + 27, InStr(1, strPDF, "Illinois Licensure Testing System") - InStr(1, strPDF, "Test Administration Period:") - 27)
                    strTestDate = strPDF.Substring(strPDF.IndexOf("Test Administration Period:") + 27, strPDF.IndexOf("Illinois Licensure Testing System") - strPDF.IndexOf("Test Administration Period:") - 27);

                    //strTestDate = CDate(Mid(strTestDate, 1, InStr(1, strTestDate, "-") - 1) & Mid(strTestDate, InStr(1, strTestDate, ","), Len(strTestDate) - InStr(1, strTestDate, ",")))
                    strTestDate = strTestDate.Substring(strTestDate.IndexOf("-") - 1).ToString() + strTestDate.Substring(strTestDate.IndexOf(",") - 1, strTestDate.Length - strTestDate.IndexOf(",") - 1);
                }

                // Fix strTestDate
                strTestDate = strTestDate.Replace("\r\n", "");

                //Reset everything
                intStateScaled = "NULL";
                intStateSub1 = "NULL";
                intStateSub2 = "NULL";
                intStateSub3 = "NULL";
                intStateSub4 = "NULL";
                intStateSub5 = "NULL";
                intStateSub6 = "NULL";
                intStateSub7 = "NULL";
                intNumberOfExaminees = "NULL";
                intNumberPassing = "NULL";
                intNumberNotPassing = "NULL";

                strTestingInformation = strPDF.Substring(strPDF.IndexOf("Statewide ") , strPDF.Length - strPDF.IndexOf("Statewide "));
    
                while (strTestingInformation.IndexOf("Statewide ", 1) > 0) 
                {               
 
                    String strStatewide = "";
                    String strSQL = "";

                    //Get test number
                    //intTestNumber = Mid(strTestingInformation, InStr(strTestingInformation, "Test:") + 6, 3)
                    intTestNumber = strTestingInformation.Substring(strTestingInformation.IndexOf("TEST:") + 6, 3);

                    //Get statewide information
                    //strStatewide = Mid(strTestingInformation, 11, InStr(1, strTestingInformation, "Test:") - 11)
                    strStatewide = strTestingInformation.Substring(10, strTestingInformation.IndexOf("TEST:") - 10);
                    strStatewide = strStatewide.Replace(Environment.NewLine, " ");

                    if (strStatewide.IndexOf("Subarea") > 0)
                    {
                        strStatewide = strStatewide.Substring(1, strStatewide.IndexOf("Subarea") - 1);
                    }
                    else if (strStatewide.IndexOf("Standard") > 0)
                    {
                        strStatewide = strStatewide.Substring(1, strStatewide.IndexOf("Standard") - 1);
                    }
                    else if (strStatewide.IndexOf("Objective") > 0)
                    {
                        strStatewide = strStatewide.Substring(1, strStatewide.IndexOf("Objective") - 1);
                    }
                    else if (strStatewide.IndexOf("Page") > 0)
                    {
                        strStatewide = strStatewide.Substring(1, strStatewide.IndexOf("Page") - 1);
                    }
          
                    //MsgBox "Test:" + intTestNumber + " - " + strStatewide

                    intNumberOfExaminees = strStatewide.Substring(0, strStatewide.IndexOf(" ") + 1);
                    strStatewide = strStatewide.Substring(strStatewide.IndexOf(" "), strStatewide.Length - strStatewide.IndexOf(" "));
                    intNumberPassing = strStatewide.Substring(0, strStatewide.IndexOf("("));

                    strStatewide = strStatewide.Substring(strStatewide.IndexOf(")") + 2, strStatewide.Length - strStatewide.IndexOf(")") - 2);
                    intNumberNotPassing = strStatewide.Substring(0, strStatewide.IndexOf("("));
                    strStatewide = strStatewide.Substring(strStatewide.IndexOf(")") + 2, strStatewide.Length - strStatewide.IndexOf(")") - 2);

                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateScaled = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateScaled = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateScaled = "NULL";
                    }
                    
                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub1 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub1 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub1 = "NULL";
                    }
        
                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub2 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();

                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub2 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub2 = "NULL";
                    }
        
                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub3 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub3 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub3 = "NULL";
                    }
        
                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub4 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub4 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub4 = "NULL";
                    }
        
                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub5 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub5 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub5 = "NULL";
                    }
        
                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub6 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub6 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub6 = "NULL";
                    }
        
                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub7 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub7 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub7 = "NULL";
                    }
        
                    strSQL = "Insert into cte_stu_nes_test_statewide (admin_date, test_code, scaled_score, scores_1,scores_2, scores_3, scores_4, scores_5,scores_6, scores_7, num_takers, num_passed, num_failed, created_by, created_date) values ('" + strTestDate + "', '" + intTestNumber + "', " + 
                        intStateScaled + ", " +
                        intStateSub1 + ", " +
                        intStateSub2 + ", " +
                        intStateSub3 + ", " +
                        intStateSub4 + ", " +
                        intStateSub5 + ", " +
                        intStateSub6 + ", " +
                        intStateSub7 + ", " +
                        intNumberOfExaminees + ", " +
                        intNumberPassing + ", " +
                        intNumberNotPassing + ", " +
                        "'PDF Import'," +
                        "'" + DateTime.Now + "'" +
                        ");";

                    // Run SQL
                    using (var db = new CoTEDB())
                    {
                        db.Database.Initialize(force: false);

                        // Check for Insert permissions
                        var cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = strSQL;

                        try
                        {

                            db.Database.Connection.Open();

                            // Run the sproc 
                            var reader = cmd.ExecuteReader();

                        }
                        catch (System.Data.SqlClient.SqlException e)
                        {
                            // Catch duplicate records and display message
                            if (e.Number == 2627)
                            {
                                error_string = error_string + strSQL + "<br>";
                                record_count--;
                            }
                            else
                            {
                                //Redirect to an error page
                                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                                return Redirect("~/FormProcessor/Error.aspx/HttpError");

                            }
                        }
                        finally
                        {
                            db.Database.Connection.Close();
                        }
                    } 
   
                    intStateScaled = "NULL";
                    intStateSub1 = "NULL";
                    intStateSub2 = "NULL";
                    intStateSub3 = "NULL";
                    intStateSub4 = "NULL";
                    intStateSub5 = "NULL";
                    intStateSub6 = "NULL";
                    intStateSub7 = "NULL";
                    intNumberOfExaminees = "NULL";
                    intNumberPassing = "NULL";
                    intNumberNotPassing = "NULL";
        
                    //Truncate strTestingInformation String
                    strTestingInformation = strTestingInformation.Substring(10);
                    strTestingInformation = strTestingInformation.Substring(strTestingInformation.IndexOf("Statewide ", 10), strTestingInformation.Length - strTestingInformation.IndexOf("Statewide ", 10));
                }

                // Results File
                // FileInfo f = new FileInfo("u:/nes/" + file.FileName);
                //f.CopyTo("u:/nes/NES.txt.imported." + admin_date.Replace("/", "_"));

                //f.Delete();

                // Statewide File
                //FileInfo f2 = new FileInfo("u:/nes/" + statewide_file.FileName);
                //f2.CopyTo("u:/nes/testing_report_" + admin_date.Replace("/", "_") + "_original.pdf");

                //f2.Delete();

                //MsgBox "PDF Import Completed"
      
            }

            using (var db = new CoTEDB())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[NES_Reports]";
                cmd.Parameters.Add(new SqlParameter("@test_date", admin_date));

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

            ViewBag.record_count = record_count;
            ViewBag.error_string = error_string;

            return View();
        }

        public ActionResult cbc_email()
        {
            string strRecord_Id = HttpContext.Request["record_id"];
            string strSend_Email = HttpContext.Request["send_email"];
            string username = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            using (var db = new CoTEDB())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[cert_cbc_email]";
                cmd.Parameters.Add(new SqlParameter("@cbc_id", strRecord_Id));
                cmd.Parameters.Add(new SqlParameter("@send_email", strSend_Email));

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

            return Redirect("/dotnet/formprocessor/index.aspx/index?form_id=359&index_id=502");
        }

        public ActionResult Jobs(string action_type_id)
        {

            string username = "";
            string action_name = "";
            string action_type = "";
            string message = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[cert_actions]";
                cmd.Parameters.Add(new SqlParameter("@action_type_id", action_type_id));

                try
                {

                    db.Database.Connection.Open();

                    // Get data
                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            action_type = reader[0].ToString();
                            action_name = reader[1].ToString();
                            message = reader[2].ToString();
                        }
                    }

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

            ViewBag.action_type = action_type;
            ViewBag.action_name = action_name;
            ViewBag.message = message;
            
            return View();

        }

        public ActionResult resend_TFW()
        {
            string strRecord_Id = HttpContext.Request["record_id"];
            string username = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            using (var db = new CoTEDB())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[tuition_fee_waivers_earned]";
                cmd.Parameters.Add(new SqlParameter("@waiver_id", strRecord_Id));

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

            return Redirect("/dotnet/formprocessor/index.aspx/index?form_id=363&index_id=506");
        }

        [HttpGet]
        public ActionResult AssignSSN()
        {
            string username = "";

            Database.SetInitializer<LACES_Context>(null);

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string cookie_username = cookie.Value;

                // Check if Authenticated_user is set if so use it
                if (cookie_username != "")
                {
                    username = cookie_username;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }
            }
            else
            {
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.message = "";

            return View();
        }

        [HttpPost]
        public ActionResult AssignSSN(Student Student, FormCollection formCollection)
        {
            using (LACES_Context db = new LACES_Context())
            {

                if (ModelState.IsValid)
                {
                    db.Entry(Student).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

            }

            ViewBag.message = "SSN " + Student.ssn.ToString() + " assigned to Student";

            return View();
        }

        public ActionResult send_application_reminders()
        {
            string strTerm_Id = HttpContext.Request["term_id"];
            string username = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            using (var db = new CoTEDB())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[sp_CTE_Stus_Not_Applied]";
                cmd.Parameters.Add(new SqlParameter("@term_id", strTerm_Id));

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

            ViewBag.message = "Emails Sent";

            return View();
        }

        public ActionResult email_merge()
        {
            string strMergeID = HttpContext.Request["merge_id"];
            string strBatchID = "Dataset_" + strMergeID + "_" + DateTime.Now.ToString("yyyy_MM_dd_HHmm_fff");
            string username = "";
            string send_to_field = "";
            string profile = "";
            string subject = "";
            string recipient = "";
            string cc = "";
            string bcc = "";
            string letter = "";
            string filtered_dataset = "";
            int letter_count = 0;
            bool testing = false;
            int testing_count = 0;
            string testing_email = "";
            string error_message = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.MergeID = strMergeID;
            ViewBag.BatchID = strBatchID;

            using (var db = new CoTEDB())
            {
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CTE.dbo.email_merge_get";
                cmd.Parameters.Add(new SqlParameter("@mail_merge_id", strMergeID));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // first recordset, get the recipient
                    using (reader)
                    {

                        while (reader.Read())
                        {
                            send_to_field = (string)reader["send_to_field"].ToString() ?? "";
                            cc = (string)reader["cc"].ToString() ?? "";
                            bcc = (string)reader["bcc"].ToString() ?? "";
                            profile = (string)reader["profile"].ToString() ?? "";
                            subject = (string)reader["subject"].ToString() ?? "";
                            testing = (bool)reader["testing"];
                            testing_email = (string)reader["testing_email"] ?? "";

                            if (reader["testing_email_count"].ToString() == "")
                            {
                                testing_count = 99999;
                            }
                            else
                            {
                                testing_count = (int)reader["testing_email_count"];
                            }

                            if (profile == "")
                            {
                                profile = "CTE";
                            }
                        }

                        // Move to second result set and get the letter
                        reader.NextResult();

                        while (reader.Read())
                        {
                            letter = reader[0].ToString();
                        }

                        // Move to third result set and get the fields
                        reader.NextResult();

                        var field_in = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<email_merge_fields>(reader);
                        List<email_merge_fields> fields = field_in.ToList();

                        //Dictionary<string, string> fields = new Dictionary<string, string>();
                        //while (reader.Read())
                        //{
                        //    fields.Add("[" + reader["field_name_letter"].ToString().ToUpper() + "]", reader["field_name_db"].ToString());
                        //}

                        // Move to the fourth set and get the dataset sql for group processing later
                        reader.NextResult();

                        while (reader.Read())
                        {
                            filtered_dataset = reader[0].ToString();
                        }


                        // Move to fifth result set and start processing results
                        reader.NextResult();

                        // Loop on dataset results
                        string letter_filled;
                        string field;
                        string db_field;
                        bool done_testing = false;

                        while (reader.Read() && done_testing == false && error_message == "")
                        {

                            // Reset letter
                            letter_filled = letter;

                            // Loop while still []
                            while (letter_filled.Contains("[") && letter_filled.Contains("]") && error_message == "")
                            {
                                db_field = "";

                                // Find the field in the letter
                                field = letter_filled.Substring(letter_filled.IndexOf("["), letter_filled.IndexOf("]") - letter_filled.IndexOf("[") + 1);

                                // Find it in dictionary object
                                //db_field = fields[field.ToUpper()].ToString();
                                foreach (var db_field_find in fields.Where(dbfield => dbfield.field_name_letter.ToUpper() == field.ToUpper().Replace("[", "").Replace("]", "").ToString()))
                                {
                                    db_field = db_field_find.field_name_db.ToString();

                                    // Replace fields with data

                                    // Check to see if it is a "group field"
                                    if (db_field_find.group_by.ToString() == "True")
                                    {
                                        string group_values = "";

                                        // Loop on group_key to create group_values 
                                        foreach (var group_value in fields.Where(dbfield => dbfield.field_name_letter.ToUpper() == field.ToUpper().Replace("[", "").Replace("]", "").ToString()))
                                        {
                                            string group_sql = filtered_dataset;
                                            group_values = "";

                                            if (group_sql.ToString().ToUpper().Contains("WHERE"))
                                            {
                                                group_sql = group_sql + " and " + group_value.group_key + " = '" + reader[group_value.group_key].ToString() + "'";
                                            }
                                            else
                                            {
                                                group_sql = group_sql + " where " + group_value.group_key + " = '" + reader[group_value.group_key].ToString() + "'";
                                            }

                                            //Remove "select *"
                                            group_sql = group_sql.ToString().ToUpper().Replace("SELECT *", "isnull(" + group_value.field_name_db + ", '')") + " order by " + group_value.field_name_db;

                                            //group_values += db_field_find.delimeter.ToString() + group_value.ToString();

                                            using (var db2 = new CoTEDB())
                                            {
                                                db2.Database.Initialize(force: false);

                                                try
                                                {

                                                    db2.Database.Connection.Open();

                                                    var cmd2 = db2.Database.Connection.CreateCommand();

                                                    cmd2.CommandType = CommandType.StoredProcedure;
                                                    cmd2.CommandText = "CTE.dbo.email_merge_grouping";
                                                    cmd2.Parameters.Add(new SqlParameter("@SQLString", group_sql));
                                                    cmd2.Parameters.Add(new SqlParameter("@delimiter", db_field_find.delimiter.ToString()));

                                                    var reader2 = cmd2.ExecuteReader();

                                                    while (reader2.Read())
                                                    {
                                                        group_values = reader2[0].ToString();
                                                    }

                                                    reader2.Close();
                                                    cmd2.Parameters.Clear();
                                                }
                                                catch (Exception e)
                                                {
                                                    //Redirect to an error page
                                                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                                                }
                                                finally
                                                {
                                                    db2.Database.Connection.Close();
                                                }

                                            }
                                        }

                                        letter_filled = letter_filled.Replace(field.ToLower(), group_values.ToString());

                                    }
                                    else
                                    {
                                        letter_filled = letter_filled.Replace(field.ToLower(), reader[db_field.ToLower()].ToString());
                                    }

                                }

                                if (db_field == "")
                                {
                                    error_message = "Field " + field + " not found in field list.";
                                }

                            }

                            // Get recipient
                            if (!testing)
                            {
                                recipient = reader[send_to_field].ToString();
                            }
                            else
                            {
                                recipient = testing_email;
                            }

                            if (error_message == "")
                            {
                                // Send emails via stored procedure - pass in (to, subject, profile, lettertext)
                                using (var db2 = new CoTEDB())
                                {
                                    db2.Database.Initialize(force: false);

                                    try
                                    {

                                        db2.Database.Connection.Open();

                                        var cmd2 = db2.Database.Connection.CreateCommand();

                                        cmd2.CommandType = CommandType.StoredProcedure;
                                        cmd2.CommandText = "CTE.dbo.email_merge_send";
                                        cmd2.Parameters.Add(new SqlParameter("@email_merge_id", strMergeID));
                                        cmd2.Parameters.Add(new SqlParameter("@batch_id", strBatchID));
                                        //cmd2.Parameters.Add(new SqlParameter("@recipient", "eohlsson@illinois.edu"));
                                        cmd2.Parameters.Add(new SqlParameter("@recipient", recipient));
                                        cmd2.Parameters.Add(new SqlParameter("@cc", cc));
                                        cmd2.Parameters.Add(new SqlParameter("@bcc", bcc));
                                        cmd2.Parameters.Add(new SqlParameter("@profile", profile));
                                        cmd2.Parameters.Add(new SqlParameter("@subject", subject));
                                        cmd2.Parameters.Add(new SqlParameter("@tableHTML", letter_filled));
                                        cmd2.Parameters.Add(new SqlParameter("@testing", testing));

                                        var reader2 = cmd2.ExecuteReader();

                                        reader2.Close();
                                        cmd2.Parameters.Clear();

                                        letter_count++;
                                    }
                                    catch (Exception e)
                                    {
                                        //Redirect to an error page
                                        Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                                    }
                                    finally
                                    {
                                        db2.Database.Connection.Close();
                                    }
                                }

                                if (letter_count == testing_count && testing)
                                {
                                    done_testing = true;
                                }

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

            ViewBag.batch_id = strBatchID;
            ViewBag.testing = testing;
            ViewBag.error_message = error_message;

            return View();
        }

        [HttpGet]
        public ActionResult OffenderChecks()
        {
            string username = "";

            Database.SetInitializer<LACES_Context>(null);

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string cookie_username = cookie.Value;

                // Check if Authenticated_user is set if so use it
                if (cookie_username != "")
                {
                    username = cookie_username;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }
            }
            else
            {
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.message = "";

            return View();
        }

        [HttpPost]
        public ActionResult OffenderChecks(FormCollection formCollection)
        {
            string key_name = "";
            string sql_string = "";
            int update_count = 0;

            for (int i = 0; i < formCollection.Count; i++)
            {
                key_name = "checkbox_" + formCollection[i];
                if (!string.IsNullOrEmpty(formCollection[key_name]) && key_name != "Submit")
                {
                    sql_string = "Update cte.dbo.cte_stu_demo set offender_db_checked = 1 where cte_stu_id = " + formCollection[key_name];

                    // Run SQL
                    using (var db = new CoTEDB())
                    {
                        db.Database.Initialize(force: false);

                        // Check for Insert permissions
                        var cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql_string;

                        try
                        {
 
                            db.Database.Connection.Open();

                            // Run the sproc 
                            var reader = cmd.ExecuteReader();

                        }
                        catch (System.Data.SqlClient.SqlException e)
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

                    update_count++;

                }
            }

            ViewBag.message = "<br/>" + update_count.ToString() + " records updated. Click <a href='/dotnet/laces/laces.aspx'>here</a> to return to the LACES menu<br/>";

            return View();
        }

        [HttpGet]
        public ActionResult MandatedReporter()
        {
            string username = "";

            Database.SetInitializer<LACES_Context>(null);

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string cookie_username = cookie.Value;

                // Check if Authenticated_user is set if so use it
                if (cookie_username != "")
                {
                    username = cookie_username;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.message = "";

            return View();
        }

        [HttpPost]
        public ActionResult MandatedReporter(FormCollection formCollection)
        {
            string key_name = "";
            string sql_string = "";
            int update_count = 0;

            for (int i = 0; i < formCollection.Count; i++)
            {
                key_name = "checkbox_" + formCollection[i];
                if (!string.IsNullOrEmpty(formCollection[key_name]) && key_name != "Submit")
                {
                    sql_string = "Update cte.dbo.cte_stu_Mandated_reporter set documents_received = 1 where cte_stu_id = " + formCollection[key_name];

                    // Run SQL
                    using (var db = new CoTEDB())
                    {
                        db.Database.Initialize(force: false);

                        // Check for Insert permissions
                        var cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql_string;

                        try
                        {

                            db.Database.Connection.Open();

                            // Run the sproc 
                            var reader = cmd.ExecuteReader();

                        }
                        catch (System.Data.SqlClient.SqlException e)
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

                    update_count++;

                }
            }

            ViewBag.message = "<br/>" + update_count.ToString() + " records updated. Click <a href='/dotnet/laces/laces.aspx'>here</a> to return to the LACES menu<br/>";

            return View();
        }

        [HttpGet]
        public ActionResult CreateBulkAssignments()
        {
            string username = "";

            Database.SetInitializer<LACES_Context>(null);

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string cookie_username = cookie.Value;

                // Check if Authenticated_user is set if so use it
                if (cookie_username != "")
                {
                    username = cookie_username;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.message = "";
            ViewBag.username = username;

            return View();
        }

        [HttpPost]
        public ActionResult CreateBulkAssignments(FormCollection formCollection)
        {
            string key_name = "";
            string sql_string = "";
            int update_count = 0;

            string username = "";

            Database.SetInitializer<LACES_Context>(null);

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string cookie_username = cookie.Value;

                // Check if Authenticated_user is set if so use it
                if (cookie_username != "")
                {
                    username = cookie_username;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            // If it is called from the filter set the filter
            if (formCollection["filter"] == "Filter")
            {
                if (!string.IsNullOrEmpty(formCollection["term_id"]))
                {
                    ViewBag.term_id = formCollection["term_id"].ToString();
                }
                if (!string.IsNullOrEmpty(formCollection["program_id"]))
                {
                    ViewBag.program_id = formCollection["program_id"].ToString();
                }
                if (!string.IsNullOrEmpty(formCollection["course_x_prog_id"]))
                {
                    ViewBag.course_x_prog_id = formCollection["course_x_prog_id"].ToString();
                }
                ViewBag.username = username;
            }
            else
            {
                string personnel_role_id = (string)formCollection["personnel_role_id"] ?? "";
                string start_date = (string)HttpContext.Request["start_date"] ?? "";
                string end_date = (string)HttpContext.Request["end_date"] ?? "";
                string grade_level = (string)HttpContext.Request["grade_level"] ?? "";

                for (int i = 0; i < formCollection.Count; i++)
                {
                    key_name = "placement_id_" + i.ToString();
                    if (!string.IsNullOrEmpty(formCollection[key_name]) && key_name != "Submit")
                    {
                        sql_string = "INSERT INTO ce_assignments (placement_id, assgn_name, personnel_role_id, start_date, end_date, grade_level, created_date, created_by) " +
                            " VALUES (" + formCollection[key_name] + ", 'Bulk Assignment', " + personnel_role_id + ", '" + start_date + "', '" + end_date + "', " + grade_level + ", getdate(), 'Bulk_Insert_' + convert(varchar(50), getdate(), 121))";

                        // Run SQL
                        using (var db = new CoTEDB())
                        {
                            db.Database.Initialize(force: false);

                            // Check for Insert permissions
                            var cmd = db.Database.Connection.CreateCommand();
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sql_string;

                            try
                            {

                                db.Database.Connection.Open();

                                // Run the sproc 
                                var reader = cmd.ExecuteReader();

                            }
                            catch (System.Data.SqlClient.SqlException e)
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

                        update_count++;

                    }
                }

                ViewBag.message = "<br/>" + update_count.ToString() + " records updated. Click <a href='/dotnet/laces/laces.aspx'>here</a> to return to the LACES menu<br/>";
            }                    

            return View();
        }

        public ActionResult PDU_Create()
        {
            string username = "";
            //string pdu_id = "";
            string pdu_url = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                username = cookie.Value;
            }
            else
            {
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            using (var db = new CoTEDB())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.CommandText = "[dbo].[PDU_create]";
                cmd.CommandText = "[dbo].[PDU_create_narrative]";
                cmd.Parameters.Add(new SqlParameter("@username", username));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            //pdu_id = reader[0].ToString();
                            pdu_url = reader[0].ToString();
                        }
                    }
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

            //return Redirect("/dotnet/FormProcessor/form.aspx/index?form_id=1420&action=edit&record_id=" + pdu_id + "&" + username);
            return Redirect(pdu_url);
        }

        [HttpGet]
        public ActionResult PDU_Choose()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PDU_Choose(FormCollection formCollection)
        {
            string username = "";
            string choice = "";
            string pdu_url = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                username = cookie.Value;
            }
            else
            {
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            choice = formCollection["choice"];

            using (var db = new CoTEDB())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.CommandText = "[dbo].[PDU_create]";
                cmd.CommandText = "[dbo].[PDU_create_narrative]";
                cmd.Parameters.Add(new SqlParameter("@username", username));
                cmd.Parameters.Add(new SqlParameter("@choice", choice));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            //pdu_id = reader[0].ToString();
                            pdu_url = reader[0].ToString();
                        }
                    }
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

            //return Redirect("/dotnet/FormProcessor/form.aspx/index?form_id=1420&action=edit&record_id=" + pdu_id + "&" + username);
            return Redirect(pdu_url);

        }

        public ActionResult PDU_Edit(string form_action, string pdu_id)
        {
            string username = "";
            //string pdu_id = "";
            string pdu_url = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                username = cookie.Value;
            }
            else
            {
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            using (var db = new CoTEDB())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.CommandText = "[dbo].[PDU_create]";
                cmd.CommandText = "[dbo].[PDU_edit_narrative]";
                cmd.Parameters.Add(new SqlParameter("@username", username));
                cmd.Parameters.Add(new SqlParameter("@form_action", form_action));
                cmd.Parameters.Add(new SqlParameter("@pdu_id", pdu_id));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            pdu_url = reader[0].ToString();
                        }
                    }
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

            return Redirect(pdu_url);
        }

        public ActionResult PDH_Evaluation_Create(string event_id)
        {
            return Redirect("/dotnet/FormProcessor/form.aspx/index?form_id=1447&action=add&record_id=-1&param1=" + event_id + "&calling_id=" + event_id);
        }

        [HttpGet]
        public ActionResult StudentTeachingAffidavit()
        {
            string username = "";

            Database.SetInitializer<LACES_Context>(null);

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string cookie_username = cookie.Value;

                // Check if Authenticated_user is set if so use it
                if (cookie_username != "")
                {
                    username = cookie_username;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.message = "";

            return View();
        }

        [HttpPost]
        public ActionResult StudentTeachingAffidavit(FormCollection formCollection)
        {
            string key_name = "";
            string sql_string = "";
            int update_count = 0;

            for (int i = 0; i < formCollection.Count; i++)
            {
                key_name = "checkbox_" + formCollection[i];
                if (!string.IsNullOrEmpty(formCollection[key_name]) && key_name != "Submit")
                {
                    sql_string = "Update cte.dbo.ce_assignments set affidavit = 1 where assignment_id = " + formCollection[key_name];

                    // Run SQL
                    using (var db = new CoTEDB())
                    {
                        db.Database.Initialize(force: false);

                        // Check for Insert permissions
                        var cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql_string;

                        try
                        {

                            db.Database.Connection.Open();

                            // Run the sproc 
                            var reader = cmd.ExecuteReader();

                        }
                        catch (System.Data.SqlClient.SqlException e)
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

                    update_count++;

                }
            }

            ViewBag.message = "<br/>" + update_count.ToString() + " records updated. Click <a href='/dotnet/laces/laces.aspx'>here</a> to return to the LACES menu<br/>";

            return View();
        }

        [HttpGet]
        public ActionResult ELISAccountCreated()
        {
            string username = "";

            Database.SetInitializer<LACES_Context>(null);

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string cookie_username = cookie.Value;

                // Check if Authenticated_user is set if so use it
                if (cookie_username != "")
                {
                    username = cookie_username;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.message = "";

            return View();
        }

        [HttpPost]
        public ActionResult ELISAccountCreated(FormCollection formCollection)
        {
            string key_name = "";
            string sql_string = "";
            int update_count = 0;

            for (int i = 0; i < formCollection.Count; i++)
            {
                key_name = "checkbox_" + formCollection[i];
                if (!string.IsNullOrEmpty(formCollection[key_name]) && key_name != "Submit")
                {
                    sql_string = "Update cte.dbo.cte_stu_demo set elis_account_created = 1 where cte_stu_id = " + formCollection[key_name];

                    // Run SQL
                    using (var db = new CoTEDB())
                    {
                        db.Database.Initialize(force: false);

                        // Check for Insert permissions
                        var cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql_string;

                        try
                        {

                            db.Database.Connection.Open();

                            // Run the sproc 
                            var reader = cmd.ExecuteReader();

                        }
                        catch (System.Data.SqlClient.SqlException e)
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

                    update_count++;

                }
            }

            ViewBag.message = "<br/>" + update_count.ToString() + " records updated. Click <a href='/dotnet/laces/laces.aspx'>here</a> to return to the LACES menu<br/>";

            return View();
        }

        [HttpGet]
        public ActionResult Precompletion()
        {
            string username = "";

            Database.SetInitializer<LACES_Context>(null);

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string cookie_username = cookie.Value;

                // Check if Authenticated_user is set if so use it
                if (cookie_username != "")
                {
                    username = cookie_username;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.message = "";

            return View();
        }

        [HttpPost]
        public ActionResult Precompletion(FormCollection formCollection)
        {
            string key_name = "";
            string sql_string = "";
            int update_count = 0;

            for (int i = 0; i < formCollection.Count; i++)
            {
                key_name = "checkbox_" + formCollection[i];
                if (!string.IsNullOrEmpty(formCollection[key_name]) && key_name != "Submit")
                {
                    sql_string = "Update cte.dbo.cte_stu_demo set pre_completion = 1 where cte_stu_id = " + formCollection[key_name];

                    // Run SQL
                    using (var db = new CoTEDB())
                    {
                        db.Database.Initialize(force: false);

                        // Check for Insert permissions
                        var cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql_string;

                        try
                        {

                            db.Database.Connection.Open();

                            // Run the sproc 
                            var reader = cmd.ExecuteReader();

                        }
                        catch (System.Data.SqlClient.SqlException e)
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

                    update_count++;

                }
            }

            ViewBag.message = "<br/>" + update_count.ToString() + " records updated. Click <a href='/dotnet/laces/laces.aspx'>here</a> to return to the LACES menu<br/>";

            return View();
        }    

        [HttpGet, Authorize]
        public ActionResult Supervisory_Survey()
        {
            string username = "";

            Database.SetInitializer<LACES_Context>(null);

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string cookie_username = cookie.Value;

                // Check if Authenticated_user is set if so use it
                if (cookie_username != "")
                {
                    username = cookie_username;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.message = "";

            return View();
        }

        [HttpPost]
        public ActionResult Supervisory_Survey(FormCollection formCollection)
        {
            string report_type = "=" + formCollection["report_type"];
            string program = "=" + formCollection["uiuc_program_id"];
            string term = "=" + formCollection["term_id"];
            string program_group = "=" + formCollection["UIUC_ProgramGroup"];
            string placement = "=" + formCollection["placement_type"];
            string url = "";

            if (term == "=")
            {
                term = ":isnull=true";
            }

            if (program == "=")
            {
                program = ":isnull=true";
            }

            if (program_group == "=")
            {
                program_group = ":isnull=true";
            }

            if (report_type == "Aggregate")
            {
                url = "http://reports.education.illinois.edu/Reportserver?%2fCoTE%2fSup_Sur_Results_Aggregate&rs%3aCommand=Render&program_id" + program +
                "&term_id" + term + "&placement_type" + placement + "&program_group" + program_group + "&rs%3aFormat=PDF"; 
            }
            else
            {
                url = "http://reports.education.illinois.edu/Reportserver?%2fCoTE%2fSup_Sur_Results_Individual&rs%3aCommand=Render&program_id" + program +
                 "&term_id" + term + "&placement_type" + placement + "&program_group" + program_group + "&rs%3aFormat=PDF";
                //http://chiedapp1.ad.uillinois.edu/Reports/Pages/Report.aspx?ItemPath=%2fCoTE%2fSup_Sur_Results_Individual
            }

            ViewBag.message = "";
            Response.Redirect(url);
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult Download_PDH_Attendence_Sheet()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult Download_PDH_Attendence_Sheet(FormCollection formCollection)
        {
            int record_count = 0;
            // Get event_id from previous page from drop down list
            string event_id = formCollection["event_id"];

            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];

                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];

                    StreamReader srContent1 = new StreamReader(file.InputStream);

                    string strContent1 = string.Empty;

                    if (srContent1.BaseStream.CanRead)
                    {
                        record_count++;

                        string sql_statement = "";

                        using (var package = new ExcelPackage(Request.InputStream))
                        {
                            int noOfCol = 0;
                            int noOfRow = 0;
                            object obj_name = null;
                            object obj_iein = null;
                            object obj_email = null;
                            object obj_date = null;
                            object obj_arrival_time = null;
                            object obj_departure_time = null;
                            object obj_attendence_time = null;
                            object obj_pdh = null;

                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            noOfCol = workSheet.Dimension.End.Column;
                            noOfRow = workSheet.Dimension.End.Row - 7;

                            for (int i = 1; i <= noOfRow; i++)
                            {
                                obj_name = workSheet.Cells[i + 7, 1].Text;
                                obj_iein = workSheet.Cells[i + 7, 2].Text;
                                obj_email = workSheet.Cells[i + 7, 3].Text;
                                obj_date = workSheet.Cells[i + 7, 4].Value;
                                obj_arrival_time = workSheet.Cells[i + 7, 5].Text;
                                obj_departure_time = workSheet.Cells[i + 7, 6].Text;
                                obj_attendence_time = workSheet.Cells[i + 7, 7].Text;
                                obj_pdh = workSheet.Cells[i + 7, 8].Text;

                                sql_statement = "Insert into cte.dbo.PDH_EVENT_X_ATTENDEE ([pdh_event_id],[full_name],[iein],[email],[event_date],[arrival_time],[departure_time],[attendence_time],[pdh_hours]) VALUES (" + event_id + ", '" + obj_name + "', '" + obj_iein + "', '" + obj_email + "', '" + obj_date + "', '" + obj_arrival_time + "', '" + obj_departure_time + "', '" + obj_attendence_time + "', '" + obj_pdh + "')";
                                //obj_cell = workSheet.Cells[1, 1].Value;

                                // Run SQL
                                using (var db = new CoTEDB())
                                {
                                    db.Database.Initialize(force: false);

                                    // Check for Insert permissions
                                    var cmd = db.Database.Connection.CreateCommand();
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = sql_statement;

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
                                }

                                // Loop through obj
                                record_count++;
                                
                            }
                        } 

                    } 

                 }
            }

            using (var db = new CoTEDB())
            {
                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[PDH_ATTENDENCE_SHEET_UPLOAD]";
                cmd.Parameters.Add(new SqlParameter("@event_id", event_id));

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

            ViewBag.record_count = record_count;

            return View();
        }

        public ActionResult PDH_EVENT_EOC()
        {
            string pdh_attendee_id = Request.QueryString["id"];


            using (var db = new CoTEDB())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[PDH_EVENT_EOC]";
                cmd.Parameters.Add(new SqlParameter("@pdh_attendee_id", pdh_attendee_id));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Move to result set and read coop data
                    var pdu = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<PDH_EVENT_EOC>(reader);


                    List<PDH_EVENT_EOC> EOCDataSet = pdu.ToList();


                    PDH_EVENT_EOC_Data EOC_PDU_Model = new PDH_EVENT_EOC_Data(EOCDataSet);

                    List<PDH_EVENT_EOC_Data> viewModelList = new List<PDH_EVENT_EOC_Data>();
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


        [HttpGet, Authorize]
        public ActionResult OrientationAttendance()
        {
            string username = "";

            Database.SetInitializer<LACES_Context>(null);

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {

                // Get username from cookie
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                string cookie_username = cookie.Value;

                // Check if Authenticated_user is set if so use it
                if (cookie_username != "")
                {
                    username = cookie_username;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.message = "";

            return View();
        }

        [HttpPost]
        public ActionResult OrientationAttendance(FormCollection formCollection)
        {
            string key_name = "";
            string sql_string = "";
            int update_count = 0;

            for (int i = 0; i < formCollection.Count; i++)
            {
                key_name = "checkbox_" + formCollection[i];
                if (!string.IsNullOrEmpty(formCollection[key_name]) && key_name != "Submit")
                {
                    sql_string = "Update cte.dbo.cte_stu_demo set attended_orientation = 1 where cte_stu_id = " + formCollection[key_name];

                    // Run SQL
                    using (var db = new CoTEDB())
                    {
                        db.Database.Initialize(force: false);

                        // Check for Insert permissions
                        var cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql_string;

                        try
                        {

                            db.Database.Connection.Open();

                            // Run the sproc 
                            var reader = cmd.ExecuteReader();

                        }
                        catch (System.Data.SqlClient.SqlException e)
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

                    update_count++;

                }
            }

            ViewBag.message = "<br/>" + update_count.ToString() + " records updated. Click <a href='/dotnet/laces/laces.aspx'>here</a> to return to the LACES menu<br/>";

            return View();
        }

        public ActionResult PDH_EVENT_OOS_EOC()
        {
            string pdh_attendee_id = Request.QueryString["id"];


            using (var db = new CoTEDB())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[PDH_EVENT_EOC]";
                cmd.Parameters.Add(new SqlParameter("@pdh_attendee_id", pdh_attendee_id));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    var reader = cmd.ExecuteReader();

                    // Move to result set and read coop data
                    var pdu = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<PDH_EVENT_OOS_EOC>(reader);


                    List<PDH_EVENT_OOS_EOC> OOSEOCDataSet = pdu.ToList();


                    PDH_EVENT_OOS_EOC_Data EOC_PDU_Model = new PDH_EVENT_OOS_EOC_Data(OOSEOCDataSet);

                    List<PDH_EVENT_OOS_EOC_Data> viewModelList = new List<PDH_EVENT_OOS_EOC_Data>();
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

        [HttpGet]
        public ActionResult PDH_EOC_Choose()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PDH_EOC_Choose(FormCollection formCollection)
        {
            string username = "";
            string choice = "";
            string pdu_url = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                var cookie = HttpContext.Request.Cookies["authenticated_user"];
                username = cookie.Value;
            }
            else
            {
                return Redirect("/dotnet/webpages/webpage.aspx?ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            choice = formCollection["choice"];

            using (var db = new CoTEDB())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.CommandText = "[dbo].[PDU_create]";
                cmd.CommandText = "[dbo].[PDH_Event]";
                cmd.Parameters.Add(new SqlParameter("@username", username));
                cmd.Parameters.Add(new SqlParameter("@choice", choice));

                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc 
                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            //pdu_id = reader[0].ToString();
                            pdu_url = reader[0].ToString();
                        }
                    }
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

            //return Redirect("/dotnet/FormProcessor/form.aspx/index?form_id=1420&action=edit&record_id=" + pdu_id + "&" + username);
            return Redirect(pdu_url);

        }

        [HttpGet, Authorize]
        public ActionResult Download_NES_Box()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult Download_NES_Box(FormCollection formCollection)
        {
            int record_count = 0;
            string error_string = "";
            string admin_date = "";
            string strSSN = "";
            string strCTE_STU_ID = "";

            string connString = System.Configuration.ConfigurationManager.ConnectionStrings["ssn_xlsx"].ConnectionString;
            // Create the connection object
            OleDbConnection oledbConn = new OleDbConnection(connString);

            //**************************
            //*** LOAD SCORE REPORTS
            //**************************
            HttpPostedFileBase file = Request.Files["UploadedTestScoreFile"];

            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string fileName = file.FileName;
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];

                StreamReader srContent1 = new StreamReader(file.InputStream);

                do
                {

                    string strContent1 = string.Empty;
                    string insert_statement = "";
                    int subscore_count = 0;

                    if (srContent1.BaseStream.CanRead)
                    {
                        record_count++;

                        strContent1 = srContent1.ReadLine();

                        if (strContent1 != null)
                        {

                            string sql_statement = " INSERT INTO cte.dbo.cte_stu_nes_test (cte_stu_id, last_name, first_name, middle_init, dob, admin_date, test_code, status, scaled_score, scores_1, scores_2, scores_3, scores_4, scores_5, scores_6, scores_7, created_date, created_by) VALUES (";

                            // Create string using
                            // [0,9] ssn
                            //insert_statement = sql_statement + strContent1.Substring(0, 9);
                            // Create string using
                            // [0,9] ssn
                            strSSN = strContent1.Substring(0, 9);

                            // Open connection
                            oledbConn.Open();

                            //DEBUGGING CODE FOR EXCEL SPREADSHEET testing to get first sheet
                            /*
                            DataTable dbSchema = oledbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            if (dbSchema == null || dbSchema.Rows.Count < 1)
                            {
                                throw new Exception("Error: Could not determine the name of the first worksheet.");
                            }
                            string firstSheetName = dbSchema.Rows[0]["TABLE_NAME"].ToString();
                            */

                            // Create OleDbCommand object and select data from worksheet Sheet1
                            OleDbCommand cmd_xls = new OleDbCommand("SELECT * FROM [Data$] WHERE [SSN] = " + strSSN, oledbConn);
                            //OleDbCommand cmd_xls = new OleDbCommand("SELECT * FROM [Data$]", oledbConn);

                            // Create new OleDbDataAdapter
                            OleDbDataAdapter oleda = new OleDbDataAdapter();

                            oleda.SelectCommand = cmd_xls;

                            // Create a DataSet which will hold the data extracted from the worksheet.
                            DataSet ds = new DataSet();

                            // Fill the datasheet
                            oleda.Fill(ds, "Data");

                            // Get the cte_stu_id 
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                strCTE_STU_ID = Convert.ToString(Convert.ToInt32(ds.Tables[0].Rows[0].ItemArray[1]));
                            }
                            else
                            {
                                strCTE_STU_ID = "null";
                            }

                            // Close connection
                            oledbConn.Close();

                            insert_statement = sql_statement + strCTE_STU_ID;

                            // [12,21] last name
                            insert_statement = insert_statement + ", '" + strContent1.Substring(11, 20).Replace("'", "") + "'";

                            // [35,13] first name
                            insert_statement = insert_statement + ", '" + strContent1.Substring(34, 12).Replace("'", "") + "'";

                            // [50,1] middle init
                            insert_statement = insert_statement + ", '" + strContent1.Substring(49, 1).Replace("'", "") + "'";

                            // [68,10] dob
                            insert_statement = insert_statement + ", '" + strContent1.Substring(53, 8) + "'";

                            // [68,10] admin date
                            insert_statement = insert_statement + ", '" + strContent1.Substring(68, 10) + "'";
                            admin_date = strContent1.Substring(68, 10);

                            // [80,3] test code
                            insert_statement = insert_statement + "," + strContent1.Substring(80, 3) + " ";

                            // [85,1] status (P/F)
                            insert_statement = insert_statement + ",'" + strContent1.Substring(85, 1) + "'";

                            // [88,3] scaled score
                            if (strContent1.TrimEnd().Length > 88)
                            {
                                insert_statement = insert_statement + "," + strContent1.Substring(88, 3) + "";
                            }
                            else
                            {
                                insert_statement = insert_statement + ", null";
                            }

                            // [93,1] number of subscores
                            if (strContent1.TrimEnd().Length > 88)
                            {
                                subscore_count = Convert.ToInt32(strContent1.Substring(93, 1), 10);
                            }
                            else
                            {
                                subscore_count = 0;
                            }

                            // [95, 3] subscore 1
                            for (int i = 1; i <= 7; i++)
                            {
                                if (i <= subscore_count)
                                {
                                    insert_statement = insert_statement + ", " + strContent1.Substring(91 + (i * 3) + i, 3);
                                }
                                else
                                {
                                    insert_statement = insert_statement + ", null";
                                }
                            }

                            insert_statement = insert_statement + ", '" + DateTime.Now.ToShortDateString() + "','NES Import')";

                            // Run SQL
                            using (var db = new CoTEDB())
                            {
                                db.Database.Initialize(force: false);

                                // Check for Insert permissions
                                var cmd = db.Database.Connection.CreateCommand();
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = insert_statement;

                                try
                                {

                                    db.Database.Connection.Open();

                                    // Run the sproc 
                                    var reader = cmd.ExecuteReader();

                                }
                                catch (System.Data.SqlClient.SqlException e)
                                {
                                    // Catch duplicate records and display message
                                    if (e.Number == 2627)
                                    {
                                        error_string = error_string + strContent1 + "<br>";
                                        record_count--;
                                    }
                                    else
                                    {
                                        //Redirect to an error page
                                        Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                                        return Redirect("~/FormProcessor/Error.aspx/HttpError");

                                    }
                                }
                                finally
                                {
                                    db.Database.Connection.Close();
                                }
                            }
                        }

                    }

                } while (srContent1.BaseStream.CanRead && srContent1.EndOfStream == false);

            }

            //*****************************
            //*** LOAD STATEWIDE RESULTS
            //*****************************
            HttpPostedFileBase statewide_file = Request.Files["UploadedStatewideScoreFile"];

            if ((statewide_file != null) && (statewide_file.ContentLength > 0) && !string.IsNullOrEmpty(statewide_file.FileName))
            {

                String strPDF = "";
                String strTestingInformation = "";
                String strTestDate = "";
                String intTestNumber = "";
                String intNumberOfExaminees = "";
                String intNumberPassing = "";
                String intNumberNotPassing = "";
                String intStateScaled = "";
                String intStateSub1 = "";
                String intStateSub2 = "";
                String intStateSub3 = "";
                String intStateSub4 = "";
                String intStateSub5 = "";
                String intStateSub6 = "";
                String intStateSub7 = "";

                //PDDocument doc = PDDocument.load("u:/nes/" + statewide_file.FileName);
                PDDocument doc = PDDocument.load(new ikvm.io.InputStreamWrapper(statewide_file.InputStream));
                PDFTextStripper pdfStripper = new PDFTextStripper();
                strPDF = pdfStripper.getText(doc);

                if (strPDF.Contains("Tests Taken Through:"))
                {
                    //strTestDate = CDate(Mid(strPDF, InStr(1, strPDF, "Tests Taken Through:") + 20, InStr(1, strPDF, "Illinois Licensure Testing System") - InStr(1, strPDF, "Tests Taken Through:") - 20))
                    strTestDate = strPDF.Substring(strPDF.IndexOf("Tests Taken Through:") + 20, strPDF.IndexOf("Illinois Licensure Testing System") - strPDF.IndexOf("Tests Taken Through:") - 20);
                }
                else if (strPDF.IndexOf("Test Date:") > 0)
                {
                    //strTestDate = CDate(Mid(strPDF, InStr(1, strPDF, "Test Date:") + 10, InStr(1, strPDF, "Illinois Licensure Testing System") - InStr(1, strPDF, "Test Date:") - 10))
                    strTestDate = strPDF.Substring(strPDF.IndexOf("Test Date:") + 10, strPDF.IndexOf("Illinois Licensure Testing System") - strPDF.IndexOf("Test Date:") - 10);
                }
                else if (strPDF.IndexOf("Test Administration Period:") > 0)
                {
                    //strTestDate = Mid(strPDF, InStr(1, strPDF, "Test Administration Period:") + 27, InStr(1, strPDF, "Illinois Licensure Testing System") - InStr(1, strPDF, "Test Administration Period:") - 27)
                    strTestDate = strPDF.Substring(strPDF.IndexOf("Test Administration Period:") + 27, strPDF.IndexOf("Illinois Licensure Testing System") - strPDF.IndexOf("Test Administration Period:") - 27);

                    //strTestDate = CDate(Mid(strTestDate, 1, InStr(1, strTestDate, "-") - 1) & Mid(strTestDate, InStr(1, strTestDate, ","), Len(strTestDate) - InStr(1, strTestDate, ",")))
                    strTestDate = strTestDate.Substring(strTestDate.IndexOf("-") - 1).ToString() + strTestDate.Substring(strTestDate.IndexOf(",") - 1, strTestDate.Length - strTestDate.IndexOf(",") - 1);
                }

                // Fix strTestDate
                strTestDate = strTestDate.Replace("\r\n", "");

                //Reset everything
                intStateScaled = "NULL";
                intStateSub1 = "NULL";
                intStateSub2 = "NULL";
                intStateSub3 = "NULL";
                intStateSub4 = "NULL";
                intStateSub5 = "NULL";
                intStateSub6 = "NULL";
                intStateSub7 = "NULL";
                intNumberOfExaminees = "NULL";
                intNumberPassing = "NULL";
                intNumberNotPassing = "NULL";

                strTestingInformation = strPDF.Substring(strPDF.IndexOf("Statewide "), strPDF.Length - strPDF.IndexOf("Statewide "));

                while (strTestingInformation.IndexOf("Statewide ", 1) > 0)
                {

                    String strStatewide = "";
                    String strSQL = "";

                    //Get test number
                    //intTestNumber = Mid(strTestingInformation, InStr(strTestingInformation, "Test:") + 6, 3)
                    intTestNumber = strTestingInformation.Substring(strTestingInformation.IndexOf("TEST:") + 6, 3);

                    //Get statewide information
                    //strStatewide = Mid(strTestingInformation, 11, InStr(1, strTestingInformation, "Test:") - 11)
                    strStatewide = strTestingInformation.Substring(10, strTestingInformation.IndexOf("TEST:") - 10);
                    strStatewide = strStatewide.Replace(Environment.NewLine, " ");

                    if (strStatewide.IndexOf("Subarea") > 0)
                    {
                        strStatewide = strStatewide.Substring(1, strStatewide.IndexOf("Subarea") - 1);
                    }
                    else if (strStatewide.IndexOf("Standard") > 0)
                    {
                        strStatewide = strStatewide.Substring(1, strStatewide.IndexOf("Standard") - 1);
                    }
                    else if (strStatewide.IndexOf("Objective") > 0)
                    {
                        strStatewide = strStatewide.Substring(1, strStatewide.IndexOf("Objective") - 1);
                    }
                    else if (strStatewide.IndexOf("Page") > 0)
                    {
                        strStatewide = strStatewide.Substring(1, strStatewide.IndexOf("Page") - 1);
                    }

                    //MsgBox "Test:" + intTestNumber + " - " + strStatewide

                    intNumberOfExaminees = strStatewide.Substring(0, strStatewide.IndexOf(" ") + 1);
                    strStatewide = strStatewide.Substring(strStatewide.IndexOf(" "), strStatewide.Length - strStatewide.IndexOf(" "));
                    intNumberPassing = strStatewide.Substring(0, strStatewide.IndexOf("("));

                    strStatewide = strStatewide.Substring(strStatewide.IndexOf(")") + 2, strStatewide.Length - strStatewide.IndexOf(")") - 2);
                    intNumberNotPassing = strStatewide.Substring(0, strStatewide.IndexOf("("));
                    strStatewide = strStatewide.Substring(strStatewide.IndexOf(")") + 2, strStatewide.Length - strStatewide.IndexOf(")") - 2);

                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateScaled = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateScaled = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateScaled = "NULL";
                    }

                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub1 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub1 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub1 = "NULL";
                    }

                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub2 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();

                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub2 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub2 = "NULL";
                    }

                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub3 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub3 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub3 = "NULL";
                    }

                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub4 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub4 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub4 = "NULL";
                    }

                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub5 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub5 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub5 = "NULL";
                    }

                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub6 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub6 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub6 = "NULL";
                    }

                    if (strStatewide.TrimStart().TrimEnd().Length > 3)
                    {
                        intStateSub7 = strStatewide.Substring(0, strStatewide.IndexOf(" "));
                        strStatewide = strStatewide.Substring(strStatewide.IndexOf(" ") + 1, strStatewide.Length - strStatewide.IndexOf(" ") - 1);
                        strStatewide = strStatewide.TrimStart().TrimEnd();
                    }
                    else if (strStatewide.TrimStart().TrimEnd().Length == 3)
                    {
                        intStateSub7 = strStatewide;
                        strStatewide = "";
                    }
                    else
                    {
                        intStateSub7 = "NULL";
                    }

                    strSQL = "Insert into cte_stu_nes_test_statewide (admin_date, test_code, scaled_score, scores_1,scores_2, scores_3, scores_4, scores_5,scores_6, scores_7, num_takers, num_passed, num_failed, created_by, created_date) values ('" + strTestDate + "', '" + intTestNumber + "', " +
                        intStateScaled + ", " +
                        intStateSub1 + ", " +
                        intStateSub2 + ", " +
                        intStateSub3 + ", " +
                        intStateSub4 + ", " +
                        intStateSub5 + ", " +
                        intStateSub6 + ", " +
                        intStateSub7 + ", " +
                        intNumberOfExaminees + ", " +
                        intNumberPassing + ", " +
                        intNumberNotPassing + ", " +
                        "'PDF Import'," +
                        "'" + DateTime.Now + "'" +
                        ");";

                    // Run SQL
                    using (var db = new CoTEDB())
                    {
                        db.Database.Initialize(force: false);

                        // Check for Insert permissions
                        var cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = strSQL;

                        try
                        {

                            db.Database.Connection.Open();

                            // Run the sproc 
                            var reader = cmd.ExecuteReader();

                        }
                        catch (System.Data.SqlClient.SqlException e)
                        {
                            // Catch duplicate records and display message
                            if (e.Number == 2627)
                            {
                                error_string = error_string + strSQL + "<br>";
                                record_count--;
                            }
                            else
                            {
                                //Redirect to an error page
                                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                                return Redirect("~/FormProcessor/Error.aspx/HttpError");

                            }
                        }
                        finally
                        {
                            db.Database.Connection.Close();
                        }
                    }

                    intStateScaled = "NULL";
                    intStateSub1 = "NULL";
                    intStateSub2 = "NULL";
                    intStateSub3 = "NULL";
                    intStateSub4 = "NULL";
                    intStateSub5 = "NULL";
                    intStateSub6 = "NULL";
                    intStateSub7 = "NULL";
                    intNumberOfExaminees = "NULL";
                    intNumberPassing = "NULL";
                    intNumberNotPassing = "NULL";

                    //Truncate strTestingInformation String
                    strTestingInformation = strTestingInformation.Substring(10);
                    strTestingInformation = strTestingInformation.Substring(strTestingInformation.IndexOf("Statewide ", 10), strTestingInformation.Length - strTestingInformation.IndexOf("Statewide ", 10));
                }

                // Results File
                // FileInfo f = new FileInfo("u:/nes/" + file.FileName);
                //f.CopyTo("u:/nes/NES.txt.imported." + admin_date.Replace("/", "_"));

                //f.Delete();

                // Statewide File
                //FileInfo f2 = new FileInfo("u:/nes/" + statewide_file.FileName);
                //f2.CopyTo("u:/nes/testing_report_" + admin_date.Replace("/", "_") + "_original.pdf");

                //f2.Delete();

                //MsgBox "PDF Import Completed"

            }

            using (var db = new CoTEDB())
            {
                // If using Code First we need to make sure the model is built before we open the connection
                // This isn't required for models created with the EF Designer
                db.Database.Initialize(force: false);

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[NES_Reports]";
                cmd.Parameters.Add(new SqlParameter("@test_date", admin_date));

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

            ViewBag.record_count = record_count;
            ViewBag.error_string = error_string;

            return View();
        }

        [HttpGet]
        public ActionResult NESTestMatcher() // For matching NES tests to students
        {
            string username = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.Username = username;

            return View();
        }

        [HttpPost]
        public ActionResult NESTestMatcher(FormCollection form) // For emailing coops based on course
        {

            string username = "";
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            string[] tests;
            string[] students;
            int test_count = 0;
            int update_count = 0;
            string sql_statement = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.Username = username;

            //Get search field
            if (form["section"] == "Filter")
            {
                ViewBag.admin_date_start = form["admin_date_start"];
                ViewBag.admin_date_end = form["admin_date_end"];
                ViewBag.test = form["test"];
                ViewBag.last_name = form["last_name"];
                ViewBag.dob = form["dob"];
                ViewBag.action = "filter";
            }
            else
            {
                tests = form["nes_id"].Split(delimiterChars);
                students = form["student"].Split(delimiterChars);

                test_count = tests.Length;

                for (int i = 0; i < test_count; i++)
                {
                    if (students[i] != "")
                    {
                        sql_statement += "Update cte_stu_nes_test set cte_stu_id = " + students[i] + ", modified_by = 'NES Test Matcher', modified_date = '" + DateTime.Now.ToString() + "' where nes_id = " + tests[i] + ";" + Environment.NewLine;
                        update_count++;
                    }
                 }

                // Run SQL
                using (var db = new CoTEDB())
                {
                    db.Database.Initialize(force: false);

                    // Check for Insert permissions
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql_statement;

                    try
                    {

                        db.Database.Connection.Open();

                        // Run the sproc 
 //                       var reader = cmd.ExecuteReader();

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

                ViewBag.message = update_count.ToString() + " records updated.";
                ViewBag.action = "save";
            }

            return View();
        }

        [HttpGet]
        public ActionResult SSNLoader() // For matching NES tests to students
        {
            string username = "";
            bool found = false;
            string cte_stu_id = "";
            string missing_ssns = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.Username = username;

            string connString = System.Configuration.ConfigurationManager.ConnectionStrings["ssn_xlsx"].ConnectionString;
            // Create the connection object
            OleDbConnection oledbConn = new OleDbConnection(connString);

            // Loop on active students
            var cmd = db.Database.Connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT cte_stu_id FROM cte.dbo.v_cte_stu_x_programs WHERE student_status = 'A' order by cte_stu_id desc";

            try
            {

                db.Database.Connection.Open();

                // Run the sproc 
                using (var reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        cte_stu_id = reader[0].ToString();

                        OleDbCommand cmd_xls = new OleDbCommand("SELECT * FROM [Data$] WHERE [CTE_STU_ID] = " + cte_stu_id, oledbConn);
                        //OleDbCommand cmd_xls = new OleDbCommand("SELECT * FROM [Data$]", oledbConn);

                        // Create new OleDbDataAdapter
                        OleDbDataAdapter oleda = new OleDbDataAdapter();

                        oleda.SelectCommand = cmd_xls;

                        // Create a DataSet which will hold the data extracted from the worksheet.
                        DataSet ds = new DataSet();

                        // Fill the datasheet
                        oleda.Fill(ds, "Data");

                        // Get the cte_stu_id 
                        if (ds.Tables[0].Rows.Count == 0)
                        {
                            missing_ssns = missing_ssns + cte_stu_id + ", ";
                        }

                    }
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
        

            // Close connection
            oledbConn.Close();

            ViewBag.cte_stu_id_list = missing_ssns.Substring(1, missing_ssns.Length - 1);

            return View();
        }


        [HttpPost]
        public ActionResult SSNLoader(FormCollection form) // For emailing coops based on course
        {

            string username = "";
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            string[] cte_stu_ids;
            string[] uins;
            string[] ssns;
            string[] students;
            int student_count = 0;
            int update_count = 0;
            string sql_statement = "";

            // Set username, if not logged, force to login screen
            if (HttpContext.User.Identity.Name != "" && HttpContext.User.Identity.ToString() != "System.Security.Principal.GenericIdentity")
            {
                username = HttpContext.User.Identity.Name;
            }
            else
            {
                return Redirect("/dotnet/account.aspx/login?​ReturnURL=" + Server.UrlEncode(HttpContext.Request.Url.OriginalString).Replace("&", "%26"));
            }

            ViewBag.Username = username;

            string connString = System.Configuration.ConfigurationManager.ConnectionStrings["ssn_xlsx"].ConnectionString;
            // Create the connection object
            OleDbConnection oledbConn = new OleDbConnection(connString);

            cte_stu_ids = form["cte_stu_id"].Split(delimiterChars);
            uins = form["uin"].Split(delimiterChars);
            ssns = form["ssn"].Split(delimiterChars);


            student_count = cte_stu_ids.Length;

            for (int i = 0; i < student_count; i++)
            {
                if (cte_stu_ids[i] != "")
                {

                    //                    sql_statement += "Update cte_stu_nes_test set cte_stu_id = " + students[i] + ", modified_by = 'NES Test Matcher', modified_date = '" + DateTime.Now.ToString() + "' where nes_id = " + tests[i] + ";" + Environment.NewLine;
                    //                    update_count++;
                    oledbConn.Open();
                    
                    OleDbCommand cmd_xls = new OleDbCommand("INSERT INTO [Data$] (UIN, CTE_STU_ID, SSN)" + " values ('" + uins[i] + "', '" + cte_stu_ids[i] + "', '" + ssns[i] + "')");

                    cmd_xls.Connection = oledbConn;

                    cmd_xls.ExecuteNonQuery();

                    oledbConn.Close();
                }
            }


            return View();
        }

    }

}
