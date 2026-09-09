using System;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("=== TÌM KHO_PHIEUNHAP CO SOCHUNGTU HOAC DIEN GIAI CHUA 198 HOAC 204 ===");
                string queryPN = @"
                    SELECT ID, SoChungTu, NgayNhap, IDNhaCungCap, TongCong, DaThanhToan, ConLai, TrangThaiThanhToan, TrangThai, IsDeleted
                    FROM KHO_PhieuNhap
                    WHERE SoChungTu LIKE '%198%' OR SoChungTu LIKE '%204%' OR SoChungTu >= 'PN26000195'
                    ORDER BY SoChungTu";
                using (var cmd = new SqlCommand(queryPN, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PN ID: {0} | So: {1} | Ngay: {2:yyyy-MM-dd} | NCC: {3} | TongCong: {4,15:N0} | DaTT: {5,15:N0} | ConLai: {6,15:N0} | TT_TT: {7} | IsDel: {8}",
                            r["ID"], r["SoChungTu"], Convert.ToDateTime(r["NgayNhap"]), r["IDNhaCungCap"], r["TongCong"], r["DaThanhToan"], r["ConLai"], r["TrangThaiThanhToan"], r["IsDeleted"]));
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
