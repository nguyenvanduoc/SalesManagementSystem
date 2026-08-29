using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace FindMinus15B
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
                    Console.WriteLine("EXEC sp_BC_KetQuaHoatDongKinhDoanh_GetList (01/08/2026 -> 31/08/2026)");
                    Console.WriteLine("=================================================");

                    var bcList = conn.Query<dynamic>("sp_BC_KetQuaHoatDongKinhDoanh_GetList", new {
                        TuNgay = tuNgay,
                        DenNgay = denNgay
                    }, commandType: CommandType.StoredProcedure).ToList();

                    decimal totalDT = 0, totalGV = 0, totalVC = 0, totalLNGop = 0, totalLNThuan = 0;

                    foreach(var b in bcList)
                    {
                        decimal dt = b.ThanhTienDoanhThu != null ? Convert.ToDecimal(b.ThanhTienDoanhThu) : 0m;
                        decimal gv = b.ThanhTienGiaVon != null ? Convert.ToDecimal(b.ThanhTienGiaVon) : 0m;
                        decimal vc = b.ChiPhiVanChuyen != null ? Convert.ToDecimal(b.ChiPhiVanChuyen) : 0m;
                        decimal lngop = b.LoiNhuanGop != null ? Convert.ToDecimal(b.LoiNhuanGop) : 0m;
                        decimal lnthuan = b.LoiNhuanThuan != null ? Convert.ToDecimal(b.LoiNhuanThuan) : 0m;

                        totalDT += dt;
                        totalGV += gv;
                        totalVC += vc;
                        totalLNGop += lngop;
                        totalLNThuan += lnthuan;
                    }

                    Console.WriteLine(string.Format("Total Doanh Thu  : {0:N0} VND", totalDT));
                    Console.WriteLine(string.Format("Total Gia Von    : {0:N0} VND", totalGV));
                    Console.WriteLine(string.Format("Total Van Chuyen : {0:N0} VND", totalVC));
                    Console.WriteLine(string.Format("Total LN Gop     : {0:N0} VND", totalLNGop));
                    Console.WriteLine(string.Format("Total LN Thuan   : {0:N0} VND", totalLNThuan));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX: " + ex);
            }
        }
    }
}
