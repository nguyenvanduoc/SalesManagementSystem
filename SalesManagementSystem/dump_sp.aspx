<%@ Page Language="C#" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>
<%@ Import Namespace="Dapper" %>
<%
    try {
        using(var conn = new DbConnectionFactory().CreateConnection()) {
            string sql = "SELECT OBJECT_DEFINITION(OBJECT_ID('sp_KT_PhieuChi_Delete'))";
            string def = conn.ExecuteScalar<string>(sql);
            Response.Write(def);
        }
    } catch (Exception ex) {
        Response.Write(ex.ToString());
    }
%>
