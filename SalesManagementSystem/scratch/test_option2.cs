using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace TestOption2
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
                    Console.WriteLine("THÁNG 7 (01/07/2026 -> 31/07/2026)");
                    Console.WriteLine("=================================================");
                    TestRange(conn, new DateTime(2026, 7, 1), new DateTime(2026, 7, 31));

                    Console.WriteLine("\n=================================================");
                    Console.WriteLine("THÁNG 8 (01/08/2026 -> 31/08/2026)");
                    Console.WriteLine("=================================================");
                    TestRange(conn, new DateTime(2026, 8, 1), new DateTime(2026, 8, 31));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX: " + ex);
            }
        }

        static void TestRange(IDbConnection conn, DateTime tuNgay, DateTime denNgay)
        {
            // Nợ đầu kỳ (trước tuNgay)
            decimal tongMuaDauKy = conn.QueryFirstOrDefault<decimal>(
                "SELECT ISNULL(SUM(pn.TongCong), 0) FROM KHO_PhieuNhap pn WHERE pn.IsDeleted = 0 AND pn.TrangThai = 2 AND pn.NgayNhap < @TuNgay",
                new { TuNgay = tuNgay }
            );
            decimal tongTraDauKy = conn.QueryFirstOrDefault<decimal>(
                "SELECT ISNULL(SUM(pc.SoTienChi), 0) FROM KT_PhieuChi pc WHERE pc.IsDeleted = 0 AND pc.TrangThai = 2 AND pc.NgayChi < @TuNgay",
                new { TuNgay = tuNgay }
            );
            decimal noDauKy = tongMuaDauKy - tongTraDauKy;

            // Mua trong kỳ
            decimal muaTrongKy = conn.QueryFirstOrDefault<decimal>(
                "SELECT ISNULL(SUM(pn.TongCong), 0) FROM KHO_PhieuNhap pn WHERE pn.IsDeleted = 0 AND pn.TrangThai = 2 AND pn.NgayNhap >= @TuNgay AND pn.NgayNhap <= @DenNgay",
                new { TuNgay = tuNgay, DenNgay = denNgay }
            );

            // Tra trong kỳ
            decimal traTrongKy = conn.QueryFirstOrDefault<decimal>(
                "SELECT ISNULL(SUM(pc.SoTienChi), 0) FROM KT_PhieuChi pc WHERE pc.IsDeleted = 0 AND pc.TrangThai = 2 AND pc.NgayChi >= @TuNgay AND pc.NgayChi <= @DenNgay",
                new { TuNgay = tuNgay, DenNgay = denNgay }
            );

            // Dư cuối kỳ
            decimal noCuoiKy = noDauKy + muaTrongKy - traTrongKy;

            Console.WriteLine($"Nợ đầu kỳ (trước {tuNgay:dd/MM/yyyy})  : {noDauKy:N0}");
            Console.WriteLine($"Mua trong kỳ ({tuNgay:dd/MM} - {denNgay:dd/MM})  : {muaTrongKy:N0}");
            Console.WriteLine($"Đã trả trong kỳ ({tuNgay:dd/MM} - {denNgay:dd/MM}): {traTrongKy:N0}");
            Console.WriteLine($"Nợ cuối kỳ ({denNgay:dd/MM/yyyy})         : {noCuoiKy:N0}");
        }
    }
}
