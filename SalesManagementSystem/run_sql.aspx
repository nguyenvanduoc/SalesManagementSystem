<%@ Page Language="C#" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>
<%@ Import Namespace="Dapper" %>
<%@ Import Namespace="System.IO" %>
<%
    try {
        string path = Server.MapPath("~/App_Data/phieuchi_change.sql");
        string sql = File.ReadAllText(path);
        var commands = sql.Split(new[] { "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
        using(var conn = new DbConnectionFactory().CreateConnection()) {
            foreach(var cmd in commands) {
                if(string.IsNullOrWhiteSpace(cmd)) continue;
                conn.Execute(cmd);
            }
            Response.Write("SUCCESS");
        }
    } catch (Exception ex) {
        Response.Write(ex.ToString());
    }
%>
