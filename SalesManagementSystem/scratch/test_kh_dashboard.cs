using System;
using System.Data;
using Dapper;
using SalesManagementSystem.Data;

namespace TestKhDashboard
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

                    DateTime tuNgay = new DateTime(2026, 8, 1);
                    DateTime denNgay = new DateTime(2026, 8, 31, 23, 59, 59);

                    Console.WriteLine("=================================================");
                    Console.WriteLine($"TEST DASHBOARD KH (TuNgay: {tuNgay:dd/MM/yyyy}, DenNgay: {denNgay:dd/MM/yyyy})");
                    Console.WriteLine("=================================================");

                    decimal noDauKyKH = conn.QueryFirstOrDefault<decimal>(@"
                        SELECT ISNULL((SELECT SUM(bh.TongCong) FROM BAN_ChungTuBanHang bh WHERE bh.IsDeleted = 0 AND bh.TrangThai = 2 AND bh.NgayChungTu < @TuNgay), 0)
                             - ISNULL((SELECT SUM(pt.SoTienThu) FROM KT_PhieuThu pt WHERE pt.TrangThai = 2 AND pt.NgayThu < @TuNgay), 0)
                    ", new { TuNgay = tuNgay });

                    decimal tongTienBanKH = conn.QueryFirstOrDefault<decimal>(@"
                        SELECT ISNULL(SUM(bh.TongCong), 0) FROM BAN_ChungTuBanHang bh WHERE bh.IsDeleted = 0 AND bh.TrangThai = 2 AND bh.NgayChungTu >= @TuNgay AND bh.NgayChungTu <= @DenNgay
                    ", new { TuNgay = tuNgay, DenNgay = denNgay });

                    decimal daThuKH = conn.QueryFirstOrDefault<decimal>(@"
                        SELECT ISNULL(SUM(pt.SoTienThu), 0) FROM KT_PhieuThu pt WHERE pt.TrangThai = 2 AND pt.NgayThu >= @TuNgay AND pt.NgayThu <= @DenNgay
                    ", new { TuNgay = tuNgay, DenNgay = denNgay });

                    decimal congNoKhachHang = noDauKyKH + tongTienBanKH - daThuKH;

                    Console.WriteLine($"1. Nợ đầu kỳ KH (trước 01/08/2026) : {noDauKyKH:N0}₫");
                    Console.WriteLine($"2. Bán trong kỳ (01/08 - 31/08)    : {tongTienBanKH:N0}₫");
                    Console.WriteLine($"3. Đã thu trong kỳ (01/08 - 31/08)  : {daThuKH:N0}₫");
                    Console.WriteLine($"4. Nợ cuối kỳ KH                    : {congNoKhachHang:N0}₫");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX: " + ex);
            }
        }
    }
}
