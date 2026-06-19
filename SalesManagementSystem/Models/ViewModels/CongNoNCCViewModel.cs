using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class CongNoNCCViewModel
    {
        public int IDPhieuNhap { get; set; }
        public string SoPhieuNhap { get; set; }
        public DateTime NgayNhap { get; set; }
        public int? IDNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; }
        public string DienThoaiNCC { get; set; }
        public decimal TongTienHang { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConLai { get; set; }

        public string TrangThaiCongNo
        {
            get
            {
                if (ConLai <= 0) return "Đã thanh toán";
                if (DaThanhToan > 0) return "Còn nợ một phần";
                return "Chưa thanh toán";
            }
        }

        public string CssTrangThaiCongNo
        {
            get
            {
                if (ConLai <= 0) return "badge bg-success";
                if (DaThanhToan > 0) return "badge bg-warning text-dark";
                return "badge bg-danger";
            }
        }
    }
}
