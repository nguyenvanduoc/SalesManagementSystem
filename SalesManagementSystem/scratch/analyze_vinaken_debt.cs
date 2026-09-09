using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("==========================================================================");
                Console.WriteLine("1. KIỂM TRA PHIẾU NHẬP PN26000198 VÀ PN26000204 TRONG DATABASE");
                Console.WriteLine("==========================================================================");
                string queryTargetPN = @"
                    SELECT pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TongCong, pn.DaThanhToan, pn.ConLai, pn.TrangThaiThanhToan, pn.TrangThai, pn.IsDeleted
                    FROM KHO_PhieuNhap pn
                    WHERE pn.SoChungTu IN ('PN26000198', 'PN26000199', 'PN26000201', 'PN26000202', 'PN26000203', 'PN26000204')
                       OR pn.ID IN (SELECT IDPhieuNhap FROM KT_PhieuChiChiTiet WHERE IDPhieuNhap IS NOT NULL)
                    ORDER BY pn.SoChungTu";
                using (var cmd = new SqlCommand(@"SELECT ID, SoChungTu, NgayNhap, ISNULL(TongCong,0) AS TongCong, ISNULL(DaThanhToan,0) AS DaThanhToan, ISNULL(ConLai,0) AS ConLai, TrangThaiThanhToan 
                                                  FROM KHO_PhieuNhap WHERE IDNhaCungCap = 1 AND IsDeleted = 0 ORDER BY ID", conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        string so = r["SoChungTu"].ToString();
                        decimal tong = Convert.ToDecimal(r["TongCong"]);
                        decimal daTT = Convert.ToDecimal(r["DaThanhToan"]);
                        decimal conLai = Convert.ToDecimal(r["ConLai"]);
                        int ttTT = Convert.ToInt32(r["TrangThaiThanhToan"]);
                        Console.WriteLine(string.Format("PN: {0} (ID {1}) | Ngay: {2:yyyy-MM-dd} | Tong: {3,15:N0} | DaTT: {4,15:N0} | ConLai: {5,15:N0} | TT_TT: {6}",
                            so, r["ID"], Convert.ToDateTime(r["NgayNhap"]), tong, daTT, conLai, ttTT));
                    }
                }

                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("2. KIỂM TRA CÁC PHIẾU NHẬP BỊ CHI DƯ / PHÂN BỔ DƯ (DaThanhToan > TongCong HOẶC Sum_PhanBo > TongCong)");
                Console.WriteLine("==========================================================================");
                string queryOverPaid = @"
                    SELECT pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TongCong, pn.DaThanhToan AS DB_DaThanhToan,
                           ISNULL(SUM(CASE WHEN pc.TrangThai = 2 AND pc.IsDeleted = 0 AND ct.LoaiChi = 1 THEN ct.SoTienPhanBo ELSE 0 END), 0) AS Sum_PB_DaGhiSo
                    FROM KHO_PhieuNhap pn
                    LEFT JOIN KT_PhieuChiChiTiet ct ON pn.ID = ct.IDPhieuNhap
                    LEFT JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    WHERE pn.IDNhaCungCap = 1 AND pn.IsDeleted = 0
                    GROUP BY pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TongCong, pn.DaThanhToan
                    HAVING ISNULL(SUM(CASE WHEN pc.TrangThai = 2 AND pc.IsDeleted = 0 AND ct.LoaiChi = 1 THEN ct.SoTienPhanBo ELSE 0 END), 0) > pn.TongCong
                        OR pn.DaThanhToan > pn.TongCong
                    ORDER BY pn.NgayNhap, pn.SoChungTu";
                decimal totalOverPaid = 0;
                using (var cmd = new SqlCommand(queryOverPaid, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        decimal tong = Convert.ToDecimal(r["TongCong"]);
                        decimal sumPB = Convert.ToDecimal(r["Sum_PB_DaGhiSo"]);
                        decimal du = sumPB - tong;
                        totalOverPaid += du;
                        Console.WriteLine(string.Format("PN: {0} (ID {1}) | TongCong: {2,15:N0} | DaPB: {3,15:N0} | DƯ: {4,15:N0}",
                            r["SoChungTu"], r["ID"], tong, sumPB, du));
                    }
                }
                Console.WriteLine(string.Format("==> TỔNG CỘNG SỐ TIỀN BỊ PHÂN BỔ DƯ (CHI DƯ VÀO CÁC PHIẾU NHẬP CŨ): {0:N0}", totalOverPaid));

                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("3. CHI TIẾT CÁC PHIẾU CHI ĐÃ PHÂN BỔ VÀO CÁC PHIẾU NHẬP BỊ DƯ");
                Console.WriteLine("==========================================================================");
                string queryPC_OverPaid = @"
                    SELECT pc.SoPhieuChi, pc.NgayChi, pc.SoTienChi, pc.TrangThai, ct.IDPhieuNhap, pn.SoChungTu AS SoPhieuNhap, ct.SoTienPhanBo, ct.DienGiai
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    INNER JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID
                    WHERE pc.IDNhaCungCap = 1 AND pc.IsDeleted = 0 AND pc.TrangThai = 2
                      AND pn.ID IN (
                          SELECT pn2.ID FROM KHO_PhieuNhap pn2 
                          LEFT JOIN KT_PhieuChiChiTiet ct2 ON pn2.ID = ct2.IDPhieuNhap
                          LEFT JOIN KT_PhieuChi pc2 ON ct2.IDPhieuChi = pc2.ID
                          WHERE pn2.IDNhaCungCap = 1 AND pn2.IsDeleted = 0
                          GROUP BY pn2.ID, pn2.TongCong
                          HAVING SUM(CASE WHEN pc2.TrangThai = 2 AND pc2.IsDeleted = 0 AND ct2.LoaiChi = 1 THEN ct2.SoTienPhanBo ELSE 0 END) > pn2.TongCong
                      )
                    ORDER BY pn.SoChungTu, pc.SoPhieuChi";
                using (var cmd = new SqlCommand(queryPC_OverPaid, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PN: {0} | PC: {1} | NgayChi: {2:yyyy-MM-dd} | SoTienChi: {3,15:N0} | SoTienPB: {4,15:N0} | Note: {5}",
                            r["SoPhieuNhap"], r["SoPhieuChi"], Convert.ToDateTime(r["NgayChi"]), r["SoTienChi"], r["SoTienPhanBo"], r["DienGiai"]));
                    }
                }

                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("4. SO SÁNH TỔNG CÔNG NỢ NCC (BÁO CÁO CÔNG NỢ VS PHIẾU CHI)");
                Console.WriteLine("==========================================================================");
                string queryCompare = @"
                    SELECT 
                        (SELECT ISNULL(SUM(TongCong),0) FROM KHO_PhieuNhap WHERE IDNhaCungCap = 1 AND IsDeleted = 0) AS TongTienHang,
                        (SELECT ISNULL(SUM(SoTienChi),0) FROM KT_PhieuChi WHERE IDNhaCungCap = 1 AND IsDeleted = 0 AND TrangThai = 2) AS TongSoTienChi,
                        (SELECT ISNULL(SUM(SoTienPhanBo),0) FROM KT_PhieuChiChiTiet ct INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID WHERE pc.IDNhaCungCap = 1 AND pc.IsDeleted = 0 AND pc.TrangThai = 2 AND ct.LoaiChi = 1) AS TongDaPhanBoLoai1,
                        (SELECT ISNULL(SUM(DaThanhToan),0) FROM KHO_PhieuNhap WHERE IDNhaCungCap = 1 AND IsDeleted = 0) AS TongDaThanhToanColumnInPN
                ";
                using (var cmd = new SqlCommand(queryCompare, conn))
                using (var r = cmd.ExecuteReader()) {
                    if (r.Read()) {
                        decimal tongNhap = Convert.ToDecimal(r["TongTienHang"]);
                        decimal tongChi = Convert.ToDecimal(r["TongSoTienChi"]);
                        decimal tongPB = Convert.ToDecimal(r["TongDaPhanBoLoai1"]);
                        decimal tongColDaTT = Convert.ToDecimal(r["TongDaThanhToanColumnInPN"]);
                        Console.WriteLine(string.Format("Tổng tiền nhập hàng (KHO_PhieuNhap.TongCong):               {0,18:N0}", tongNhap));
                        Console.WriteLine(string.Format("Tổng tiền đã chi ghi sổ (KT_PhieuChi.SoTienChi, TT=2):      {0,18:N0}", tongChi));
                        Console.WriteLine(string.Format("Tổng tiền đã phân bổ vào PN (KT_PhieuChiChiTiet, LoaiChi=1): {0,18:N0}", tongPB));
                        Console.WriteLine(string.Format("Tổng Cột DaThanhToan trong KHO_PhieuNhap:                  {0,18:N0}", tongColDaTT));
                        Console.WriteLine(string.Format("Còn nợ tổng thể nhà cung cấp (TongNhap - TongChi):          {0,18:N0}", tongNhap - tongChi));
                        Console.WriteLine(string.Format("Chênh lệch giữa Tổng Chi và Tổng Phân Bổ Loai 1:            {0,18:N0}", tongChi - tongPB));
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
