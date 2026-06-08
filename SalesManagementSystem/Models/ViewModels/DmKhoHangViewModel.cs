using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class DmKhoHangViewModel
    {
        public int ID { get; set; }
        public string MaKhoHang { get; set; }
        public string TenKhoHang { get; set; }
        public int? STT { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }

        public string TenNguoiTao { get; set; }
        public string TenNguoiCapNhat { get; set; }
    }
}
