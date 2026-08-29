using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace TestSyncLogic
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
                    using (var tr = conn.BeginTransaction())
                    {
                        Console.WriteLine("=========================================================");
                        Console.WriteLine("DRY-RUN SYNC TEST IN TRANSACTION (WILL ROLLBACK)");
                        Console.WriteLine("=========================================================");

                        // 1. Delete orphaned or mismatched KHO_GiaoDichKho entries for LoaiChungTu = 2 (PhieuXuat)
                        // where PhieuXuat is TrangThai = 2 and IsDeleted = 0
                        int deleted = conn.Execute(@"
                            DELETE FROM KHO_GiaoDichKho
                            WHERE LoaiChungTu = 2
                              AND SoChungTu IN (SELECT SoChungTu FROM KHO_PhieuXuat WHERE TrangThai = 2 AND IsDeleted = 0)
                        ", transaction: tr);

                        Console.WriteLine(string.Format("Deleted old KHO_GiaoDichKho records for active PhieuXuat: {0}", deleted));

                        // 2. Re-insert all KHO_GiaoDichKho records from KHO_PhieuXuat_ChiTiet
                        int inserted = conn.Execute(@"
                            INSERT INTO KHO_GiaoDichKho (
                                NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, 
                                SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao, IsHuy
                            )
                            SELECT 
                                p.NgayXuat, 
                                p.SoChungTu, 
                                2, -- 2 = Phiếu xuất kho
                                ct.ID, 
                                p.IDKho, 
                                ct.IDSanPham, 
                                0, 
                                ct.SoLuong, 
                                ct.DonGia, 
                                ct.ThanhTien, 
                                ISNULL(p.GhiChu, N'Xuất kho'), 
                                ISNULL(p.NgayTao, GETDATE()), 
                                ISNULL(p.NguoiGhi, ISNULL(p.NguoiTao, 1)),
                                0
                            FROM KHO_PhieuXuat_ChiTiet ct
                            INNER JOIN KHO_PhieuXuat p ON ct.IDPhieuXuat = p.ID
                            WHERE p.TrangThai = 2 AND p.IsDeleted = 0
                        ", transaction: tr);

                        Console.WriteLine(string.Format("Inserted clean KHO_GiaoDichKho records from PhieuXuat_ChiTiet: {0}", inserted));

                        // 3. Verify ZOLUN (ID=4) & PX00033 in Kho Ngã 5 (ID=1) after sync
                        var zolun_after = conn.Query(@"
                            SELECT gd.*, sp.MaSanPham
                            FROM KHO_GiaoDichKho gd
                            LEFT JOIN DM_SanPham sp ON gd.IDSanPham = sp.ID
                            WHERE gd.IDKho = 1 AND gd.IDSanPham = 4
                              AND gd.NgayChungTu >= '2026-06-01' AND gd.NgayChungTu <= '2026-06-30 23:59:59'
                            ORDER BY gd.NgayChungTu, gd.ID
                        ", transaction: tr).ToList();

                        Console.WriteLine(string.Format("\nAfter sync: Total KHO_GiaoDichKho entries for ZOLUN in Kho Ngã 5 (June): {0}", zolun_after.Count));
                        foreach (var g in zolun_after)
                        {
                            Console.WriteLine(string.Format("  - GD ID: {0} | Date: {1:dd/MM/yyyy} | SoChungTu: {2} | Xuat: {3}",
                                g.ID, Convert.ToDateTime(g.NgayChungTu), g.SoChungTu, g.SoLuongXuat));
                        }

                        // 4. Verify ZOCAO (ID=5) in Kho Ngã 5 (ID=1) after sync
                        var zocao_after = conn.Query(@"
                            SELECT gd.*, sp.MaSanPham
                            FROM KHO_GiaoDichKho gd
                            LEFT JOIN DM_SanPham sp ON gd.IDSanPham = sp.ID
                            WHERE gd.IDKho = 1 AND gd.IDSanPham = 5
                              AND gd.NgayChungTu >= '2026-06-01' AND gd.NgayChungTu <= '2026-06-30 23:59:59'
                            ORDER BY gd.NgayChungTu, gd.ID
                        ", transaction: tr).ToList();

                        Console.WriteLine(string.Format("\nAfter sync: Total KHO_GiaoDichKho entries for ZOCAO in Kho Ngã 5 (June): {0}", zocao_after.Count));
                        foreach (var g in zocao_after)
                        {
                            Console.WriteLine(string.Format("  - GD ID: {0} | Date: {1:dd/MM/yyyy} | SoChungTu: {2} | Xuat: {3}",
                                g.ID, Convert.ToDateTime(g.NgayChungTu), g.SoChungTu, g.SoLuongXuat));
                        }

                        tr.Rollback();
                        Console.WriteLine("\nTransaction ROLLED BACK safely. Database was NOT modified!");
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
