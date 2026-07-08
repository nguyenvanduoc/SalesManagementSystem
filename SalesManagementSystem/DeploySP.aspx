<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>

<!DOCTYPE html>
<html>
<head>
    <title>Deploy SP</title>
</head>
<body>
    <%
        try
        {
            var db = new DbConnectionFactory();
            string path = Server.MapPath("~/App_Data/alter_sp_kho_phieunhap_getlist.sql");
            string sql = File.ReadAllText(path);
            
            var commands = sql.Split(new string[] { "GO\r\n", "GO\n", "GO " }, StringSplitOptions.RemoveEmptyEntries);
            using (var conn = db.CreateConnection())
            {
                conn.Open();
                foreach(var cmdStr in commands)
                {
                    var trimmed = cmdStr.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = trimmed;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            Response.Write("<h3>SP Deployed successfully.</h3>");
        }
        catch (Exception ex)
        {
            Response.Write("<h3>Error:</h3>");
            Response.Write("<pre>" + ex.Message + "\n" + ex.StackTrace + "</pre>");
        }
    %>
</body>
</html>
