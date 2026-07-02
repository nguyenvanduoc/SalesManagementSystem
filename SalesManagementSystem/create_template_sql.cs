using System;
using System.IO;
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
            
            // Totals row
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

            string hex = BitConverter.ToString(fileBytes).Replace("-", "");
            string sql = string.Format(@"
DELETE FROM DM_BieuMau WHERE MaBieuMau = 'KQHDKD_BaoCaoKetQuaKinhDoanh';
INSERT INTO DM_BieuMau (MaBieuMau, TenBieuMau, TenFile, DuoiFile, NoiDung, NgayTao) 
VALUES ('KQHDKD_BaoCaoKetQuaKinhDoanh', N'Báo cáo KQHDKD', 'KQHDKD_Template.xlsx', 'xlsx', 0x{0}, GETDATE());
", hex);
            File.WriteAllText(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\InsertTemplate.sql", sql);
            Console.WriteLine("SQL file created at App_Data/InsertTemplate.sql");
        }
        catch(Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.ToString());
        }
    }
}
