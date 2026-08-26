using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace TestKhCards
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

                    DateTime? tuNgay = new DateTime(2026, 8, 1);
                    DateTime? denNgay = new DateTime(2026, 8, 24);
                    int? idKhachHang = null;

                    Console.WriteLine("=================================================");
                    Console.WriteLine($"CÔNG NỢ KHÁCH HÀNG (TuNgay: {tuNgay:dd/MM/yyyy}, DenNgay: {denNgay:dd/MM/yyyy})");
                    Console.WriteLine("=================================================");

                    var list = conn.Query<dynamic>("sp_CongNoKhachHang_GetList", new {
                        TuNgay = tuNgay,
                        DenNgay = denNgay,
                        IDKhachHang = idKhachHang,
                        TrangThaiCongNo = (int?)null
                    }, commandType: CommandType.StoredProcedure).ToList();

                    decimal tongDauKy    = list.Sum(x => (decimal)(x.TonDauKy ?? 0m));
                    decimal tongDoanhThu = list.Sum(x => (decimal)(x.DoanhThu ?? 0m));
                    decimal tongDaThu    = list.Sum(x => (decimal)(x.DaThu ?? 0m));
                    decimal tongConThu   = list.Sum(x => (decimal)(x.ConPhaiThu ?? 0m));
                    decimal tongQuaHan   = list.Sum(x => (decimal)(x.TienQuaHan ?? 0m));

                    Console.WriteLine($"1. Nợ đầu kỳ                        : {tongDauKy:N0} VND");
                    Console.WriteLine($"2. Doanh thu phát sinh trong kỳ    : {tongDoanhThu:N0} VND");
                    Console.WriteLine($"3. Đã thu trong kỳ                   : {tongDaThu:N0} VND");
                    Console.WriteLine($"4. Còn phải thu (Nợ cuối kỳ)        : {tongConThu:N0} VND");
                    Console.WriteLine($"   Formula Check: {tongDauKy:N0} + {tongDoanhThu:N0} - {tongDaThu:N0} = {tongDauKy + tongDoanhThu - tongDaThu:N0} VND");
                    Console.WriteLine($"5. Công nợ quá hạn                   : {tongQuaHan:N0} VND");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX: " + ex);
            }
        }
    }
}
