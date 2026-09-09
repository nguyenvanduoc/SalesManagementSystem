using System;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("=== DANH SÁCH TẤT CẢ PHIẾU CHI VINAKEN ===");
                string queryPC = @"
                    SELECT ID, SoPhieuChi, NgayChi, SoTienChi, IDPhieuNhap, TrangThai, IsDeleted, DienGiai
                    FROM KT_PhieuChi
                    WHERE IDNhaCungCap = 1
                    ORDER BY ID";
                using (var cmd = new SqlCommand(queryPC, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PC ID: {0} | So: {1} | Ngay: {2:yyyy-MM-dd} | SoTienChi: {3:N0} | Header_IDPN: {4} | TT: {5} | IsDel: {6} | Note: {7}",
                            r["ID"], r["SoPhieuChi"], Convert.ToDateTime(r["NgayChi"]), r["SoTienChi"], r["IDPhieuNhap"], r["TrangThai"], r["IsDeleted"], r["DienGiai"]));
                    }
                }

                Console.WriteLine("\n=== CÁC PHIẾU CHI CÓ PHÂN BỔ HOẶC CÓ IDPHIEUNHAP > 200 ===");
                string queryCT = @"
                    SELECT ct.ID, ct.IDPhieuChi, pc.SoPhieuChi, pc.NgayChi, pc.TrangThai AS TrangThaiPC, ct.IDPhieuNhap, pn.SoChungTu AS SoPhieuNhap, 
                           ct.LoaiChi, ct.SoTienPhanBo, ct.DienGiai
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    LEFT JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID
                    WHERE pc.IDNhaCungCap = 1
                    ORDER BY pc.ID, ct.ID";
                using (var cmd = new SqlCommand(queryCT, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        if (Convert.ToInt32(r["IDPhieuChi"]) >= 190) {
                            Console.WriteLine(string.Format("CT_ID: {0} | PC: {1} (ID {2}, TT={3}) | PN: {4} (ID {5}) | LoaiChi: {6} | SoTienPB: {7:N0} | Note: {8}",
                                r["ID"], r["SoPhieuChi"], r["IDPhieuChi"], r["TrangThaiPC"], r["SoPhieuNhap"], r["IDPhieuNhap"], r["LoaiChi"], r["SoTienPhanBo"], r["DienGiai"]));
                        }
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
