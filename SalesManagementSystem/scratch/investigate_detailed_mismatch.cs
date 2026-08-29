using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace InvestigateDetailedMismatch
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
                    Console.WriteLine("1. CHECK ZOLUN (ID=4) IN KHO_GIAODICHKHO VS KHO_PHIEUXUAT_CHITIET (KHO NGÃ 5, JUNE 2026)");
                    Console.WriteLine("=========================================================");

                    // Get all PhieuXuat_ChiTiet for zolun (ID=4) at Kho Ngã 5 (ID=1) in June 2026
                    var zolun_px = conn.Query(@"
                        SELECT px.ID as IDPhieuXuat, px.SoChungTu, px.NgayXuat, px.IDKho, ct.ID as IDChiTiet, ct.IDSanPham, ct.SoLuong, px.TrangThai, px.IsDeleted
                        FROM KHO_PhieuXuat px
                        INNER JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
                        WHERE px.IDKho = 1 AND ct.IDSanPham = 4
                          AND px.NgayXuat >= '2026-06-01' AND px.NgayXuat <= '2026-06-30 23:59:59'
                        ORDER BY px.NgayXuat, px.SoChungTu
                    ").ToList();

                    Console.WriteLine(string.Format("Total PhieuXuat_ChiTiet for ZOLUN in June (Kho Ngã 5): {0}", zolun_px.Count));
                    foreach (var p in zolun_px)
                    {
                        var gd = conn.Query(@"
                            SELECT gd.* 
                            FROM KHO_GiaoDichKho gd
                            WHERE (gd.IDChiTietKho = @IDChiTiet OR (gd.SoChungTu = @SoChungTu AND gd.IDSanPham = 4))
                        ", new { IDChiTiet = (int)p.IDChiTiet, SoChungTu = (string)p.SoChungTu }).ToList();

                        Console.WriteLine(string.Format("\nPX: {0} | Date: {1:dd/MM/yyyy} | QtyPX: {2} | Status: {3} | Deleted: {4}",
                            p.SoChungTu, Convert.ToDateTime(p.NgayXuat), p.SoLuong, p.TrangThai, p.IsDeleted));

                        if (gd.Count == 0)
                        {
                            Console.WriteLine("   -> MISSING IN KHO_GiaoDichKho!");
                        }
                        else
                        {
                            foreach (var g in gd)
                            {
                                Console.WriteLine(string.Format("   -> GD ID: {0} | SoChungTu: {1} | KhoGD: {2} | SPGD: {3} | XuatGD: {4} | IsHuy: {5}",
                                    g.ID, g.SoChungTu, g.IDKho, g.IDSanPham, g.SoLuongXuat, g.IsHuy));
                            }
                        }
                    }

                    Console.WriteLine("\n=========================================================");
                    Console.WriteLine("2. CHECK ZOCAO (ID=5) IN KHO_GIAODICHKHO VS KHO_PHIEUXUAT_CHITIET (KHO NGÃ 5, JUNE 2026)");
                    Console.WriteLine("=========================================================");

                    var zocao_px = conn.Query(@"
                        SELECT px.ID as IDPhieuXuat, px.SoChungTu, px.NgayXuat, px.IDKho, ct.ID as IDChiTiet, ct.IDSanPham, ct.SoLuong, px.TrangThai, px.IsDeleted
                        FROM KHO_PhieuXuat px
                        INNER JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
                        WHERE px.IDKho = 1 AND ct.IDSanPham = 5
                          AND px.NgayXuat >= '2026-06-01' AND px.NgayXuat <= '2026-06-30 23:59:59'
                        ORDER BY px.NgayXuat, px.SoChungTu
                    ").ToList();

                    Console.WriteLine(string.Format("Total PhieuXuat_ChiTiet for ZOCAO in June (Kho Ngã 5): {0}", zocao_px.Count));
                    foreach (var p in zocao_px)
                    {
                        var gd = conn.Query(@"
                            SELECT gd.* 
                            FROM KHO_GiaoDichKho gd
                            WHERE (gd.IDChiTietKho = @IDChiTiet OR (gd.SoChungTu = @SoChungTu AND gd.IDSanPham = 5))
                        ", new { IDChiTiet = (int)p.IDChiTiet, SoChungTu = (string)p.SoChungTu }).ToList();

                        Console.WriteLine(string.Format("\nPX: {0} | Date: {1:dd/MM/yyyy} | QtyPX: {2} | Status: {3} | Deleted: {4}",
                            p.SoChungTu, Convert.ToDateTime(p.NgayXuat), p.SoLuong, p.TrangThai, p.IsDeleted));

                        if (gd.Count == 0)
                        {
                            Console.WriteLine("   -> MISSING IN KHO_GiaoDichKho!");
                        }
                        else
                        {
                            foreach (var g in gd)
                            {
                                Console.WriteLine(string.Format("   -> GD ID: {0} | SoChungTu: {1} | KhoGD: {2} | SPGD: {3} | XuatGD: {4} | IsHuy: {5}",
                                    g.ID, g.SoChungTu, g.IDKho, g.IDSanPham, g.SoLuongXuat, g.IsHuy));
                            }
                        }
                    }

                    Console.WriteLine("\n=========================================================");
                    Console.WriteLine("3. CHECK PX00033 IN KHO_GIAODICHKHO (ALL RECORDS)");
                    Console.WriteLine("=========================================================");
                    var px33_all_gd = conn.Query("SELECT * FROM KHO_GiaoDichKho WHERE SoChungTu = 'PX00033'").ToList();
                    Console.WriteLine(string.Format("PX00033 GD count: {0}", px33_all_gd.Count));
                    foreach (var g in px33_all_gd)
                    {
                        Console.WriteLine(string.Format("GD ID: {0} | IDKho: {1} | IDSanPham: {2} | Xuat: {3} | IsHuy: {4} | IDChiTietKho: {5}",
                            g.ID, g.IDKho, g.IDSanPham, g.SoLuongXuat, g.IsHuy, g.IDChiTietKho));
                    }

                    Console.WriteLine("\n=========================================================");
                    Console.WriteLine("4. CHECK ALL EXPORT SLIPS IN JUNE 2026 WITH MISSING/CANCELLED GIAODICHKHO ENTRIES");
                    Console.WriteLine("=========================================================");
                    var all_px_june_missing = conn.Query(@"
                        SELECT px.SoChungTu, px.NgayXuat, px.IDKho, k.TenKhoHang, ct.ID as IDChiTiet, ct.IDSanPham, sp.MaSanPham, sp.TenSanPham, ct.SoLuong
                        FROM KHO_PhieuXuat px
                        INNER JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
                        LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                        LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
                        WHERE px.NgayXuat >= '2026-06-01' AND px.NgayXuat <= '2026-06-30 23:59:59'
                          AND px.IsDeleted = 0
                          AND NOT EXISTS (
                              SELECT 1 FROM KHO_GiaoDichKho gd
                              WHERE gd.IDChiTietKho = ct.ID AND gd.IsHuy = 0
                          )
                        ORDER BY px.IDKho, px.NgayXuat, px.SoChungTu
                    ").ToList();

                    Console.WriteLine(string.Format("Total PhieuXuat_ChiTiet in June missing active KHO_GiaoDichKho: {0}", all_px_june_missing.Count));
                    foreach (var m in all_px_june_missing)
                    {
                        Console.WriteLine(string.Format("  - PX: {0} | Date: {1:dd/MM/yyyy} | Kho: {2} (ID={3}) | SP: {4} ({5}) | Qty: {6} | IDChiTiet: {7}",
                            m.SoChungTu, Convert.ToDateTime(m.NgayXuat), m.TenKhoHang, m.IDKho, m.MaSanPham, m.TenSanPham, m.SoLuong, m.IDChiTiet));
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
