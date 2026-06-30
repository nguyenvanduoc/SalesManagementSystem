using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class DmKhoanMucChiViewModel
    {
        public int ID { get; set; }
        public string MaKhoanMuc { get; set; }
        public string TenKhoanMuc { get; set; }
        public bool IsHoatDong { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }

        public string TenNguoiTao { get; set; }
        public string TenNguoiCapNhat { get; set; }
    }
}
