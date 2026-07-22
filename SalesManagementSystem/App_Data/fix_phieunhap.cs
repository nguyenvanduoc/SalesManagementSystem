using System;
using System.IO;
using SalesManagementSystem.Data;
using Dapper;
using System.Linq;

namespace FixDB
{
    class Program
    {
        static void Main()
        {
            var db = new DbConnectionFactory();
            using (var conn = db.CreateConnection())
            {
                // Find all PhieuNhap that have allocations from CANCELED PhieuChi
                var query = @"
                    SELECT ct.IDPhieuNhap, ct.SoTienPhanBo 
                    FROM KT_PhieuChiChiTiet ct
                    JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    WHERE pc.TrangThai = 3 AND ct.LoaiChi = 1 AND ct.IDPhieuNhap IS NOT NULL
                ";
                var allocations = conn.Query(query).ToList();
                foreach (var alloc in allocations)
                {
                    int idPhieuNhap = alloc.IDPhieuNhap;
                    decimal soTienPhanBo = alloc.SoTienPhanBo;

                    conn.Execute(@"
                        UPDATE KHO_PhieuNhap
                        SET DaThanhToan = ISNULL(DaThanhToan, 0) - @SoTienPhanBo,
                            ConLai = ISNULL(TongCong, 0) - (ISNULL(DaThanhToan, 0) - @SoTienPhanBo),
                            TrangThaiThanhToan = CASE 
                                WHEN ISNULL(TongCong, 0) - (ISNULL(DaThanhToan, 0) - @SoTienPhanBo) <= 0 THEN 2 
                                WHEN ISNULL(DaThanhToan, 0) - @SoTienPhanBo <= 0 THEN 0 
                                ELSE 1 
                            END
                        WHERE ID = @IDPhieuNhap
                    ", new { SoTienPhanBo = soTienPhanBo, IDPhieuNhap = idPhieuNhap });
                }
                Console.WriteLine("Fixed " + allocations.Count + " allocations.");
            }
        }
    }
}
