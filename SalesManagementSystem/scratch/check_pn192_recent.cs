using System;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("=== TẤT CẢ PHIẾU CHI VÀ CHI TIẾT CỦA CÁC PHIẾU CHI MỚI TỪ PC26000198 ĐẾN PC26000205 ===");
                string queryPC = @"
                    SELECT pc.ID, pc.SoPhieuChi, pc.NgayChi, pc.SoTienChi, pc.TrangThai, ct.IDPhieuNhap, pn.SoChungTu AS SoPhieuNhap, 
                           ct.LoaiChi, ct.SoTienPhanBo, ct.DienGiai
                    FROM KT_PhieuChi pc
                    LEFT JOIN KT_PhieuChiChiTiet ct ON pc.ID = ct.IDPhieuChi
                    LEFT JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID
                    WHERE pc.IDNhaCungCap = 1 AND pc.IsDeleted = 0 AND pc.SoPhieuChi >= 'PC26000198'
                    ORDER BY pc.SoPhieuChi, ct.ID";
                using (var cmd = new SqlCommand(queryPC, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PC: {0} (ID {1}) | NgayChi: {2:yyyy-MM-dd} | SoTienChi: {3,15:N0} | PN: {4} (ID {5}) | SoTienPB: {6,15:N0} | Note: {7}",
                            r["SoPhieuChi"], r["ID"], Convert.ToDateTime(r["NgayChi"]), r["SoTienChi"], r["SoPhieuNhap"], r["IDPhieuNhap"], r["SoTienPhanBo"], r["DienGiai"]));
                    }
                }

                Console.WriteLine("\n=== CÁC LẦN PHÂN BỔ HIỆN TẠI VÀO PN26000192 (ID 201) ===");
                string queryPN192 = @"
                    SELECT ct.ID, ct.IDPhieuChi, pc.SoPhieuChi, pc.NgayChi, pc.TrangThai, ct.SoTienPhanBo, ct.DienGiai
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    WHERE ct.IDPhieuNhap = 201 AND pc.IsDeleted = 0";
                using (var cmd = new SqlCommand(queryPN192, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("CT ID: {0} | PC: {1} (ID {2}) | NgayChi: {3:yyyy-MM-dd} | TT: {4} | SoTienPB: {5,15:N0} | Note: {6}",
                            r["ID"], r["SoPhieuChi"], r["IDPhieuChi"], Convert.ToDateTime(r["NgayChi"]), r["TrangThai"], r["SoTienPhanBo"], r["DienGiai"]));
                    }
                }

                Console.WriteLine("\n=== KIỂM TRA PHIẾU PN26000186 (ID 195) ===");
                string queryPN186 = @"
                    SELECT ID, SoChungTu, TongCong, DaThanhToan, ConLai
                    FROM KHO_PhieuNhap WHERE SoChungTu = 'PN26000186' OR ID = 195";
                using (var cmd = new SqlCommand(queryPN186, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PN: {0} | TongCong: {1,15:N0} | DaTT: {2,15:N0} | ConLai: {3,15:N0}",
                            r["SoChungTu"], r["TongCong"], r["DaThanhToan"], r["ConLai"]));
                    }
                }

                Console.WriteLine("\n=== CÁC LẦN PHÂN BỔ VÀO PN26000186 (ID 195) ===");
                string queryCT186 = @"
                    SELECT ct.ID, ct.IDPhieuChi, pc.SoPhieuChi, pc.NgayChi, pc.TrangThai, ct.SoTienPhanBo, ct.DienGiai
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    WHERE ct.IDPhieuNhap = 195 AND pc.IsDeleted = 0";
                using (var cmd = new SqlCommand(queryCT186, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("CT ID: {0} | PC: {1} (ID {2}) | NgayChi: {3:yyyy-MM-dd} | TT: {4} | SoTienPB: {5,15:N0} | Note: {6}",
                            r["ID"], r["SoPhieuChi"], r["IDPhieuChi"], Convert.ToDateTime(r["NgayChi"]), r["TrangThai"], r["SoTienPhanBo"], r["DienGiai"]));
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
