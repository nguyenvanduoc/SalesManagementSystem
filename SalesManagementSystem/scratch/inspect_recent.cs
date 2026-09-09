using System;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("=== A. DANH SÁCH PHIẾU NHẬP TỪ PN26000180 TRỞ ĐI ===");
                string queryPN = @"
                    SELECT ID, SoChungTu, NgayNhap, TongCong, DaThanhToan, ConLai, TrangThaiThanhToan, TrangThai
                    FROM KHO_PhieuNhap
                    WHERE IDNhaCungCap = 1 AND IsDeleted = 0 AND SoChungTu >= 'PN26000180'
                    ORDER BY SoChungTu";
                using (var cmd = new SqlCommand(queryPN, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PN ID: {0} | So: {1} | Ngay: {2:yyyy-MM-dd} | TongCong: {3:N0} | DaTT: {4:N0} | ConLai: {5:N0} | TT_TT: {6}",
                            r["ID"], r["SoChungTu"], Convert.ToDateTime(r["NgayNhap"]), r["TongCong"], r["DaThanhToan"], r["ConLai"], r["TrangThaiThanhToan"]));
                    }
                }

                Console.WriteLine("\n=== B. DANH SÁCH PHIẾU CHI TỪ PC26000190 TRỞ ĐI ===");
                string queryPC = @"
                    SELECT ID, SoPhieuChi, NgayChi, SoTienChi, IDPhieuNhap, TrangThai, DienGiai
                    FROM KT_PhieuChi
                    WHERE IDNhaCungCap = 1 AND IsDeleted = 0 AND SoPhieuChi >= 'PC26000190'
                    ORDER BY SoPhieuChi";
                using (var cmd = new SqlCommand(queryPC, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PC ID: {0} | So: {1} | Ngay: {2:yyyy-MM-dd} | SoTienChi: {3:N0} | Header_IDPN: {4} | TT: {5} | Note: {6}",
                            r["ID"], r["SoPhieuChi"], Convert.ToDateTime(r["NgayChi"]), r["SoTienChi"], r["IDPhieuNhap"], r["TrangThai"], r["DienGiai"]));
                    }
                }

                Console.WriteLine("\n=== C. CHI TIẾT PHÂN BỔ CỦA CÁC PHIẾU CHI TỪ PC26000190 TRỞ ĐI ===");
                string queryCT = @"
                    SELECT ct.ID, ct.IDPhieuChi, pc.SoPhieuChi, pc.NgayChi, pc.TrangThai AS TrangThaiPC, ct.IDPhieuNhap, pn.SoChungTu AS SoPhieuNhap, 
                           ct.LoaiChi, ct.SoTienPhanBo, ct.DienGiai
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    LEFT JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID
                    WHERE pc.IDNhaCungCap = 1 AND pc.IsDeleted = 0 AND pc.SoPhieuChi >= 'PC26000190'
                    ORDER BY pc.SoPhieuChi, ct.ID";
                using (var cmd = new SqlCommand(queryCT, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("CT_ID: {0} | PC: {1} (ID {2}, TT={3}) | PN: {4} (ID {5}) | LoaiChi: {6} | SoTienPB: {7:N0} | Note: {8}",
                            r["ID"], r["SoPhieuChi"], r["IDPhieuChi"], r["TrangThaiPC"], r["SoPhieuNhap"], r["IDPhieuNhap"], r["LoaiChi"], r["SoTienPhanBo"], r["DienGiai"]));
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
