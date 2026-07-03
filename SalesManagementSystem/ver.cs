using System;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = SalesManagementSystem.Helpers.Security.ConfigManager.GetConnectionString("DefaultConnection");
            using(var conn = new SqlConnection(connStr)) {
                conn.Open();
                using(var cmd = new SqlCommand("SELECT @@VERSION", conn)) {
                    Console.WriteLine(cmd.ExecuteScalar());
                }
            }
        } catch (Exception ex) { Console.WriteLine("ERR:" + ex.Message); }
    }
}
