<%@ Page Language="C#" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>
<%@ Import Namespace="Dapper" %>
<%
    using(var conn = new DbConnectionFactory().CreateConnection()) {
        var sql = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/alter_dieu_chinh_don_hang_phibocxep.sql"));
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
    Response.Write("Done Altering SP!");
%>
