using System;
using System.Data.SqlClient;

class Program {
    static void Main() {
        string connStr = "Server=.;Database=SalesManagementSystem;Integrated Security=True;";
        try {
            using(var conn = new SqlConnection(connStr)) {
                conn.Open();
                var cmd = new SqlCommand("SELECT OBJECT_DEFINITION(OBJECT_ID('sp_CongNoKhachHang_GetList'))", conn);
                var def = cmd.ExecuteScalar() as string;
                if (def != null && def.Contains("@TuNgay IS NULL OR")) {
                    Console.WriteLine("SP contains filtering.");
                } else {
                    Console.WriteLine("SP DOES NOT contain filtering.");
                    Console.WriteLine("Snippet: " + (def != null ? def.Substring(0, Math.Min(200, def.Length)) : "null"));
                }
            }
        } catch (Exception ex) {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
