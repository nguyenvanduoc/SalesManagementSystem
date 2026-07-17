using System;
using System.Data.SqlClient;
using System.IO;

class Program {
    static void Main() {
        string connStr = "Server=.\\SQLEXPRESS;Database=SalesManagement;Trusted_Connection=True;";
        try {
            using (SqlConnection conn = new SqlConnection(connStr)) {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_helptext 'sp_DON_DieuChinhDonHang_Save'", conn)) {
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        string result = "";
                        while(reader.Read()) {
                            result += reader.GetString(0);
                        }
                        File.WriteAllText("sp_def.txt", result);
                        Console.WriteLine("Success");
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}
