<%@ Page Language="C#" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>
<%@ Import Namespace="Dapper" %>
<%@ Import Namespace="System.IO" %>
<%
    try {
        string sql = File.ReadAllText(Server.MapPath("~/App_Data/alter_dieu_chinh_don_hang_phibocxep.sql"));
        using(var conn = new DbConnectionFactory().CreateConnection()) {
            conn.Execute(sql);
            Response.Write("SUCCESS");
        }
    } catch (Exception ex) {
        Response.Write(ex.ToString());
    }
%>
