using System;

namespace SalesManagementSystem.Models.Entities
{
    public class ChucVu
    {
        public int ID { get; set; }
        public string TenChucVu { get; set; }
        public string MaChucVu { get; set; }
        public int? STT { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
    }
}
