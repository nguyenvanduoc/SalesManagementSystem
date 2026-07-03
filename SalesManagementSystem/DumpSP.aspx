<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="SalesManagementSystem.Helpers.Security" %>
<%
    try {
        string connStr = ConfigManager.GetConnectionString("DefaultConnection");
        using (var conn = new SqlConnection(connStr)) {
            conn.Open();
            using (var cmd = new SqlCommand("sp_helptext 'sp_KT_PhieuChi_GetList'", conn)) {
                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        Response.Write(Server.HtmlEncode(reader.GetString(0)) + "<br/>");
                    }
                }
            }
        }
    } catch (Exception ex) {
        Response.Write("ERROR: " + ex.Message);
    }
%>
