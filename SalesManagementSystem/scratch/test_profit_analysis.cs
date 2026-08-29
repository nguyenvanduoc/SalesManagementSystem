using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace TestProfitAnalysis
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
                    Console.WriteLine("1. CHECK ALL BAN_ChungTuBanHang (INCLUDING DELETED & DRAFT)");
                    Console.WriteLine("=================================================");
                    var allVouchers = conn.Query<dynamic>(@"
                        SELECT ID, SoChungTu, NgayChungTu, TongCong, TrangThai, IsDeleted
                        FROM BAN_ChungTuBanHang
                        WHERE NgayChungTu >= @TuNgay AND NgayChungTu <= @DenNgay
                        ORDER BY ID ASC", new { TuNgay = tuNgay, DenNgay = denNgay }).ToList();

                    Console.WriteLine(string.Format("Total rows: {0}", allVouchers.Count));
                    foreach (var v in allVouchers)
                    {
                        Console.WriteLine(string.Format("ID: {0} | SoCT: {1} | Ngay: {2:dd/MM/yyyy HH:mm} | TongCong: {3:N0} | TrangThai: {4} | IsDeleted: {5}",
                            v.ID, v.SoChungTu, v.NgayChungTu, v.TongCong, v.TrangThai, v.IsDeleted));
                    }

                    Console.WriteLine("\n=================================================");
                    Console.WriteLine("2. CHECK PREVIOUS VERSION OF SP OR CALCULATION OF GIÁ VỐN & LỢI NHUẬN");
                    Console.WriteLine("=================================================");

                    // How was Gia Von calculated previously or if GiaVon was calculated from KHO_PhieuNhap / KHO_GiaoDichKho?
                    // Let's check if there are other columns or calculation formulas in Dashboard SP or past files!
                    
                    var oldSpFiles = System.IO.Directory.GetFiles(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem", "*Dashboard*", System.IO.SearchOption.AllDirectories);
                    foreach(var f in oldSpFiles)
                    {
                        if (f.EndsWith(".sql") || f.EndsWith(".cs") || f.EndsWith(".txt"))
                        {
                            Console.WriteLine("Found file: " + f);
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
