using System;
using System.Data.SqlClient;
using System.IO;

class Program {
    static void Main() {
        string connStr = "Server=.;Database=SalesManagementSystem;Integrated Security=True;";
        using(var conn = new SqlConnection(connStr)) {
            conn.Open();
            var cmd = new SqlCommand("sp_helptext 'sp_CongNoKhachHang_GetList'", conn);
            using(var reader = cmd.ExecuteReader()) {
                using(var sw = new StreamWriter("sp_dump.txt")) {
                    while(reader.Read()) {
                        sw.Write(reader.GetString(0));
                    }
                }
            }
        }
    }
}
