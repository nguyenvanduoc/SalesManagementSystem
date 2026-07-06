using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuThuKhachHangListViewModel
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayChungTu { get; set; }
        public int IDKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public decimal TongCong { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConLai { get; set; }
        public bool HasDinhKem { get; set; }
        public int TrangThaiCongNo { get; set; }
        public string TenNguoiTao { get; set; }
        public DateTime NgayTao { get; set; }
    }
}
