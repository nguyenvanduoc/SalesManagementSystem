<%@ Page Language="C#" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            using(var conn = new DbConnectionFactory().CreateConnection())
            {
                conn.Open();
                string sqlPath = Server.MapPath("~/App_Data/alter_schema_cogs.sql");
                string sql = File.ReadAllText(sqlPath);
                
                string[] batches = sql.Split(new[] { "\r\nGO\r\n", "\nGO\n", "GO\r", "GO\n", "\r\nGO" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach(string batch in batches)
                {
                    if(string.IsNullOrWhiteSpace(batch)) continue;
                    using(var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = batch;
                        cmd.ExecuteNonQuery();
                    }
                }
                
                Response.Write("SUCCESS: Alter schema executed!");
            }
        }
        catch(Exception ex)
        {
            Response.Write("ERROR: " + ex.ToString());
        }
    }
</script>
