using System;
using System.Data.SqlClient;

class Program {
    static void Main() {
        string connStr = "Server=.;Database=SalesManagementSystem;Integrated Security=True;";
        try {
            using (var conn = new SqlConnection(connStr)) {
                conn.Open();
                Console.WriteLine("Success");
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}
