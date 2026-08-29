using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace CheckAllMissingGDKho
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
                    Console.WriteLine("1. CHECK ALL PHIEU XUAT (KHO_PhieuXuat) MISSING KHO_GIAODICHKHO");
                    Console.WriteLine("=========================================================");

                    var missing_px = conn.Query(@"
                        SELECT px.ID as IDPhieuXuat, px.SoChungTu, px.NgayXuat, px.IDKho, k.TenKhoHang,
                               ct.ID as IDChiTiet, ct.IDSanPham, sp.MaSanPham, sp.TenSanPham, ct.SoLuong, ct.DonGia, ct.ThanhTien,
                               px.TrangThai, px.IsDeleted, px.IDChungTuBanHang, px.GhiChu
                        FROM KHO_PhieuXuat px
                        INNER JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
                        LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                        LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
                        WHERE px.TrangThai = 2 AND px.IsDeleted = 0
                          AND NOT EXISTS (
                              SELECT 1 FROM KHO_GiaoDichKho gd
                              WHERE gd.LoaiChungTu = 2 
                                AND (gd.IDChiTietKho = ct.ID OR (gd.SoChungTu = px.SoChungTu AND gd.IDSanPham = ct.IDSanPham AND gd.IDKho = px.IDKho))
                                AND gd.IsHuy = 0
                          )
                        ORDER BY px.IDKho, px.NgayXuat, px.SoChungTu
                    ").ToList();

                    Console.WriteLine(string.Format("Total PhieuXuat_ChiTiet records (TrangThai=2, IsDeleted=0) MISSING in KHO_GiaoDichKho: {0}", missing_px.Count));
                    foreach (var m in missing_px)
                    {
                        Console.WriteLine(string.Format("  - PX: {0} | Date: {1:dd/MM/yyyy} | Kho: {2} (ID={3}) | SP: {4} ({5}) | Qty: {6} | IDChiTiet: {7} | IDChungTuBH: {8}",
                            m.SoChungTu, Convert.ToDateTime(m.NgayXuat), m.TenKhoHang, m.IDKho, m.MaSanPham, m.TenSanPham, m.SoLuong, m.IDChiTiet, m.IDChungTuBanHang));
                    }

                    Console.WriteLine("\n=========================================================");
                    Console.WriteLine("2. CHECK ALL PHIEU XUAT (KHO_PhieuXuat) WITH QUANTITY / SP MISMATCH IN KHO_GIAODICHKHO");
                    Console.WriteLine("=========================================================");

                    var mismatch_px = conn.Query(@"
                        SELECT px.SoChungTu, px.NgayXuat, px.IDKho, k.TenKhoHang,
                               ct.ID as IDChiTiet, ct.IDSanPham, sp.MaSanPham, sp.TenSanPham, ct.SoLuong as QtyPhieu,
                               gd.ID as IDGD, gd.IDKho as KhoGD, gd.IDSanPham as SPGD, gd.SoLuongXuat as QtyGD, gd.IsHuy
                        FROM KHO_PhieuXuat px
                        INNER JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
                        LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                        LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
                        INNER JOIN KHO_GiaoDichKho gd ON gd.IDChiTietKho = ct.ID
                        WHERE px.TrangThai = 2 AND px.IsDeleted = 0
                          AND (gd.SoLuongXuat <> ct.SoLuong OR gd.IDKho <> px.IDKho OR gd.IDSanPham <> ct.IDSanPham OR gd.IsHuy = 1)
                        ORDER BY px.IDKho, px.NgayXuat, px.SoChungTu
                    ").ToList();

                    Console.WriteLine(string.Format("Total PhieuXuat_ChiTiet records MISMATCHED with KHO_GiaoDichKho: {0}", mismatch_px.Count));
                    foreach (var m in mismatch_px)
                    {
                        Console.WriteLine(string.Format("  - PX: {0} | Date: {1:dd/MM/yyyy} | Kho: {2} (ID={3}) | SP: {4} | QtyPhieu: {5} | IDGD: {6} | KhoGD: {7} | SPGD: {8} | QtyGD: {9} | IsHuy: {10}",
                            m.SoChungTu, Convert.ToDateTime(m.NgayXuat), m.TenKhoHang, m.IDKho, m.MaSanPham, m.QtyPhieu, m.IDGD, m.KhoGD, m.SPGD, m.QtyGD, m.IsHuy));
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
