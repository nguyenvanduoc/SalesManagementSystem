using System;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("=== CÁC PHIẾU NHẬP TỪ PN26000190 ĐẾN PN26000205 ===");
                string queryPN = @"
                    SELECT ID, SoChungTu, NgayNhap, TongCong, DaThanhToan, ConLai, TrangThaiThanhToan, TrangThai, IsDeleted
                    FROM KHO_PhieuNhap
                    WHERE IDNhaCungCap = 1 AND SoChungTu >= 'PN26000190'
                    ORDER BY SoChungTu";
                using (var cmd = new SqlCommand(queryPN, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PN ID: {0} | So: {1} | Ngay: {2:yyyy-MM-dd} | TongCong: {3,15:N0} | DaTT: {4,15:N0} | ConLai: {5,15:N0} | TT_TT: {6} | IsDel: {7}",
                            r["ID"], r["SoChungTu"], Convert.ToDateTime(r["NgayNhap"]), r["TongCong"], r["DaThanhToan"], r["ConLai"], r["TrangThaiThanhToan"], r["IsDeleted"]));
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
