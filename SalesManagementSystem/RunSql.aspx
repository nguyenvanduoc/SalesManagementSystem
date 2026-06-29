<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="SalesManagementSystem.Helpers.Security" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Text.RegularExpressions" %>
<%
    try {
        string connStr = ConfigManager.GetConnectionString("DefaultConnection");
        string sqlPath = Server.MapPath("~/App_Data/create_dieu_chinh_phieu_nhap.sql");
        string sql = File.ReadAllText(sqlPath);
        
        var parts = Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        using (var conn = new SqlConnection(connStr)) {
            conn.Open();
            foreach(var part in parts) {
                if (string.IsNullOrWhiteSpace(part)) continue;
                using (var cmd = new SqlCommand(part, conn)) {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        Response.Write("SUCCESS");
    } catch (Exception ex) {
        Response.Write("ERROR: " + ex.Message);
    }
%>
