using System;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("=== STORED PROCEDURE: sp_CongNo_PhaseTra_NCC_GetList ===");
                using (var cmd = new SqlCommand("SELECT OBJECT_DEFINITION(OBJECT_ID('sp_CongNo_PhaseTra_NCC_GetList'))", conn)) {
                    var def = cmd.ExecuteScalar();
                    Console.WriteLine(def != null ? def.ToString() : "NULL");
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
