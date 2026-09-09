using System;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("=== TẤT CẢ CÁC LẦN PHÂN BỔ VÀO PN26000188 (ID = 197) ===");
                string queryCT = @"
                    SELECT ct.ID AS ChiTietID, ct.IDPhieuChi, pc.SoPhieuChi, pc.NgayChi, pc.SoTienChi, pc.TrangThai AS TrangThaiPC, 
                           pc.IsDeleted AS IsDelPC, ct.LoaiChi, ct.SoTienPhanBo, ct.DienGiai
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    WHERE ct.IDPhieuNhap = 197
                    ORDER BY pc.ID, ct.ID";
                using (var cmd = new SqlCommand(queryCT, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("ChiTietID: {0} | PC: {1} (ID {2}) | NgayChi: {3:yyyy-MM-dd} | TrangThaiPC: {4} | IsDelPC: {5} | SoTienPB: {6,15:N0} | Note: {7}",
                            r["ChiTietID"], r["SoPhieuChi"], r["IDPhieuChi"], Convert.ToDateTime(r["NgayChi"]), r["TrangThaiPC"], r["IsDelPC"], r["SoTienPhanBo"], r["DienGiai"]));
                    }
                }

                Console.WriteLine("\n=== THÔNG TIN PHIẾU NHẬP PN26000188 (ID = 197) IN KHO_PHIEUNHAP ===");
                string queryPN = "SELECT ID, SoChungTu, TongCong, DaThanhToan, ConLai, TrangThaiThanhToan FROM KHO_PhieuNhap WHERE ID = 197";
                using (var cmd = new SqlCommand(queryPN, conn))
                using (var r = cmd.ExecuteReader()) {
                    if (r.Read()) {
                        Console.WriteLine(string.Format("PN: {0} | TongCong: {1:N0} | DaThanhToan (DB Column): {2:N0} | ConLai (DB Column): {3:N0} | TT_TT: {4}",
                            r["SoChungTu"], r["TongCong"], r["DaThanhToan"], r["ConLai"], r["TrangThaiThanhToan"]));
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
