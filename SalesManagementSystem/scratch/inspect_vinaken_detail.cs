using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("=== 1. TẤT CẢ PHIẾU NHẬP CỦA VINAKEN TRONG THÁNG 8/2026 ===");
                string queryPN = @"
                    SELECT ID, SoChungTu, NgayNhap, ISNULL(TongCong, 0) AS TongCong, ISNULL(DaThanhToan, 0) AS DaThanhToan, 
                           ISNULL(ConLai, 0) AS ConLai, TrangThaiThanhToan, TrangThai, IsDeleted
                    FROM KHO_PhieuNhap
                    WHERE IDNhaCungCap = 1 AND IsDeleted = 0
                    ORDER BY NgayNhap, ID";
                using (var cmd = new SqlCommand(queryPN, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("ID: {0} | So: {1} | Ngay: {2:yyyy-MM-dd} | Tong: {3:N0} | DaTT: {4:N0} | ConLai: {5:N0} | TT_TT: {6}",
                            r["ID"], r["SoChungTu"], Convert.ToDateTime(r["NgayNhap"]), r["TongCong"], r["DaThanhToan"], r["ConLai"], r["TrangThaiThanhToan"]));
                    }
                }

                Console.WriteLine("\n=== 2. TẤT CẢ PHIẾU CHI CỦA VINAKEN TRONG THÁNG 8/2026 ===");
                string queryPC = @"
                    SELECT ID, SoPhieuChi, NgayChi, SoTienChi, IDPhieuNhap, TrangThai, IsDeleted, DienGiai
                    FROM KT_PhieuChi
                    WHERE IDNhaCungCap = 1 AND IsDeleted = 0
                    ORDER BY NgayChi, ID";
                using (var cmd = new SqlCommand(queryPC, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PC ID: {0} | So: {1} | Ngay: {2:yyyy-MM-dd} | SoTienChi: {3:N0} | Header_IDPN: {4} | TT: {5} | Note: {6}",
                            r["ID"], r["SoPhieuChi"], Convert.ToDateTime(r["NgayChi"]), r["SoTienChi"], r["IDPhieuNhap"], r["TrangThai"], r["DienGiai"]));
                    }
                }

                Console.WriteLine("\n=== 3. TẤT CẢ CHI TIẾT PHÂN BỔ (KT_PhieuChiChiTiet) CỦA VINAKEN ===");
                string queryCT = @"
                    SELECT ct.ID, ct.IDPhieuChi, pc.SoPhieuChi, pc.NgayChi, pc.TrangThai AS TrangThaiPC, ct.IDPhieuNhap, pn.SoChungTu AS SoPhieuNhap, 
                           ct.LoaiChi, ct.SoTienPhanBo, ct.DienGiai
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    LEFT JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID
                    WHERE pc.IDNhaCungCap = 1 AND pc.IsDeleted = 0
                    ORDER BY pc.NgayChi, pc.ID, ct.ID";
                using (var cmd = new SqlCommand(queryCT, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("CT_ID: {0} | PC: {1} (ID {2}, TT={3}) | PN: {4} (ID {5}) | LoaiChi: {6} | SoTienPB: {7:N0} | Note: {8}",
                            r["ID"], r["SoPhieuChi"], r["IDPhieuChi"], r["TrangThaiPC"], r["SoPhieuNhap"], r["IDPhieuNhap"], r["LoaiChi"], r["SoTienPhanBo"], r["DienGiai"]));
                    }
                }

                Console.WriteLine("\n=== 4. THỐNG KÊ CÔNG NỢ LŨY KẾ & TỔNG NHẬP / TỔNG CHI CỦA VINAKEN ===");
                string queryTong = @"
                    SELECT 
                        (SELECT ISNULL(SUM(TongCong),0) FROM KHO_PhieuNhap WHERE IDNhaCungCap = 1 AND IsDeleted = 0) AS TongNhap,
                        (SELECT ISNULL(SUM(SoTienChi),0) FROM KT_PhieuChi WHERE IDNhaCungCap = 1 AND IsDeleted = 0 AND TrangThai = 2) AS TongChiDaGhiSo,
                        (SELECT ISNULL(SUM(SoTienChi),0) FROM KT_PhieuChi WHERE IDNhaCungCap = 1 AND IsDeleted = 0 AND TrangThai = 1) AS TongChiNhap,
                        (SELECT ISNULL(SUM(SoTienPhanBo),0) FROM KT_PhieuChiChiTiet ct INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID WHERE pc.IDNhaCungCap = 1 AND pc.IsDeleted = 0 AND pc.TrangThai = 2 AND ct.LoaiChi = 1) AS TongPhanBoLoai1,
                        (SELECT ISNULL(SUM(SoTienPhanBo),0) FROM KT_PhieuChiChiTiet ct INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID WHERE pc.IDNhaCungCap = 1 AND pc.IsDeleted = 0 AND pc.TrangThai = 2 AND ct.LoaiChi = 2) AS TongTienTraTruocPhatSinh,
                        (SELECT ISNULL(SUM(SoTienPhanBo),0) FROM KT_PhieuChiChiTiet ct INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID WHERE pc.IDNhaCungCap = 1 AND pc.IsDeleted = 0 AND pc.TrangThai = 2 AND ct.LoaiChi = 3) AS TongTienTraTruocSuDung
                ";
                using (var cmd = new SqlCommand(queryTong, conn))
                using (var r = cmd.ExecuteReader()) {
                    if (r.Read()) {
                        decimal tongNhap = Convert.ToDecimal(r["TongNhap"]);
                        decimal tongChiGhiSo = Convert.ToDecimal(r["TongChiDaGhiSo"]);
                        decimal tongChiNhap = Convert.ToDecimal(r["TongChiNhap"]);
                        decimal tongPB1 = Convert.ToDecimal(r["TongPhanBoLoai1"]);
                        decimal tongTT2 = Convert.ToDecimal(r["TongTienTraTruocPhatSinh"]);
                        decimal tongTT3 = Convert.ToDecimal(r["TongTienTraTruocSuDung"]);
                        Console.WriteLine(string.Format("Tong Nhap: {0:N0}", tongNhap));
                        Console.WriteLine(string.Format("Tong Chi (Da Ghi So): {0:N0}", tongChiGhiSo));
                        Console.WriteLine(string.Format("Tong Chi (Nhap): {0:N0}", tongChiNhap));
                        Console.WriteLine(string.Format("Tong PB Loai 1 (Thanh toan phieu nhap): {0:N0}", tongPB1));
                        Console.WriteLine(string.Format("Tong PB Loai 2 (Tra truoc phat sinh): {0:N0}", tongTT2));
                        Console.WriteLine(string.Format("Tong PB Loai 3 (Tra truoc su dung): {0:N0}", tongTT3));
                        Console.WriteLine(string.Format("Cong no thuc te (TongNhap - TongChiGhiSo): {0:N0}", tongNhap - tongChiGhiSo));
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
