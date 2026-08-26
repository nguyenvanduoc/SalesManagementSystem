using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace CheckPhieuChi
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

                    Console.WriteLine("KT_PhieuChi break down by IDNhaCungCap:");
                    var pcList = conn.Query<dynamic>(@"
                        SELECT 
                            pc.ID, pc.SoPhieuChi, pc.NgayChi, pc.SoTienChi, pc.IDNhaCungCap, pc.IDPhieuNhap, pc.IDKhoanMucChi,
                            km.TenKhoanMuc
                        FROM KT_PhieuChi pc
                        LEFT JOIN DM_KhoanMucChi km ON pc.IDKhoanMucChi = km.ID
                        WHERE pc.IsDeleted = 0 AND pc.TrangThai = 2
                    ").ToList();

                    int totalPc = pcList.Count;
                    int withNcc = pcList.Count(x => x.IDNhaCungCap != null);
                    int withPn  = pcList.Count(x => x.IDPhieuNhap != null);

                    Console.WriteLine($"Total active KT_PhieuChi (TrangThai=2): {totalPc}");
                    Console.WriteLine($" - With IDNhaCungCap: {withNcc}");
                    Console.WriteLine($" - With IDPhieuNhap : {withPn}");

                    // Check payments associated with purchase invoices via KT_PhieuChiChiTiet or IDPhieuNhap/IDNhaCungCap
                    var nccPayments = conn.Query<dynamic>(@"
                        SELECT 
                            pc.ID, pc.NgayChi, pc.SoTienChi, pc.IDNhaCungCap, pc.IDPhieuNhap
                        FROM KT_PhieuChi pc
                        WHERE pc.IsDeleted = 0 AND pc.TrangThai = 2
                          AND (pc.IDNhaCungCap IS NOT NULL OR pc.IDPhieuNhap IS NOT NULL 
                               OR EXISTS (SELECT 1 FROM KT_PhieuChiChiTiet ct WHERE ct.IDPhieuChi = pc.ID))
                    ").ToList();

                    Console.WriteLine($"Total KT_PhieuChi for NCC/PhieuNhap: {nccPayments.Count}");

                    DateTime tuNgayAug = new DateTime(2026, 8, 1);
                    DateTime denNgayAug = new DateTime(2026, 8, 31);

                    decimal muaAug = conn.QueryFirstOrDefault<decimal>("SELECT ISNULL(SUM(TongCong),0) FROM KHO_PhieuNhap WHERE IsDeleted=0 AND NgayNhap >= @TuNgay AND NgayNhap <= @DenNgay", new { TuNgay = tuNgayAug, DenNgay = denNgayAug });
                    Console.WriteLine($"\nKHO_PhieuNhap (August): {muaAug:N0}");

                    // Test payment allocation logic used in sp_CongNo_PhaseTra_NCC_GetList
                    var paidAug_PerInvoice = conn.QueryFirstOrDefault<decimal>(@"
                        SELECT ISNULL(SUM(pd.DaThanhToan), 0)
                        FROM KHO_PhieuNhap pn
                        INNER JOIN (
                            SELECT 
                                pn.ID,
                                ISNULL(
                                    (SELECT SUM(ct.SoTienPhanBo)
                                     FROM KT_PhieuChiChiTiet ct
                                     INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                                     WHERE ct.IDPhieuNhap = pn.ID 
                                       AND ct.LoaiChi = 1
                                       AND pc.TrangThai = 2
                                       AND pc.IsDeleted = 0),
                                    0
                                ) + ISNULL(
                                    (SELECT SUM(pc2.SoTienChi)
                                     FROM KT_PhieuChi pc2
                                     WHERE pc2.IDPhieuNhap = pn.ID
                                       AND pc2.TrangThai = 2
                                       AND pc2.IsDeleted = 0
                                       AND NOT EXISTS (SELECT 1 FROM KT_PhieuChiChiTiet ct WHERE ct.IDPhieuChi = pc2.ID)
                                    ),
                                    0
                                ) AS DaThanhToan
                            FROM KHO_PhieuNhap pn
                            WHERE pn.IsDeleted = 0
                        ) pd ON pn.ID = pd.ID
                        WHERE pn.IsDeleted = 0
                          AND pn.NgayNhap >= @TuNgay AND pn.NgayNhap <= @DenNgay
                    ", new { TuNgay = tuNgayAug, DenNgay = denNgayAug });

                    Console.WriteLine($"Paid for August Invoices (Per-invoice payment): {paidAug_PerInvoice:N0}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX: " + ex);
            }
        }
    }
}
