using System;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connStr = @"Data Source=localhost\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();

                Console.WriteLine("=== 1. TẤT CẢ PHIẾU NHẬP VÀ SỐ TIỀN DƯ/THIẾU SO VỚI TỔNG CỘNG ===");
                string queryPN = @"
                    SELECT pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TongCong, pn.DaThanhToan, (pn.TongCong - pn.DaThanhToan) AS ConNo,
                           ISNULL((SELECT SUM(ct.SoTienPhanBo) FROM KT_PhieuChiChiTiet ct INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID WHERE ct.IDPhieuNhap = pn.ID AND pc.IsDeleted = 0 AND pc.TrangThai = 2), 0) AS TongPB
                    FROM KHO_PhieuNhap pn
                    WHERE pn.IDNhaCungCap = 1 AND pn.IsDeleted = 0
                    ORDER BY pn.NgayNhap, pn.ID";
                using (var cmd = new SqlCommand(queryPN, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        decimal tong = Convert.ToDecimal(r["TongCong"]);
                        decimal daTT = Convert.ToDecimal(r["DaThanhToan"]);
                        decimal tongPB = Convert.ToDecimal(r["TongPB"]);
                        decimal diff = tongPB - tong;
                        if (diff != 0 || (tong - daTT) != 0) {
                            Console.WriteLine(string.Format("PN: {0} (ID {1}) | Ngay: {2:yyyy-MM-dd} | Tong: {3,15:N0} | DaTT_Col: {4,15:N0} | TongPB_ChiTiet: {5,15:N0} | ChechLech(PB-Tong): {6,15:N0}",
                                r["SoChungTu"], r["ID"], Convert.ToDateTime(r["NgayNhap"]), tong, daTT, tongPB, diff));
                        }
                    }
                }

                Console.WriteLine("\n=== 2. TÌM SỐ TIỀN 1.680.000 HOẶC CÁC PHIẾU NHẬP CÓ SỐ DƯ LẺ KHÁC ===");
                string queryCheck = @"
                    SELECT pn.SoChungTu, pn.NgayNhap, pn.TongCong, pn.DaThanhToan, (pn.DaThanhToan - pn.TongCong) AS Du
                    FROM KHO_PhieuNhap pn
                    WHERE pn.IDNhaCungCap = 1 AND pn.IsDeleted = 0 AND pn.DaThanhToan <> pn.TongCong
                    ORDER BY pn.NgayNhap";
                using (var cmd = new SqlCommand(queryCheck, conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) {
                        Console.WriteLine(string.Format("PN: {0} | Ngay: {1:yyyy-MM-dd} | Tong: {2,15:N0} | DaTT: {3,15:N0} | Le/Du/Thieu: {4,15:N0}",
                            r["SoChungTu"], Convert.ToDateTime(r["NgayNhap"]), r["TongCong"], r["DaThanhToan"], r["Du"]));
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
