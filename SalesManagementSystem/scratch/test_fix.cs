using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace TestFix
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
                    Console.WriteLine("1. MÀN HÌNH CÔNG NỢ NCC (01/08/2026 -> 31/08/2026)");
                    Console.WriteLine("=================================================");
                    var listAug = conn.Query<dynamic>("sp_CongNo_PhaseTra_NCC_GetList", new {
                        TuNgay = tuNgay,
                        DenNgay = denNgay,
                        IDNhaCungCap = (int?)null,
                        TrangThaiCongNo = (int?)null
                    }, commandType: CommandType.StoredProcedure).ToList();

                    decimal sumTongHangAug = listAug.Sum(x => (decimal)(x.TongTienHang ?? 0m));
                    decimal sumDaTraAug   = listAug.Sum(x => (decimal)(x.DaThanhToan ?? 0m));
                    decimal sumConLaiAug  = listAug.Sum(x => (decimal)(x.ConLai ?? 0m));

                    Console.WriteLine($"Màn hình Công nợ NCC - Tổng tiền hàng : {sumTongHangAug:N0}");
                    Console.WriteLine($"Màn hình Công nợ NCC - Đã thanh toán  : {sumDaTraAug:N0}");
                    Console.WriteLine($"Màn hình Công nợ NCC - Còn lại        : {sumConLaiAug:N0}");

                    Console.WriteLine("\n=================================================");
                    Console.WriteLine("2. DASHBOARD STORED PROCEDURE (sp_Dashboard_GetData 01/08/2026 -> 31/08/2026)");
                    Console.WriteLine("=================================================");
                    using (var grid = conn.QueryMultiple("sp_Dashboard_GetData", new {
                        TuNgay = tuNgay,
                        DenNgay = denNgay,
                        TuNgayKyTruoc = new DateTime(2026, 7, 1),
                        DenNgayKyTruoc = new DateTime(2026, 7, 31)
                    }, commandType: CommandType.StoredProcedure))
                    {
                        var summary = grid.Read<dynamic>().FirstOrDefault();
                        if (summary != null)
                        {
                            var dict = (System.Collections.Generic.IDictionary<string, object>)summary;
                            Console.WriteLine($"Dashboard - TongTienHangNCC : {dict["TongTienHangNCC"]:N0}");
                            Console.WriteLine($"Dashboard - DaThanhToanNCC  : {dict["DaThanhToanNCC"]:N0}");
                            Console.WriteLine($"Dashboard - CongNoNhaCungCap: {dict["CongNoNhaCungCap"]:N0}");

                            bool matchTong = (decimal)dict["TongTienHangNCC"] == sumTongHangAug;
                            bool matchPaid = (decimal)dict["DaThanhToanNCC"] == sumDaTraAug;
                            bool matchDebt = (decimal)dict["CongNoNhaCungCap"] == sumConLaiAug;

                            if (matchTong && matchPaid && matchDebt)
                            {
                                Console.WriteLine("\n>>> RESULT: SUCCESS! ALL NUMBERS MATCH 100% PERFECTLY! <<<");
                            }
                            else
                            {
                                Console.WriteLine("\n>>> RESULT: MISMATCH ENCOUNTERED <<<");
                            }
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
