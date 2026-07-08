using System;
using System.IO;
using System.Data.SqlClient;

class Program {
    static void Main() {
        string connStr = @"Data Source=.;Initial Catalog=QuanLyBanHang;Integrated Security=True";
        using (SqlConnection conn = new SqlConnection(connStr)) {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand("SELECT NoiDung FROM DM_BieuMau WHERE MaBieuMau = 'CNKH'", conn)) {
                var bytes = cmd.ExecuteScalar() as byte[];
                if (bytes != null) {
                    File.WriteAllBytes("CNKH_Template.xlsx", bytes);
                    Console.WriteLine("Saved to CNKH_Template.xlsx");
                } else {
                    Console.WriteLine("Template not found");
                }
            }
        }
    }
}
