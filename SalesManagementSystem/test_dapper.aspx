<%@ Page Language="C#" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="Dapper" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>
<%@ Import Namespace="SalesManagementSystem.Models.ViewModels" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            using(var conn = new DbConnectionFactory().CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@TuNgay", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
                parameters.Add("@DenNgay", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1));
                
                var data = conn.Query<BaoCaoKetQuaHoatDongKinhDoanhRowModel>(
                    "sp_BC_KetQuaHoatDongKinhDoanh_GetList",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();
                
                Response.Write("Success: " + data.Count + " rows");
            }
        }
        catch(Exception ex)
        {
            Response.Write("Error: " + ex.ToString());
        }
    }
</script>
