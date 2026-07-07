using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class CongNoKhachHangViewModel
    {
        public int IDChungTuBanHang { get; set; }
        public int? IDDonDatHang { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayChungTu { get; set; }
        public int? IDKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string DienThoai { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal DaThu { get; set; }
        public decimal ConPhaiThu { get; set; }
        public decimal TonDauKy { get; set; }
        public decimal TienQuaHan { get; set; }
        
        public decimal LuyKe => TonDauKy + DoanhThu;

        public string TrangThai
        {
            get
            {
                if (ConPhaiThu <= 0) return "Đã thanh toán";
                if (TienQuaHan > 0) return "Quá hạn";
                if (DaThu > 0) return "Thanh toán một phần";
                return "Chưa thanh toán";
            }
        }

        public string CssTrangThai
        {
            get
            {
                if (ConPhaiThu <= 0) return "badge bg-success";
                if (TienQuaHan > 0) return "badge"; // We will add custom inline styling or class
                if (DaThu > 0) return "badge bg-warning text-dark";
                return "badge bg-danger";
            }
        }
    }
}
