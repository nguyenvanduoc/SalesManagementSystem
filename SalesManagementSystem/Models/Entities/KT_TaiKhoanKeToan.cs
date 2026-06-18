using System;

namespace SalesManagementSystem.Models.Entities
{
    public class KT_TaiKhoanKeToan
    {
        public int ID { get; set; }
        public string SoTaiKhoan { get; set; }
        public string TenTaiKhoan { get; set; }
        public int? IDTaiKhoanCha { get; set; }
        public bool IsChiTiet { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
    }
}
