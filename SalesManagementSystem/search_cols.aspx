<%@ Page Language="C#" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>
<%@ Import Namespace="Dapper" %>
<%
    try {
        using(var conn = new DbConnectionFactory().CreateConnection()) {
            var columns = conn.Query("SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME LIKE '%Phi%' OR COLUMN_NAME LIKE '%VanChuyen%'").ToList();
            Response.Write("<pre>");
            foreach(var c in columns) {
                Response.Write($"{c.TABLE_NAME} - {c.COLUMN_NAME}\n");
            }
            Response.Write("</pre>");
        }
    } catch (Exception ex) {
        Response.Write(ex.ToString());
    }
%>
