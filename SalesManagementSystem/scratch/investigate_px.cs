using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace InvestigatePX
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
                    Console.WriteLine("=== 1. WAREHOUSES LIKE 'Ngã 5' OR 'Nga 5' ===");
                    var khos = conn.Query(@"SELECT ID, MaKhoHang, TenKhoHang FROM DM_KhoHang WHERE TenKhoHang LIKE N'%Ngã 5%' OR TenKhoHang LIKE N'%Nga 5%' OR MaKhoHang LIKE N'%Nga%'").ToList();
                    foreach (var k in khos)
                    {
                        Console.WriteLine(string.Format("ID: {0} | Ma: {1} | Ten: {2}", k.ID, k.MaKhoHang, k.TenKhoHang));
                    }

                    Console.WriteLine("\n=== 2. PRODUCTS LIKE 'zocao' OR 'zolun' OR '123' ===");
                    var sps = conn.Query(@"SELECT ID, MaSanPham, TenSanPham FROM DM_SanPham WHERE MaSanPham LIKE '%zocao%' OR TenSanPham LIKE '%zocao%' OR MaSanPham LIKE '%zolun%' OR TenSanPham LIKE '%zolun%'").ToList();
                    foreach (var s in sps)
                    {
                        Console.WriteLine(string.Format("ID: {0} | Ma: {1} | Ten: {2}", s.ID, s.MaSanPham, s.TenSanPham));
                    }

                    Console.WriteLine("\n=== 3. PHIẾU XUẤT PX00033 DETAILS ===");
                    var px33 = conn.QueryFirstOrDefault(@"SELECT ID, SoChungTu, NgayXuat, IDKho, TrangThai, IsDeleted, GhiChu FROM KHO_PhieuXuat WHERE SoChungTu = 'PX00033' OR SoChungTu LIKE '%PX00033%'");
                    if (px33 != null)
                    {
                        Console.WriteLine(string.Format("PX00033 Header -> ID: {0} | SoChungTu: {1} | NgayXuat: {2} | IDKho: {3} | TrangThai: {4} | IsDeleted: {5} | GhiChu: {6}", 
                            px33.ID, px33.SoChungTu, px33.NgayXuat, px33.IDKho, px33.TrangThai, px33.IsDeleted, px33.GhiChu));

                        var px33_details = conn.Query(@"SELECT ct.*, sp.MaSanPham, sp.TenSanPham FROM KHO_PhieuXuat_ChiTiet ct LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID WHERE ct.IDPhieuXuat = @ID", new { ID = (int)px33.ID }).ToList();
                        Console.WriteLine("PX00033 Details in KHO_PhieuXuat_ChiTiet:");
                        foreach (var d in px33_details)
                        {
                            Console.WriteLine(string.Format("  Detail ID: {0} | IDSanPham: {1} | MaSP: {2} | TenSP: {3} | SoLuong: {4}", d.ID, d.IDSanPham, d.MaSanPham, d.TenSanPham, d.SoLuong));
                        }

                        var px33_gd = conn.Query(@"SELECT gd.*, sp.MaSanPham, sp.TenSanPham, k.TenKhoHang FROM KHO_GiaoDichKho gd LEFT JOIN DM_SanPham sp ON gd.IDSanPham = sp.ID LEFT JOIN DM_KhoHang k ON gd.IDKho = k.ID WHERE gd.SoChungTu = 'PX00033' OR gd.SoChungTu LIKE '%PX00033%'").ToList();
                        Console.WriteLine("PX00033 Entries in KHO_GiaoDichKho:");
                        foreach (var g in px33_gd)
                        {
                            Console.WriteLine(string.Format("  GD ID: {0} | IDKho: {1} ({2}) | IDSanPham: {3} ({4}) | Xuat: {5} | Nhap: {6} | IsHuy: {7} | Ngay: {8}",
                                g.ID, g.IDKho, g.TenKhoHang, g.IDSanPham, g.MaSanPham, g.SoLuongXuat, g.SoLuongNhap, g.IsHuy, g.NgayChungTu));
                        }
                    }
                    else
                    {
                        Console.WriteLine("PX00033 not found in KHO_PhieuXuat!");
                    }

                    int nga5_id = 1; // ID 1 = Kho thành phẩm Ngã 5
                    int zocao_id = 5; // ID 5 = zocao

                    Console.WriteLine("\n=== 4. ALL JUNE 2026 EXPORT SLIPS FOR KHO NGÃ 5 (ID=1) ===");
                    var june_px = conn.Query(@"
                        SELECT px.ID, px.SoChungTu, px.NgayXuat, px.IDKho, px.TrangThai, px.IsDeleted,
                               ct.IDSanPham, sp.MaSanPham, sp.TenSanPham, ct.SoLuong
                        FROM KHO_PhieuXuat px
                        INNER JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
                        LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                        LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
                        WHERE px.IDKho = @IDKho
                          AND px.NgayXuat >= '2026-06-01' AND px.NgayXuat <= '2026-06-30 23:59:59'
                        ORDER BY px.NgayXuat, px.SoChungTu
                    ", new { IDKho = nga5_id }).ToList();

                    Console.WriteLine(string.Format("Total June export details count for Kho Ngã 5: {0}", june_px.Count));
                    foreach (var item in june_px)
                    {
                        Console.WriteLine(string.Format("PX: {0} | Date: {1:dd/MM/yyyy} | SP: {2} ({3}) | Qty: {4} | Status: {5} | Deleted: {6}",
                            item.SoChungTu, Convert.ToDateTime(item.NgayXuat), item.MaSanPham, item.TenSanPham, item.SoLuong, item.TrangThai, item.IsDeleted));
                    }

                    Console.WriteLine("\n=== 5. ALL KHO_GIAODICHKHO IN JUNE 2026 FOR KHO NGÃ 5 (ID=1) & ZOCAO (ID=5) ===");
                    var gd_zocao = conn.Query(@"
                        SELECT gd.* 
                        FROM KHO_GiaoDichKho gd
                        WHERE gd.IDKho = @IDKho AND gd.IDSanPham = @IDSanPham
                          AND gd.NgayChungTu >= '2026-06-01' AND gd.NgayChungTu <= '2026-06-30 23:59:59'
                        ORDER BY gd.NgayChungTu, gd.ID
                    ", new { IDKho = nga5_id, IDSanPham = zocao_id }).ToList();

                    Console.WriteLine(string.Format("Total KHO_GiaoDichKho entries for zocao in June: {0}", gd_zocao.Count));
                    foreach (var g in gd_zocao)
                    {
                        Console.WriteLine(string.Format("  GD ID: {0} | SoChungTu: {1} | Date: {2:dd/MM/yyyy} | Nhap: {3} | Xuat: {4} | IsHuy: {5}",
                            g.ID, g.SoChungTu, Convert.ToDateTime(g.NgayChungTu), g.SoLuongNhap, g.SoLuongXuat, g.IsHuy));
                    }

                    Console.WriteLine("\n=== 6. ALL EXPORT SLIPS CONTAINING ZOCAO IN JUNE FOR KHO NGÃ 5 ===");
                    var px_zocao = conn.Query(@"
                        SELECT px.ID, px.SoChungTu, px.NgayXuat, px.TrangThai, px.IsDeleted, ct.SoLuong, ct.ID as IDChiTiet, px.IDKho
                        FROM KHO_PhieuXuat px
                        INNER JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
                        WHERE px.IDKho = @IDKho AND ct.IDSanPham = @IDSanPham
                          AND px.NgayXuat >= '2026-06-01' AND px.NgayXuat <= '2026-06-30 23:59:59'
                        ORDER BY px.NgayXuat, px.SoChungTu
                    ", new { IDKho = nga5_id, IDSanPham = zocao_id }).ToList();

                    Console.WriteLine(string.Format("PhieuXuat ChiTiet count for zocao in June (Kho Ngã 5): {0}", px_zocao.Count));
                    foreach (var p in px_zocao)
                    {
                        var hasGD = gd_zocao.Any(g => g.SoChungTu == p.SoChungTu && g.IsHuy == false);
                        Console.WriteLine(string.Format("PX: {0} | Date: {1:dd/MM/yyyy} | Qty: {2} | Status: {3} | IsDeleted: {4} | In GiaoDichKho: {5}",
                            p.SoChungTu, Convert.ToDateTime(p.NgayXuat), p.SoLuong, p.TrangThai, p.IsDeleted, hasGD));
                    }

                    Console.WriteLine("\n=== 7. CHECK ALL PHIEU XUAT KHO FOR ZOCAO ACROSS ALL WAREHOUSES IN JUNE ===");
                    var all_px_zocao_june = conn.Query(@"
                        SELECT px.ID, px.SoChungTu, px.NgayXuat, px.IDKho, k.TenKhoHang, ct.IDSanPham, sp.MaSanPham, ct.SoLuong, px.TrangThai, px.IsDeleted
                        FROM KHO_PhieuXuat px
                        INNER JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
                        LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                        LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
                        WHERE sp.MaSanPham = 'zocao'
                          AND px.NgayXuat >= '2026-06-01' AND px.NgayXuat <= '2026-06-30 23:59:59'
                        ORDER BY px.NgayXuat, px.SoChungTu
                    ").ToList();

                    Console.WriteLine(string.Format("Total PhieuXuat for zocao across ALL warehouses in June: {0}", all_px_zocao_june.Count));
                    foreach (var p in all_px_zocao_june)
                    {
                        Console.WriteLine(string.Format("  PX: {0} | Date: {1:dd/MM/yyyy} | Kho: {2} (ID: {3}) | Qty: {4} | Status: {5} | Deleted: {6}",
                            p.SoChungTu, Convert.ToDateTime(p.NgayXuat), p.TenKhoHang, p.IDKho, p.SoLuong, p.TrangThai, p.IsDeleted));
                    }

                    Console.WriteLine("\n=== 8. CHECK DISCREPANCIES BETWEEN KHO_PhieuXuat_ChiTiet AND KHO_GiaoDichKho FOR ALL JUNE EXPORTS ===");
                    var disc = conn.Query(@"
                        SELECT px.SoChungTu, px.NgayXuat, px.IDKho, k.TenKhoHang, ct.IDSanPham, sp.MaSanPham, ct.SoLuong as QtyPhieuXuat,
                               gd.ID as IDGD, gd.SoLuongXuat as QtyGD, gd.IsHuy, gd.IDKho as GDKho, px.TrangThai, px.IsDeleted
                        FROM KHO_PhieuXuat px
                        INNER JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
                        LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                        LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
                        LEFT JOIN KHO_GiaoDichKho gd ON (gd.IDChiTietKho = ct.ID OR (gd.SoChungTu = px.SoChungTu AND gd.IDSanPham = ct.IDSanPham))
                        WHERE px.NgayXuat >= '2026-06-01' AND px.NgayXuat <= '2026-06-30 23:59:59'
                          AND px.IsDeleted = 0
                        ORDER BY px.NgayXuat, px.SoChungTu
                    ").ToList();

                    Console.WriteLine(string.Format("Total Export details in June: {0}", disc.Count));
                    foreach (var d in disc)
                    {
                        if (d.IDGD == null || d.IsHuy == true || d.QtyPhieuXuat != d.QtyGD || d.IDKho != d.GDKho)
                        {
                            Console.WriteLine(string.Format("MISMATCH! PX: {0} | Date: {1:dd/MM/yyyy} | KhoPX: {2} | SP: {3} | QtyPX: {4} | IDGD: {5} | QtyGD: {6} | IsHuy: {7} | GDKho: {8}",
                                d.SoChungTu, Convert.ToDateTime(d.NgayXuat), d.TenKhoHang, d.MaSanPham, d.QtyPhieuXuat, d.IDGD, d.QtyGD, d.IsHuy, d.GDKho));
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
