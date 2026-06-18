using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class NhaCungCapViewModel
    {
        public int ID { get; set; }
        public string MaNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; }
        public string DienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public string TenNguoiTao { get; set; }
    }
}
