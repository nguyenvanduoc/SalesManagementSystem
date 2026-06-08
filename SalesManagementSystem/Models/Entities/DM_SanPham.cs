using System;

namespace SalesManagementSystem.Models.Entities
{
    public class DM_SanPham
    {
        public int ID { get; set; }
        public string TenSanPham { get; set; }
        public string MaSanPham { get; set; }
        public int? STT { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
    }
}
