using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class CongNoKhachHangSP02ViewModel
    {
        public int IDKhachHang { get; set; }
        public string TenNhanVien { get; set; }
        public string TenKhuVuc { get; set; }
        public string TinhThanh { get; set; }
        public string TenKhachHang { get; set; }
        public decimal DuDauKy { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal ThanhToan { get; set; }
        public decimal KhachThanhToanTruoc { get; set; }
        public decimal HangChoGiao { get; set; }
        public string GhiChu { get; set; }
        
        public decimal DuCuoiKy => DuDauKy + DoanhThu - ThanhToan;
    }
}
