using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

class Program
{
    static void Main()
    {
        try
        {
            // Create Excel using NPOI
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("BaoCao");
            
            // Title
            IRow titleRow = sheet.CreateRow(0);
            ICell titleCell = titleRow.CreateCell(0);
            titleCell.SetCellValue("BÁO CÁO KẾT QUẢ HOẠT ĐỘNG KINH DOANH");
            
            // Subtitle
            IRow subTitleRow = sheet.CreateRow(1);
            ICell subTitleCell = subTitleRow.CreateCell(0);
            subTitleCell.SetCellValue("Từ ngày %TuNgay% đến ngày %DenNgay%");
            
            // Headers
            IRow headerRow = sheet.CreateRow(3);
            string[] headers = { "STT", "Mã sản phẩm", "Tên sản phẩm", "ĐVT", "Số lượng doanh thu", "Thành tiền doanh thu", "Số lượng giá vốn", "Thành tiền giá vốn", "Chi phí vận chuyển", "Chi phí bao bì", "Lợi nhuận gộp", "Lợi nhuận thuần", "Tỷ suất LN" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                ICell cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
            }
            
            // Template row
            IRow templateRow = sheet.CreateRow(4);
            for (int i = 0; i < headers.Length; i++)
            {
                templateRow.CreateCell(i).SetCellValue("");
            }
            
            // Totals row (below template, doesn't matter where it is initially, but let's put it on row 6)
            IRow totalsRow = sheet.CreateRow(6);
            totalsRow.CreateCell(0).SetCellValue("Tổng cộng:");
            totalsRow.CreateCell(5).SetCellValue("%TotalDoanhThu%");
            totalsRow.CreateCell(7).SetCellValue("%TotalGiaVon%");
            totalsRow.CreateCell(8).SetCellValue("%TotalChiPhiVanChuyen%");
            totalsRow.CreateCell(9).SetCellValue("%TotalChiPhiBaoBi%");
            totalsRow.CreateCell(10).SetCellValue("%TotalLoiNhuanGop%");
            totalsRow.CreateCell(11).SetCellValue("%TotalLoiNhuanThuan%");
            totalsRow.CreateCell(12).SetCellValue("%TotalTySuatLN%");
            
            byte[] fileBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                fileBytes = ms.ToArray();
            }

            // Insert into DB
            Assembly asm = Assembly.LoadFrom(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll");
            Type factoryType = asm.GetType("SalesManagementSystem.Data.DbConnectionFactory");
            object factory = Activator.CreateInstance(factoryType);
            
            MethodInfo createConnMethod = factoryType.GetMethod("CreateConnection");
            using (IDbConnection conn = (IDbConnection)createConnMethod.Invoke(factory, null))
            {
                conn.Open();
                
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM DM_BieuMau WHERE MaBieuMau = 'KQHDKD_BaoCaoKetQuaKinhDoanh';";
                    cmd.ExecuteNonQuery();
                    
                    cmd.CommandText = @"INSERT INTO DM_BieuMau (MaBieuMau, TenBieuMau, TenFile, DuoiFile, NoiDung, NgayTao) 
                                        VALUES (@MaBieuMau, @TenBieuMau, @TenFile, @DuoiFile, @NoiDung, GETDATE());";
                                        
                    var p1 = cmd.CreateParameter(); p1.ParameterName = "@MaBieuMau"; p1.Value = "KQHDKD_BaoCaoKetQuaKinhDoanh"; cmd.Parameters.Add(p1);
                    var p2 = cmd.CreateParameter(); p2.ParameterName = "@TenBieuMau"; p2.Value = "Báo cáo Kết quả Kinh doanh"; cmd.Parameters.Add(p2);
                    var p3 = cmd.CreateParameter(); p3.ParameterName = "@TenFile"; p3.Value = "KQHDKD_Template.xlsx"; cmd.Parameters.Add(p3);
                    var p4 = cmd.CreateParameter(); p4.ParameterName = "@DuoiFile"; p4.Value = "xlsx"; cmd.Parameters.Add(p4);
                    var p5 = cmd.CreateParameter(); p5.ParameterName = "@NoiDung"; p5.Value = fileBytes; cmd.Parameters.Add(p5);
                    
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine("Template inserted successfully.");
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.ToString());
        }
    }
}
