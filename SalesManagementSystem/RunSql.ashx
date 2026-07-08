<%@ WebHandler Language="C#" Class="RunSql" %>

using System;
using System.Web;
using System.IO;
using System.Data.SqlClient;
using SalesManagementSystem.Helpers.Security;

public class RunSql : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        
        try {
            string sqlFilePath = context.Server.MapPath("~/App_Data/create_sp_CongNoNCC.sql");
            string script = File.ReadAllText(sqlFilePath);
            
            string connString = ConfigManager.GetConnectionString("DefaultConnection");
            
            using (var conn = new SqlConnection(connString)) {
                conn.Open();
                var commands = script.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string commandString in commands)
                {
                    if (commandString.Trim().Length > 0)
                    {
                        using (var command = new SqlCommand(commandString, conn))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            context.Response.Write("SQL executed successfully.");
        }
        catch(Exception ex) {
            context.Response.Write("Error: " + ex.Message + "\n" + ex.StackTrace);
        }
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }
}
