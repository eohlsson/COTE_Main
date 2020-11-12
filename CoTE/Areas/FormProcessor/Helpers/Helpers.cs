using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Web.Mvc;
using FormProcessor.Models; 
using System.Web;
using System.Configuration;
using System.Data.SqlClient;

namespace CoTE.Areas.FormProcessor.Helpers
{
    
    public class Helpers
    {

        public static void WriteToLog(string username, string ip_address, string msg)
        {
            
            String ConnString = ConfigurationManager.ConnectionStrings["UserDb"].ConnectionString;
            String sql_statement = "";

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {

                    sql_statement = "Insert into FRM_PROC.dbo.FRM_DEBUG (username, message, ip_address, debug_datetime) " +
                        " VALUES " +
                        "('" + username + "', '" + msg.Replace("'", "''") + "', '" + ip_address + "', '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + "')";
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = sql_statement;

                    var reader = cmd.ExecuteReader();
                }
            }

            /* Write to file
            string strLogFilePath = "\\dotnet\\debug\\fp_debug.txt";

            try
            {
                if (!File.Exists(strLogFilePath))
                {
                    File.Create(strLogFilePath).Close();
                }
                using (StreamWriter w = File.AppendText(strLogFilePath))
                {
                    w.WriteLine("{0}: " + msg, DateTime.Now.ToString());
                    w.Flush();
                    w.Close();
                }
            }
            finally
            {
            }
            */
            
        }

    }
}