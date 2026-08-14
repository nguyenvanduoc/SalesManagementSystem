using System;
using System.Data.SqlClient;
using SalesManagementSystem.Helpers.Security;

class Program {
    static void Main() {
        try {
            string connStr = ConfigManager.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connStr)) {
                conn.Open();
                using (var cmd = conn.CreateCommand()) {
                    cmd.CommandText = "EXEC sp_KHO_PhieuNhap_GetList @SoChungTu = 'PN26000134', @TotalRecords = 0 OUTPUT";
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            Console.WriteLine("ID: " + reader["ID"] + ", SoChungTu: " + reader["SoChungTu"] + ", TongSoLuong: " + reader["TongSoLuong"] + ", TienVanChuyen: " + String.Format("{0:N0}", reader["TienVanChuyen"]) + ", TongCong: " + String.Format("{0:N0}", reader["TongCong"]));
                        }
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
