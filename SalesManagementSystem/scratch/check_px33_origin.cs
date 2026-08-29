using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace CheckPX33Origin
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

                    Console.WriteLine("=== 1. PX00033 FULL ROW IN KHO_PHIEUXUAT ===");
                    var px = conn.QueryFirstOrDefault(@"
                        SELECT px.*, k.TenKhoHang, ctbh.SoChungTu as SoBH, dh.SoDonHang
                        FROM KHO_PhieuXuat px
                        LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
                        LEFT JOIN BAN_ChungTuBanHang ctbh ON px.IDChungTuBanHang = ctbh.ID
                        LEFT JOIN NS_DonDatHang dh ON px.IDDonDatHang = dh.ID
                        WHERE px.SoChungTu = 'PX00033'
                    ");

                    if (px != null)
                    {
                        Console.WriteLine(string.Format("ID: {0} | SoChungTu: {1} | NgayXuat: {2:dd/MM/yyyy} | IDKho: {3} ({4}) | TrangThai: {5} | IDChungTuBanHang: {6} (SoBH: {7}) | IDDonDatHang: {8} (SoDH: {9}) | NguoiTao: {10} | NgayTao: {11} | NguoiGhi: {12} | NgayGhi: {13} | GhiChu: {14}",
                            px.ID, px.SoChungTu, Convert.ToDateTime(px.NgayXuat), px.IDKho, px.TenKhoHang, px.TrangThai, px.IDChungTuBanHang, px.SoBH, px.IDDonDatHang, px.SoDonHang, px.NguoiTao, px.NgayTao, px.NguoiGhi, px.NgayGhi, px.GhiChu));
                    }

                    Console.WriteLine("\n=== 2. SALES INVOICE (BH00033 OR CONNECTED) ===");
                    var bh = conn.QueryFirstOrDefault(@"
                        SELECT ctbh.*, kh.TenKhachHang
                        FROM BAN_ChungTuBanHang ctbh
                        LEFT JOIN NS_KhachHang kh ON ctbh.IDKhachHang = kh.ID
                        WHERE ctbh.SoChungTu LIKE '%00033%' OR ctbh.ID = @IDBH
                    ", new { IDBH = px != null ? (int?)px.IDChungTuBanHang : null });

                    if (bh != null)
                    {
                        Console.WriteLine(string.Format("ID: {0} | SoChungTu: {1} | NgayChungTu: {2:dd/MM/yyyy} | TrangThai: {3} | KhachHang: {4} | TongTien: {5}",
                            bh.ID, bh.SoChungTu, Convert.ToDateTime(bh.NgayChungTu), bh.TrangThai, bh.TenKhachHang, bh.TongCong));

                        var bh_ct = conn.Query(@"
                            SELECT ct.*, sp.MaSanPham, sp.TenSanPham
                            FROM BAN_ChungTuBanHang_ChiTiet ct
                            LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                            WHERE ct.IDChungTuBanHang = @ID
                        ", new { ID = (int)bh.ID }).ToList();

                        Console.WriteLine("Details of Sales Invoice:");
                        foreach (var c in bh_ct)
                        {
                            Console.WriteLine(string.Format("  - IDChiTiet: {0} | SP: {1} ({2}) | SoLuong: {3} | DonGia: {4}",
                                c.ID, c.MaSanPham, c.TenSanPham, c.SoLuong, c.DonGia));
                        }
                    }

                    Console.WriteLine("\n=== 3. SALES ORDER (DH26000032 OR CONNECTED) ===");
                    var dh = conn.QueryFirstOrDefault(@"
                        SELECT dh.*, kh.TenKhachHang
                        FROM NS_DonDatHang dh
                        LEFT JOIN NS_KhachHang kh ON dh.IDKhachHang = kh.ID
                        WHERE dh.SoDonHang = 'DH26000032' OR dh.ID = @IDDH
                    ", new { IDDH = px != null ? (int?)px.IDDonDatHang : null });

                    if (dh != null)
                    {
                        Console.WriteLine(string.Format("ID: {0} | SoDonHang: {1} | NgayTao: {2:dd/MM/yyyy} | TrangThai: {3} | KhachHang: {4}",
                            dh.ID, dh.SoDonHang, Convert.ToDateTime(dh.NgayTaoDon), dh.TrangThaiDon, dh.TenKhachHang));

                        var dh_ct = conn.Query(@"
                            SELECT ct.*, sp.MaSanPham, sp.TenSanPham
                            FROM NS_DonDatHang_ChiTiet ct
                            LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                            WHERE ct.IDDonDatHang = @ID
                        ", new { ID = (int)dh.ID }).ToList();

                        Console.WriteLine("Details of Sales Order:");
                        foreach (var c in dh_ct)
                        {
                            Console.WriteLine(string.Format("  - IDChiTiet: {0} | SP: {1} ({2}) | SoLuong: {3}",
                                c.ID, c.MaSanPham, c.TenSanPham, c.SoLuong));
                        }
                    }

                    Console.WriteLine("\n=== 4. CHECK WHY GIAODICHKHO HAS MISSING ENTRIES FOR PX00033 & OTHERS ===");
                    var deleted_gd = conn.Query("SELECT * FROM KHO_GiaoDichKho WHERE SoChungTu = 'PX00033'").ToList();
                    Console.WriteLine(string.Format("Total records in KHO_GiaoDichKho for PX00033: {0}", deleted_gd.Count));
                    foreach (var g in deleted_gd)
                    {
                        Console.WriteLine(string.Format("GD ID: {0} | LoaiChungTu: {1} | IDChiTietKho: {2} | IDKho: {3} | IDSanPham: {4} | Xuat: {5} | IsHuy: {6}",
                            g.ID, g.LoaiChungTu, g.IDChiTietKho, g.IDKho, g.IDSanPham, g.SoLuongXuat, g.IsHuy));
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
