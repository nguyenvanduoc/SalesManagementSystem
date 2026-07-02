<%@ Page Language="C#" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>
<%@ Import Namespace="Dapper" %>
<%
    try {
        using(var conn = new DbConnectionFactory().CreateConnection()) {
            var triggers = conn.Query("SELECT name, OBJECT_DEFINITION(object_id) AS definition FROM sys.triggers WHERE parent_id = OBJECT_ID('NS_DonDatHangChiTiet') OR parent_id = OBJECT_ID('NS_DonDatHang')").ToList();
            Response.Write("<pre>");
            foreach(var t in triggers) {
                Response.Write($"Trigger: {t.name}\n{t.definition}\n\n");
            }
            Response.Write("</pre>");
        }
    } catch (Exception ex) {
        Response.Write(ex.ToString());
    }
%>
