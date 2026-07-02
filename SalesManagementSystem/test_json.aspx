<%@ Page Language="C#" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>
<%@ Import Namespace="Dapper" %>
<%@ Import Namespace="SalesManagementSystem.Models.ViewModels" %>
<%
    try {
        using(var conn = new DbConnectionFactory().CreateConnection()) {
            var chiTiets = conn.Query<DonDatHangChiTietViewModel>("SELECT TOP 1 * FROM NS_DonDatHangChiTiet ORDER BY ID DESC").ToList();
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(chiTiets));
        }
    } catch (Exception ex) {
        Response.Write(ex.ToString());
    }
%>
