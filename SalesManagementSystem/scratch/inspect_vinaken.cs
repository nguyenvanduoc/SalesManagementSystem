using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string webConfigPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Web.config";
            var map = new ExeConfigurationFileMap { ExeConfigFilename = webConfigPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            
            string connStr = null;
            foreach(ConnectionStringSettings cs in config.ConnectionStrings.ConnectionStrings) {
                if (cs.Name == "LocalSqlServer") continue;
                connStr = cs.ConnectionString;
                break;
            }
            if (string.IsNullOrEmpty(connStr)) {
                connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";
            }

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("=== 1. NHÀ CUNG CẤP VINAKEN ===");
                using (var cmd = new SqlCommand("SELECT ID, MaNhaCungCap, TenNhaCungCap FROM DM_NhaCungCap WHERE TenNhaCungCap LIKE N'%Vinaken%' OR MaNhaCungCap LIKE '%NCC001%'", conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("ID: {0}, Ma: {1}, Ten: {2}", r["ID"], r["MaNhaCungCap"], r["TenNhaCungCap"]));
                    }
                }

                Console.WriteLine("\n=== 2. PHIẾU NHẬP PN26000198 & PN26000204 (và tất cả phiếu nhập của Vinaken) ===");
                string queryPN = @"
                    SELECT ID, SoChungTu, NgayNhap, ISNULL(TongCong, 0) AS TongCong, ISNULL(DaThanhToan, 0) AS DaThanhToan, 
                           ISNULL(ConLai, 0) AS ConLai, TrangThaiThanhToan, TrangThai, IsDeleted, IDNhaCungCap
                    FROM KHO_PhieuNhap
                    WHERE IDNhaCungCap IN (SELECT ID FROM DM_NhaCungCap WHERE TenNhaCungCap LIKE N'%Vinaken%' OR MaNhaCungCap LIKE '%NCC001%')
                       OR SoChungTu IN ('PN26000198', 'PN26000204')
                    ORDER BY NgayNhap, SoChungTu";
                using (var cmd = new SqlCommand(queryPN, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PN ID: {0}, So: {1}, Ngay: {2:yyyy-MM-dd}, TongCong: {3:N0}, DaThanhToan(Col): {4:N0}, ConLai(Col): {5:N0}, TTThanhToan: {6}, TrangThai: {7}, IsDel: {8}",
                            r["ID"], r["SoChungTu"], Convert.ToDateTime(r["NgayNhap"]), r["TongCong"], r["DaThanhToan"], r["ConLai"], r["TrangThaiThanhToan"], r["TrangThai"], r["IsDeleted"]));
                    }
                }

                Console.WriteLine("\n=== 3. PHIẾU CHI CỦA VINAKEN ===");
                string queryPC = @"
                    SELECT ID, SoPhieuChi, NgayChi, SoTienChi, IDNhaCungCap, IDPhieuNhap, TrangThai, IsDeleted, DienGiai
                    FROM KT_PhieuChi
                    WHERE IDNhaCungCap IN (SELECT ID FROM DM_NhaCungCap WHERE TenNhaCungCap LIKE N'%Vinaken%' OR MaNhaCungCap LIKE '%NCC001%')
                      AND IsDeleted = 0
                    ORDER BY NgayChi, SoPhieuChi";
                using (var cmd = new SqlCommand(queryPC, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PC ID: {0}, So: {1}, Ngay: {2:yyyy-MM-dd}, SoTienChi: {3:N0}, IDPN: {4}, TrangThai: {5}, IsDel: {6}, DienGiai: {7}",
                            r["ID"], r["SoPhieuChi"], Convert.ToDateTime(r["NgayChi"]), r["SoTienChi"], r["IDPhieuNhap"], r["TrangThai"], r["IsDeleted"], r["DienGiai"]));
                    }
                }

                Console.WriteLine("\n=== 4. CHI TIẾT PHÂN BỔ PHIẾU CHI (KT_PhieuChiChiTiet) CỦA VINAKEN ===");
                string queryCT = @"
                    SELECT ct.ID, ct.IDPhieuChi, pc.SoPhieuChi, pc.NgayChi, pc.TrangThai AS TrangThaiPC, ct.IDPhieuNhap, pn.SoChungTu AS SoPhieuNhap, 
                           ct.LoaiChi, ct.SoTienPhanBo, ct.DienGiai
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    LEFT JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID
                    WHERE pc.IDNhaCungCap IN (SELECT ID FROM DM_NhaCungCap WHERE TenNhaCungCap LIKE N'%Vinaken%' OR MaNhaCungCap LIKE '%NCC001%')
                      AND pc.IsDeleted = 0
                    ORDER BY pc.NgayChi, pc.SoPhieuChi, ct.ID";
                using (var cmd = new SqlCommand(queryCT, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("CT ID: {0}, PC: {1} (ID {2}, TT={3}), PN: {4} (ID {5}), LoaiChi: {6}, SoTienPB: {7:N0}, Note: {8}",
                            r["ID"], r["SoPhieuChi"], r["IDPhieuChi"], r["TrangThaiPC"], r["SoPhieuNhap"], r["IDPhieuNhap"], r["LoaiChi"], r["SoTienPhanBo"], r["DienGiai"]));
                    }
                }

                Console.WriteLine("\n=== 5. TỔNG SỐ TIỀN PHÂN BỔ CHO TỪNG PHIẾU NHẬP (TỪ KT_PhieuChiChiTiet vs KHO_PhieuNhap.DaThanhToan) ===");
                string querySumPN = @"
                    SELECT pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TongCong, pn.DaThanhToan AS DB_DaThanhToan, pn.ConLai AS DB_ConLai,
                           ISNULL(SUM(CASE WHEN pc.TrangThai = 2 AND pc.IsDeleted = 0 AND ct.LoaiChi = 1 THEN ct.SoTienPhanBo ELSE 0 END), 0) AS Sum_PhanBo_DaGhiSo,
                           ISNULL(SUM(CASE WHEN pc.TrangThai = 1 AND pc.IsDeleted = 0 AND ct.LoaiChi = 1 THEN ct.SoTienPhanBo ELSE 0 END), 0) AS Sum_PhanBo_Nháp
                    FROM KHO_PhieuNhap pn
                    LEFT JOIN KT_PhieuChiChiTiet ct ON pn.ID = ct.IDPhieuNhap
                    LEFT JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    WHERE pn.IDNhaCungCap IN (SELECT ID FROM DM_NhaCungCap WHERE TenNhaCungCap LIKE N'%Vinaken%' OR MaNhaCungCap LIKE '%NCC001%')
                      AND pn.IsDeleted = 0
                    GROUP BY pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TongCong, pn.DaThanhToan, pn.ConLai
                    ORDER BY pn.NgayNhap, pn.SoChungTu";
                using (var cmd = new SqlCommand(querySumPN, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        decimal tong = Convert.ToDecimal(r["TongCong"]);
                        decimal dbDaTT = Convert.ToDecimal(r["DB_DaThanhToan"]);
                        decimal sumGhiSo = Convert.ToDecimal(r["Sum_PhanBo_DaGhiSo"]);
                        decimal sumNhap = Convert.ToDecimal(r["Sum_PhanBo_Nháp"]);
                        Console.WriteLine(string.Format("PN {0} (ID {1}): Tong={2:N0} | DB_DaTT={3:N0} | Sum_GhiSo={4:N0} | Sum_Nhap={5:N0} | ConLai_GhiSo={6:N0}",
                            r["SoChungTu"], r["ID"], tong, dbDaTT, sumGhiSo, sumNhap, tong - sumGhiSo));
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
