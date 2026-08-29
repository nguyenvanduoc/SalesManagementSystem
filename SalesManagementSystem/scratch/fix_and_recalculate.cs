using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace FixAndRecalculate
{
    class Program
    {
        static void Main()
        {
            try
            {
                var db = new DbConnectionFactory();
                using (var conn = db.CreateConnection())
                {
                    conn.Open();

                    Console.WriteLine("=================================================");
                    Console.WriteLine("1. CHECK ALL BAN_ChungTuBanHang_ChiTiet WITH ABNORMAL COST PRICE");
                    Console.WriteLine("=================================================");

                    var abnormalItems = conn.Query<dynamic>(@"
                        SELECT ct.ID, bh.SoChungTu, bh.NgayChungTu, sp.MaSanPham, sp.TenSanPham, 
                               ct.SoLuong, ct.DonGia AS DonGiaBan, ct.ThanhTien AS ThanhTienBan, 
                               ct.DonGiaVon, ct.ThanhTienVon, ap.AvgDonGia
                        FROM BAN_ChungTuBanHang_ChiTiet ct
                        INNER JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
                        LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                        OUTER APPLY (
                            SELECT SUM(pn_ct.SoLuong * pn_ct.DonGia) / NULLIF(SUM(pn_ct.SoLuong), 0) AS AvgDonGia
                            FROM KHO_PhieuNhap_ChiTiet pn_ct
                            INNER JOIN KHO_PhieuNhap pn ON pn_ct.IDPhieuNhap = pn.ID
                            WHERE pn_ct.IDSanPham = ct.IDSanPham 
                              AND pn.TrangThai = 2 AND pn.IsDeleted = 0
                        ) ap
                        WHERE bh.IsDeleted = 0 
                          AND (ct.DonGiaVon > ct.DonGia * 2 OR ct.DonGiaVon > ISNULL(ap.AvgDonGia, 0) * 3)").ToList();

                    Console.WriteLine(string.Format("Total abnormal cost rows found: {0}", abnormalItems.Count));
                    foreach (var item in abnormalItems)
                    {
                        Console.WriteLine(string.Format("ID: {0} | SoCT: {1} | Ngay: {2:dd/MM/yyyy} | SP: {3}-{4} | SL: {5:N0} | DonGiaBan: {6:N0} | DonGiaVon: {7:N0} | AvgDonGia: {8:N0}",
                            item.ID, item.SoChungTu, item.NgayChungTu, item.MaSanPham, item.TenSanPham, item.SoLuong, item.DonGiaBan, item.DonGiaVon, item.AvgDonGia));
                    }

                    Console.WriteLine("\n=================================================");
                    Console.WriteLine("2. FIX ABNORMAL DATA IN BAN_ChungTuBanHang_ChiTiet (e.g. ID 471 in BH00206)");
                    Console.WriteLine("=================================================");

                    // Update ID 471 (BH00206) with correct DonGiaVon from AvgDonGia or DonGiaVon = 58225
                    int rowsUpdated = conn.Execute(@"
                        UPDATE ct
                        SET ct.DonGiaVon = ISNULL(ap.AvgDonGia, 58225),
                            ct.ThanhTienVon = ct.SoLuong * ISNULL(ap.AvgDonGia, 58225)
                        FROM BAN_ChungTuBanHang_ChiTiet ct
                        INNER JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
                        OUTER APPLY (
                            SELECT SUM(pn_ct.SoLuong * pn_ct.DonGia) / NULLIF(SUM(pn_ct.SoLuong), 0) AS AvgDonGia
                            FROM KHO_PhieuNhap_ChiTiet pn_ct
                            INNER JOIN KHO_PhieuNhap pn ON pn_ct.IDPhieuNhap = pn.ID
                            WHERE pn_ct.IDSanPham = ct.IDSanPham 
                              AND pn.TrangThai = 2 AND pn.IsDeleted = 0
                        ) ap
                        WHERE ct.ID = 471 OR (ct.DonGiaVon > ct.DonGia * 2 AND ct.DonGia > 0)");

                    Console.WriteLine(string.Format("Rows updated in database: {0}", rowsUpdated));

                    Console.WriteLine("\n=================================================");
                    Console.WriteLine("3. UPDATE AND EXECUTE sp_Dashboard_GetData IN DATABASE");
                    Console.WriteLine("=================================================");

                    string sqlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "sp_Dashboard_GetData.sql");
                    if (System.IO.File.Exists(sqlPath))
                    {
                        string sqlContent = System.IO.File.ReadAllText(sqlPath);
                        var parts = sqlContent.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            if (!string.IsNullOrWhiteSpace(part))
                            {
                                conn.Execute(part);
                            }
                        }
                        Console.WriteLine("Successfully re-executed sp_Dashboard_GetData in database!");
                    }

                    Console.WriteLine("\n=================================================");
                    Console.WriteLine("4. VERIFY DASHBOARD SUMMARY RESULTS FOR AUGUST 2026");
                    Console.WriteLine("=================================================");

                    DateTime tuNgay = new DateTime(2026, 8, 1);
                    DateTime denNgay = new DateTime(2026, 8, 31, 23, 59, 59);

                    using (var multi = conn.QueryMultiple("sp_Dashboard_GetData", new {
                        TuNgay = tuNgay,
                        DenNgay = denNgay,
                        TuNgayKyTruoc = new DateTime(2026, 7, 1),
                        DenNgayKyTruoc = new DateTime(2026, 7, 31, 23, 59, 59)
                    }, commandType: CommandType.StoredProcedure))
                    {
                        var summary = multi.Read<dynamic>().FirstOrDefault();
                        if (summary != null)
                        {
                            var dict = (System.Collections.Generic.IDictionary<string, object>)summary;
                            Console.WriteLine(string.Format("Doanh Thu       : {0:N0} VND", dict["DoanhThu"]));
                            Console.WriteLine(string.Format("Lợi Nhuận       : {0:N0} VND", dict["LoiNhuan"]));
                            Console.WriteLine(string.Format("Doanh Thu Kỳ Tr: {0:N0} VND", dict["DoanhThuKyTruoc"]));
                            Console.WriteLine(string.Format("Lợi Nhuận Kỳ Tr: {0:N0} VND", dict["LoiNhuanKyTruoc"]));
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX: " + ex);
            }
        }
    }
}
