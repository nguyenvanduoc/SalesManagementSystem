<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Dapper" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Web.Mvc" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            var db = DependencyResolver.Current.GetService<DbConnectionFactory>();
            using (var conn = db.CreateConnection())
            {
                var sql = "SELECT OBJECT_DEFINITION(OBJECT_ID('sp_KHO_PhieuNhap_Save'))";
                var spSave = conn.ExecuteScalar<string>(sql);

                sql = "SELECT OBJECT_DEFINITION(OBJECT_ID('sp_KHO_PhieuNhap_GetList'))";
                var spGetList = conn.ExecuteScalar<string>(sql);
                
                sql = "SELECT OBJECT_DEFINITION(OBJECT_ID('sp_KHO_PhieuNhap_GetByID'))";
                var spGetById = conn.ExecuteScalar<string>(sql);
                
                sql = "SELECT OBJECT_DEFINITION(OBJECT_ID('sp_KHO_PhieuNhap_GhiSo'))";
                var spGhiSo = conn.ExecuteScalar<string>(sql);
                
                sql = "SELECT OBJECT_DEFINITION(OBJECT_ID('sp_KHO_PhieuNhap_Huy'))";
                var spHuy = conn.ExecuteScalar<string>(sql);

                Response.Write("<h3>sp_KHO_PhieuNhap_Save</h3><pre>" + Server.HtmlEncode(spSave) + "</pre>");
                Response.Write("<h3>sp_KHO_PhieuNhap_GetList</h3><pre>" + Server.HtmlEncode(spGetList) + "</pre>");
                Response.Write("<h3>sp_KHO_PhieuNhap_GetByID</h3><pre>" + Server.HtmlEncode(spGetById) + "</pre>");
                Response.Write("<h3>sp_KHO_PhieuNhap_GhiSo</h3><pre>" + Server.HtmlEncode(spGhiSo) + "</pre>");
                Response.Write("<h3>sp_KHO_PhieuNhap_Huy</h3><pre>" + Server.HtmlEncode(spHuy) + "</pre>");
            }
        }
        catch (Exception ex)
        {
            Response.Write(ex.ToString());
        }
    }
</script>
