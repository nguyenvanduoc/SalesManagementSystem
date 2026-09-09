using System;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("==========================================================================");
                Console.WriteLine("1. KIỂM TRA PHIẾU NHẬP PN26000192 (ID = 201) TRONG KHO_PHIEUNHAP & CÁC PHÂN BỔ CỦA NÓ");
                Console.WriteLine("==========================================================================");
                string queryPN192 = @"
                    SELECT ID, SoChungTu, NgayNhap, TongCong, DaThanhToan, ConLai, TrangThaiThanhToan
                    FROM KHO_PhieuNhap
                    WHERE SoChungTu = 'PN26000192' OR ID = 201";
                using (var cmd = new SqlCommand(queryPN192, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PN: {0} (ID {1}) | Ngay: {2:yyyy-MM-dd} | TongCong: {3,15:N0} | DaTT(DB): {4,15:N0} | ConLai(DB): {5,15:N0}",
                            r["SoChungTu"], r["ID"], Convert.ToDateTime(r["NgayNhap"]), r["TongCong"], r["DaThanhToan"], r["ConLai"]));
                    }
                }

                Console.WriteLine("\n--- CÁC LẦN PHÂN BỔ VÀO PN26000192 (ID = 201) ---");
                string queryCT192 = @"
                    SELECT ct.ID, ct.IDPhieuChi, pc.SoPhieuChi, pc.NgayChi, pc.TrangThai, ct.SoTienPhanBo, ct.DienGiai
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    WHERE ct.IDPhieuNhap = 201 AND pc.IsDeleted = 0 AND pc.TrangThai = 2";
                using (var cmd = new SqlCommand(queryCT192, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("CT ID: {0} | PC: {1} (ID {2}) | NgayChi: {3:yyyy-MM-dd} | SoTienPB: {4,15:N0} | Note: {5}",
                            r["ID"], r["SoPhieuChi"], r["IDPhieuChi"], Convert.ToDateTime(r["NgayChi"]), r["SoTienPhanBo"], r["DienGiai"]));
                    }
                }

                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("2. CÁC PHIẾU CÓ PHÂN BỔ HOẶC DƯ ĐÚNG 1.680.000 ĐỒNG TRONG HỆ THỐNG");
                Console.WriteLine("==========================================================================");
                string query1680 = @"
                    SELECT ct.ID, ct.IDPhieuChi, pc.SoPhieuChi, pc.NgayChi, ct.IDPhieuNhap, pn.SoChungTu AS SoPhieuNhap, ct.SoTienPhanBo, ct.DienGiai
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    LEFT JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID
                    WHERE (ct.SoTienPhanBo = 1680000 OR ABS(ISNULL(pn.DaThanhToan,0) - ISNULL(pn.TongCong,0)) = 1680000)
                      AND pc.IDNhaCungCap = 1 AND pc.IsDeleted = 0";
                using (var cmd = new SqlCommand(query1680, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("CT ID: {0} | PC: {1} | PN: {2} (ID {3}) | SoTienPB: {4,15:N0} | Note: {5}",
                            r["ID"], r["SoPhieuChi"], r["SoPhieuNhap"], r["IDPhieuNhap"], r["SoTienPhanBo"], r["DienGiai"]));
                    }
                }

                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("3. TẤT CẢ PHIẾU NHẬP ĐANG BỊ DƯ (DaThanhToan > TongCong) HIỆN TẠI VÀ SỐ TIỀN DƯ");
                Console.WriteLine("==========================================================================");
                string queryOverPaidAll = @"
                    SELECT pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TongCong, pn.DaThanhToan, 
                           (pn.DaThanhToan - pn.TongCong) AS SoTienDu
                    FROM KHO_PhieuNhap pn
                    WHERE pn.IDNhaCungCap = 1 AND pn.IsDeleted = 0 AND pn.DaThanhToan > pn.TongCong
                    ORDER BY pn.NgayNhap, pn.SoChungTu";
                using (var cmd = new SqlCommand(queryOverPaidAll, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PN: {0} (ID {1}) | Ngay: {2:yyyy-MM-dd} | TongCong: {3,15:N0} | DaTT: {4,15:N0} | DƯ: {5,15:N0}",
                            r["SoChungTu"], r["ID"], Convert.ToDateTime(r["NgayNhap"]), r["TongCong"], r["DaThanhToan"], r["SoTienDu"]));
                    }
                }

                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("4. CHI TIẾT CÁC PHIẾU CHI CỦA PN26000186 HOẶC XUNG QUANH 1.680.000");
                Console.WriteLine("==========================================================================");
                string queryPN186 = @"
                    SELECT ct.ID, ct.IDPhieuChi, pc.SoPhieuChi, pc.NgayChi, ct.IDPhieuNhap, pn.SoChungTu AS SoPhieuNhap, ct.SoTienPhanBo, ct.DienGiai
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    LEFT JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID
                    WHERE pc.IDNhaCungCap = 1 AND pc.IsDeleted = 0 AND pc.TrangThai = 2
                      AND (pn.SoChungTu IN ('PN26000186', 'PN26000187', 'PN26000192') OR pc.SoPhieuChi = 'PC26000198')
                    ORDER BY pc.SoPhieuChi, ct.ID";
                using (var cmd = new SqlCommand(queryPN186, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PC: {0} | PN: {1} (ID {2}) | SoTienPB: {3,15:N0} | Note: {4}",
                            r["SoPhieuChi"], r["SoPhieuNhap"], r["IDPhieuNhap"], r["SoTienPhanBo"], r["DienGiai"]));
                    }
                }

            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
