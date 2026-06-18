using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class KhachHangViewModel
    {
        public int ID { get; set; }
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string MaSoThue { get; set; }
        public int? IDNhomKhachHang { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public int? IDNhanVien { get; set; }
        public int? IDTinhThanh { get; set; }
        
        // Joined fields
        public string TenNhomKhachHang { get; set; }
        public string TenNhanVien { get; set; }
        public string TenTinhThanh { get; set; }
        
        // Audit fields
        public int? NguoiTao { get; set; }
        public DateTime? NgayTao { get; set; } 
        public int? NguoiCapNhat { get; set; } 
        public DateTime? NgayCapNhat { get; set; }
    }
}
