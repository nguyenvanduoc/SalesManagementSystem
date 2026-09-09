using System;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("=== TẤT CẢ PHIẾU NHẬP CÓ NGÀY NHẬP >= 2026-08-20 HOẶC SỐ CHỨNG TỪ SỐ CỦA NÓ ===");
                string queryPN = @"
                    SELECT pn.ID, pn.SoChungTu, pn.NgayNhap, pn.IDNhaCungCap, ncc.TenNhaCungCap, pn.TongCong, pn.DaThanhToan, pn.ConLai, pn.TrangThaiThanhToan, pn.TrangThai, pn.IsDeleted
                    FROM KHO_PhieuNhap pn
                    LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
                    WHERE pn.NgayNhap >= '2026-08-20' OR pn.SoChungTu LIKE 'PN26000%'
                    ORDER BY pn.ID DESC";
                using (var cmd = new SqlCommand(queryPN, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("ID: {0} | So: {1} | Ngay: {2:yyyy-MM-dd} | NCC_ID: {3} ({4}) | Tong: {5,15:N0} | DaTT: {6,15:N0} | ConLai: {7,15:N0} | TT_TT: {8} | IsDel: {9}",
                            r["ID"], r["SoChungTu"], Convert.ToDateTime(r["NgayNhap"]), r["IDNhaCungCap"], r["TenNhaCungCap"], r["TongCong"], r["DaThanhToan"], r["ConLai"], r["TrangThaiThanhToan"], r["IsDeleted"]));
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
