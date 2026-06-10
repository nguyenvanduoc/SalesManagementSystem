using System;

namespace SalesManagementSystem.Models.Entities
{
    public class NS_KhachHang
    {
        public int ID { get; set; }
        public string MaSoThue { get; set; }
        public string TenKhachHang { get; set; }
        public string MaKhachHang { get; set; }
        public int? IDNhomKhachHang { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public int? IDNhanVien { get; set; }
        public int? IDTinhThanh { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayTao { get; set; } 
        public int? NguoiCapNhat { get; set; } 
        public DateTime? NgayCapNhat { get; set; }
    }
}
