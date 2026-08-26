using System;
using System.Data;
using Dapper;
using SalesManagementSystem.Data;

namespace VerifyOpt2
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
                    DateTime denNgay = new DateTime(2026, 8, 31);

                    Console.WriteLine("=================================================");
                    Console.WriteLine($"FINAL DASHBOARD CARDS VERIFICATION ({tuNgay:dd/MM/yyyy} -> {denNgay:dd/MM/yyyy})");
                    Console.WriteLine("=================================================");

                    using (var multi = conn.QueryMultiple("sp_Dashboard_GetData", new {
                        TuNgay = tuNgay,
                        DenNgay = denNgay,
                        TuNgayKyTruoc = tuNgay.AddMonths(-1),
                        DenNgayKyTruoc = denNgay.AddMonths(-1)
                    }, commandType: CommandType.StoredProcedure))
                    {
                        var summary = multi.ReadFirstOrDefault();
                        if (summary != null)
                        {
                            Console.WriteLine("--- CARD 5: CÔNG NỢ KHÁCH HÀNG ---");
                            Console.WriteLine($"1. Nợ đầu kỳ KH    : {(decimal)(summary.NoDauKyKH ?? 0m):N0}₫");
                            Console.WriteLine($"2. Bán trong kỳ    : {(decimal)(summary.TongTienBanKH ?? 0m):N0}₫");
                            Console.WriteLine($"3. Đã thu          : {(decimal)(summary.DaThuKH ?? 0m):N0}₫");
                            Console.WriteLine($"4. Nợ cuối kỳ KH   : {(decimal)(summary.CongNoKhachHang ?? 0m):N0}₫");

                            Console.WriteLine("\n--- CARD 6: CÔNG NỢ NCC ---");
                            Console.WriteLine($"1. Nợ đầu kỳ NCC   : {(decimal)(summary.NoDauKyNCC ?? 0m):N0}₫");
                            Console.WriteLine($"2. Mua trong kỳ    : {(decimal)(summary.TongTienHangNCC ?? 0m):N0}₫");
                            Console.WriteLine($"3. Đã trả          : {(decimal)(summary.DaThanhToanNCC ?? 0m):N0}₫");
                            Console.WriteLine($"4. Nợ cuối kỳ NCC  : {(decimal)(summary.CongNoNhaCungCap ?? 0m):N0}₫");
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
