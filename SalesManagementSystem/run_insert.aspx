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
                string sqlPath = Server.MapPath("~/App_Data/InsertTemplate.sql");
                string sql = File.ReadAllText(sqlPath);
                
                // Split GO if any (though my script didn't use GO, just multiple statements)
                using(var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
                
                Response.Write("SUCCESS: Template inserted!");
            }
        }
        catch(Exception ex)
        {
            Response.Write("ERROR: " + ex.ToString());
        }
    }
</script>
