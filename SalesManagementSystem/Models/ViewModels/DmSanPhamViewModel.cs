using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class DmSanPhamViewModel
    {
        public int ID { get; set; }
        public string TenSanPham { get; set; }
        public string MaSanPham { get; set; }
        public string DVT { get; set; }
        public int? STT { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
        public string TenNguoiTao { get; set; }
    }
}
