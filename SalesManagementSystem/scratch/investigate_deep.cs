using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace InvestigateDeep
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

                    Console.WriteLine("=========================================================");
                    Console.WriteLine("1. CHECK PX00033 IN ALL TABLES");
                    Console.WriteLine("=========================================================");
                    
                    var px33 = conn.QueryFirstOrDefault(@"
                        SELECT px.*, k.TenKhoHang
                        FROM KHO_PhieuXuat px
                        LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
                        WHERE px.SoChungTu = 'PX00033'
                    ");
                    if (px33 != null)
                    {
                        Console.WriteLine(string.Format("PX00033 -> ID: {0}, NgayXuat: {1:dd/MM/yyyy}, Kho: {2} (ID={3}), TrangThai: {4}, IsDeleted: {5}, GhiChu: {6}",
                            px33.ID, Convert.ToDateTime(px33.NgayXuat), px33.TenKhoHang, px33.IDKho, px33.TrangThai, px33.IsDeleted, px33.GhiChu));

                        var px33_ct = conn.Query(@"
                            SELECT ct.*, sp.MaSanPham, sp.TenSanPham
                            FROM KHO_PhieuXuat_ChiTiet ct
                            LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                            WHERE ct.IDPhieuXuat = @ID
                        ", new { ID = (int)px33.ID }).ToList();

                        Console.WriteLine("Details in KHO_PhieuXuat_ChiTiet:");
                        foreach (var c in px33_ct)
                        {
                            Console.WriteLine(string.Format("  - IDChiTiet: {0}, IDSanPham: {1}, MaSP: {2}, TenSP: {3}, SoLuong: {4}",
                                c.ID, c.IDSanPham, c.MaSanPham, c.TenSanPham, c.SoLuong));
                        }

                        var px33_gd = conn.Query(@"
                            SELECT gd.*, sp.MaSanPham, k.TenKhoHang
                            FROM KHO_GiaoDichKho gd
                            LEFT JOIN DM_SanPham sp ON gd.IDSanPham = sp.ID
                            LEFT JOIN DM_KhoHang k ON gd.IDKho = k.ID
                            WHERE gd.SoChungTu = 'PX00033'
                        ").ToList();

                        Console.WriteLine("Entries in KHO_GiaoDichKho:");
                        foreach (var g in px33_gd)
                        {
                            Console.WriteLine(string.Format("  - IDGD: {0}, IDChiTietKho: {1}, Kho: {2} (ID={3}), SP: {4} (ID={5}), Xuat: {6}, Nhap: {7}, IsHuy: {8}",
                                g.ID, g.IDChiTietKho, g.TenKhoHang, g.IDKho, g.MaSanPham, g.IDSanPham, g.SoLuongXuat, g.SoLuongNhap, g.IsHuy));
                        }
                    }

                    Console.WriteLine("\n=========================================================");
                    Console.WriteLine("2. ALL EXPORT SLIPS (KHO_PhieuXuat) FOR ZOCAO (ID=5) AT KHO NGÃ 5 (ID=1) IN JUNE 2026");
                    Console.WriteLine("=========================================================");

                    var list_px_zocao = conn.Query(@"
                        SELECT px.ID as IDPhieu, px.SoChungTu, px.NgayXuat, px.IDKho, k.TenKhoHang,
                               ct.ID as IDChiTiet, ct.IDSanPham, sp.MaSanPham, sp.TenSanPham, ct.SoLuong as SoLuongPhieu,
                               px.TrangThai, px.IsDeleted, px.GhiChu
                        FROM KHO_PhieuXuat px
                        INNER JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
                        LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                        LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
                        WHERE px.IDKho = 1 AND ct.IDSanPham = 5
                          AND px.NgayXuat >= '2026-06-01' AND px.NgayXuat <= '2026-06-30 23:59:59'
                        ORDER BY px.NgayXuat, px.SoChungTu
                    ").ToList();

                    Console.WriteLine(string.Format("Total PhieuXuat for zocao at Kho Ngã 5 in June 2026: {0}", list_px_zocao.Count));
                    foreach (var p in list_px_zocao)
                    {
                        // Search in KHO_GiaoDichKho
                        var gds = conn.Query(@"
                            SELECT gd.*, k.TenKhoHang, sp.MaSanPham
                            FROM KHO_GiaoDichKho gd
                            LEFT JOIN DM_KhoHang k ON gd.IDKho = k.ID
                            LEFT JOIN DM_SanPham sp ON gd.IDSanPham = sp.ID
                            WHERE gd.SoChungTu = @SoChungTu OR gd.IDChiTietKho = @IDChiTiet
                        ", new { SoChungTu = (string)p.SoChungTu, IDChiTiet = (int)p.IDChiTiet }).ToList();

                        Console.WriteLine(string.Format("\nPX: {0} | Date: {1:dd/MM/yyyy} | QtyInPhieu: {2} | Status: {3} | Deleted: {4}",
                            p.SoChungTu, Convert.ToDateTime(p.NgayXuat), p.SoLuongPhieu, p.TrangThai, p.IsDeleted));
                        
                        if (gds.Count == 0)
                        {
                            Console.WriteLine("   -> CRITICAL: NO ENTRY IN KHO_GiaoDichKho AT ALL!");
                        }
                        else
                        {
                            foreach (var g in gds)
                            {
                                Console.WriteLine(string.Format("   -> GD ID: {0} | IDChiTietKho: {1} | KhoGD: {2} (ID={3}) | SP: {4} (ID={5}) | XuatGD: {6} | IsHuy: {7}",
                                    g.ID, g.IDChiTietKho, g.TenKhoHang, g.IDKho, g.MaSanPham, g.IDSanPham, g.SoLuongXuat, g.IsHuy));
                            }
                        }
                    }

                    Console.WriteLine("\n=========================================================");
                    Console.WriteLine("3. ALL KHO_GIAODICHKHO FOR ZOCAO (ID=5) AT KHO NGÃ 5 (ID=1) IN JUNE 2026");
                    Console.WriteLine("=========================================================");

                    var all_gd_zocao_june = conn.Query(@"
                        SELECT gd.*, sp.MaSanPham
                        FROM KHO_GiaoDichKho gd
                        LEFT JOIN DM_SanPham sp ON gd.IDSanPham = sp.ID
                        WHERE gd.IDKho = 1 AND gd.IDSanPham = 5
                          AND gd.NgayChungTu >= '2026-06-01' AND gd.NgayChungTu <= '2026-06-30 23:59:59'
                        ORDER BY gd.NgayChungTu, gd.ID
                    ").ToList();

                    Console.WriteLine(string.Format("Total KHO_GiaoDichKho entries (Kho Ngã 5, zocao, June 2026): {0}", all_gd_zocao_june.Count));
                    foreach (var g in all_gd_zocao_june)
                    {
                        Console.WriteLine(string.Format("GD ID: {0} | Date: {1:dd/MM/yyyy} | SoChungTu: {2} | Nhap: {3} | Xuat: {4} | IsHuy: {5} | IDChiTietKho: {6} | DienGiai: {7}",
                            g.ID, Convert.ToDateTime(g.NgayChungTu), g.SoChungTu, g.SoLuongNhap, g.SoLuongXuat, g.IsHuy, g.IDChiTietKho, g.DienGiai));
                    }

                    Console.WriteLine("\n=========================================================");
                    Console.WriteLine("4. ALL PHIEU XUAT KHO IN ENTIRE DATABASE FOR KHO NGÃ 5 (ID=1) IN JUNE 2026");
                    Console.WriteLine("=========================================================");

                    var all_px_june = conn.Query(@"
                        SELECT px.ID, px.SoChungTu, px.NgayXuat, px.IDKho, px.TrangThai, px.IsDeleted,
                               ct.ID as IDChiTiet, ct.IDSanPham, sp.MaSanPham, sp.TenSanPham, ct.SoLuong
                        FROM KHO_PhieuXuat px
                        INNER JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
                        LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                        WHERE px.IDKho = 1
                          AND px.NgayXuat >= '2026-06-01' AND px.NgayXuat <= '2026-06-30 23:59:59'
                        ORDER BY px.NgayXuat, px.SoChungTu
                    ").ToList();

                    Console.WriteLine(string.Format("Total PhieuXuat_ChiTiet records for Kho Ngã 5 in June: {0}", all_px_june.Count));
                    foreach (var item in all_px_june)
                    {
                        var inGD = conn.ExecuteScalar<int>(@"
                            SELECT COUNT(*) FROM KHO_GiaoDichKho
                            WHERE IDKho = 1 AND IDSanPham = @IDSP AND SoChungTu = @SoChungTu AND IsHuy = 0
                        ", new { IDSP = (int)item.IDSanPham, SoChungTu = (string)item.SoChungTu });

                        Console.WriteLine(string.Format("PX: {0} | Date: {1:dd/MM/yyyy} | SP: {2} ({3}) | Qty: {4} | Status: {5} | Deleted: {6} | CountInGD: {7}",
                            item.SoChungTu, Convert.ToDateTime(item.NgayXuat), item.MaSanPham, item.TenSanPham, item.SoLuong, item.TrangThai, item.IsDeleted, inGD));
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
