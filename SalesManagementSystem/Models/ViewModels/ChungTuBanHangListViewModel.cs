using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class ChungTuBanHangListViewModel
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayChungTu { get; set; }
        public int? IDDonDatHang { get; set; }
        public string SoDonHang { get; set; }
        public int IDKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public int IDKho { get; set; }
        public string TenKhoHang { get; set; }
        public int? IDTaiKhoanThanhToan { get; set; }
        public string SoTaiKhoan { get; set; }
        public decimal TongTienHang { get; set; }
        public decimal TongTienThue { get; set; }
        public decimal TongCong { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConLai { get; set; }
        public int TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public string SoDienThoaiTaiXe { get; set; }
        public string HoTenTaiXe { get; set; }
    }
}
