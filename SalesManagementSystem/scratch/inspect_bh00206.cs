using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace InspectBH00206
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
                    Console.WriteLine("INSPECT VOUCHER BH00206 DETAILS");
                    Console.WriteLine("=================================================");

                    var bh = conn.QueryFirstOrDefault<dynamic>(@"
                        SELECT ID, SoChungTu, NgayChungTu, IDKho, IDKhachHang, TongTienHang, TongCong, TrangThai, IsDeleted
                        FROM BAN_ChungTuBanHang
                        WHERE SoChungTu = 'BH00206'");

                    if (bh != null)
                    {
                        Console.WriteLine(string.Format("BH00206 ID: {0} | Ngay: {1:dd/MM/yyyy} | TongCong: {2:N0}", bh.ID, bh.NgayChungTu, bh.TongCong));

                        var details = conn.Query<dynamic>(@"
                            SELECT ct.ID, ct.IDSanPham, sp.MaSanPham, sp.TenSanPham, ct.SoLuong, ct.DonGia, ct.ThanhTien, ct.DonGiaVon, ct.ThanhTienVon
                            FROM BAN_ChungTuBanHang_ChiTiet ct
                            LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                            WHERE ct.IDChungTuBanHang = @ID", new { ID = (int)bh.ID }).ToList();

                        foreach (var d in details)
                        {
                            Console.WriteLine(string.Format("Detail ID: {0} | SP: {1}-{2} | SL: {3:N0} | DonGiaBan: {4:N0} | ThanhTienBan: {5:N0} | DonGiaVon: {6:N0} | ThanhTienVon: {7:N0}",
                                d.ID, d.MaSanPham, d.TenSanPham, d.SoLuong, d.DonGia, d.ThanhTien, d.DonGiaVon, d.ThanhTienVon));
                        }
                    }

                    Console.WriteLine("\n=================================================");
                    Console.WriteLine("CHECK AVERAGE IMPORT PRICE FOR PRODUCT zocao IN KHO_PhieuNhap_ChiTiet");
                    Console.WriteLine("=================================================");

                    var avgImport = conn.QueryFirstOrDefault<dynamic>(@"
                        SELECT 
                            SUM(pn_ct.SoLuong * pn_ct.DonGia) / NULLIF(SUM(pn_ct.SoLuong), 0) AS AvgDonGia,
                            SUM(pn_ct.SoLuong) AS SumSL,
                            SUM(pn_ct.SoLuong * pn_ct.DonGia) AS SumThanhTien
                        FROM KHO_PhieuNhap_ChiTiet pn_ct
                        INNER JOIN KHO_PhieuNhap pn ON pn_ct.IDPhieuNhap = pn.ID
                        INNER JOIN DM_SanPham sp ON pn_ct.IDSanPham = sp.ID
                        WHERE sp.MaSanPham LIKE '%zocao%' AND pn.TrangThai = 2 AND pn.IsDeleted = 0");

                    if (avgImport != null)
                    {
                        Console.WriteLine(string.Format("zocao AvgDonGia: {0:N0} | SumSL: {1:N0} | SumThanhTien: {2:N0}",
                            avgImport.AvgDonGia, avgImport.SumSL, avgImport.SumThanhTien));
                    }

                    Console.WriteLine("\n=================================================");
                    Console.WriteLine("ALL DETAILS IN BAN_ChungTuBanHang_ChiTiet WHERE DonGiaVon > 300,000");
                    Console.WriteLine("=================================================");

                    var highVonDetails = conn.Query<dynamic>(@"
                        SELECT ct.ID, bh.SoChungTu, bh.NgayChungTu, sp.MaSanPham, sp.TenSanPham, ct.SoLuong, ct.DonGia, ct.ThanhTien, ct.DonGiaVon, ct.ThanhTienVon
                        FROM BAN_ChungTuBanHang_ChiTiet ct
                        INNER JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
                        LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                        WHERE ct.DonGiaVon > 300000 AND bh.IsDeleted = 0").ToList();

                    foreach (var h in highVonDetails)
                    {
                        Console.WriteLine(string.Format("ID: {0} | SoCT: {1} | Ngay: {2:dd/MM/yyyy} | SP: {3}-{4} | SL: {5:N0} | DonGiaBan: {6:N0} | DonGiaVon: {7:N0} | ThanhTienVon: {8:N0}",
                            h.ID, h.SoChungTu, h.NgayChungTu, h.MaSanPham, h.TenSanPham, h.SoLuong, h.DonGia, h.DonGiaVon, h.ThanhTienVon));
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
