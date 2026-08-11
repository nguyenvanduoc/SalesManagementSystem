<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="SalesManagementSystem.Helpers.Security" %>
<%@ Import Namespace="System.IO" %>
<%
    try {
        string connStr = ConfigManager.GetConnectionString("DefaultConnection");
        string sqlFile = Server.MapPath("~/App_Data/sp_DON_DieuChinhDonHang_Save.sql");
        string sql = System.IO.File.ReadAllText(sqlFile, System.Text.Encoding.UTF8);
        using (var conn = new SqlConnection(connStr)) {
            conn.Open();
            using (var cmd = new SqlCommand(sql, conn)) {
                cmd.CommandTimeout = 60;
                cmd.ExecuteNonQuery();
            }
        }
        Response.Write("OK: sp_DON_DieuChinhDonHang_Save updated at " + DateTime.Now);
    } catch (Exception ex) {
        Response.Write("ERROR: " + ex.Message + "<br/><pre>" + ex.ToString() + "</pre>");
    }
%>
