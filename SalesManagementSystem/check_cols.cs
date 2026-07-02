using System;
using System.Data.SqlClient;

class Program {
    static void Main() {
        string connStr = @"Data Source=.;Initial Catalog=SalesManagementSystem;Integrated Security=True";
        using(var conn = new SqlConnection(connStr)) {
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME LIKE '%Phi%' OR COLUMN_NAME LIKE '%VanChuyen%'";
            using(var reader = cmd.ExecuteReader()) {
                while(reader.Read()) {
                    Console.WriteLine(string.Format("{0} - {1}", reader["TABLE_NAME"], reader["COLUMN_NAME"]));
                }
            }
        }
    }
}
