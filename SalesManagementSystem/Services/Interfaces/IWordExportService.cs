using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ Base Export Word dùng chung toàn hệ thống
    /// </summary>
    public interface IWordExportService
    {
        /// <summary>
        /// Xuất file Word (hoặc PDF) từ biểu mẫu cấu hình trong DM_BieuMau
        /// </summary>
        /// <param name="maBieuMau">Mã biểu mẫu cấu hình trong database</param>
        /// <param name="data">Object chứa dữ liệu biến đơn cần thay thế</param>
        /// <param name="tables">Dictionary chứa các danh sách dữ liệu dùng để lặp bảng (DataTable, List...)</param>
        /// <param name="isPdf">Xác định xem có chuyển đổi sang file PDF hay không</param>
        /// <returns>Đối tượng ExportResult chứa file byte array và thông tin file</returns>
        ExportResult ExportWord(string maBieuMau, object data, Dictionary<string, object> tables = null, bool isPdf = false);
    }
}
