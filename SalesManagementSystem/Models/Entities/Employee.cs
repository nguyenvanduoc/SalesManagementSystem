using System;

namespace SalesManagementSystem.Models.Entities
{
    public class Employee
    {
        public int ID { get; set; }
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string HoDem { get; set; }
        public DateTime? NgaySinh { get; set; }
        public bool? GioiTinh { get; set; }
        public string SoCMND { get; set; }
        public DateTime? NgayCap { get; set; }
        public string DiaChi { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string SoDienThoai2 { get; set; }
        public DateTime? NgayTao { get; set; }
        public int NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int NguoiCapNhat { get; set; }
    }
}
