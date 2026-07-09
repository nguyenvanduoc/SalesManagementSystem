<%@ Page Language="C#" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="Dapper" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            using(var conn = new DbConnectionFactory().CreateConnection())
            {
                conn.Open();
                string spName = Request.QueryString["sp"] ?? "sp_Dashboard_GetData";
                string sql = "SELECT OBJECT_DEFINITION(OBJECT_ID(@SpName)) AS SpDef";
                string def = conn.ExecuteScalar<string>(sql, new { SpName = spName });
                
                if(string.IsNullOrEmpty(def))
                {
                    Response.Write("SP '" + spName + "' not found or no definition available.");
                    return;
                }
                
                // Save to App_Data
                string outPath = Server.MapPath("~/App_Data/" + spName + "_extracted.sql");
                File.WriteAllText(outPath, def, System.Text.Encoding.UTF8);
                
                Response.ContentType = "text/plain";
                Response.Write(def);
            }
        }
        catch(Exception ex)
        {
            Response.Write("ERROR: " + ex.ToString());
        }
    }
</script>
