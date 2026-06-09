using System;

namespace SalesManagementSystem.Models.Entities
{
    public class NS_DonDatHang
    {
        public int ID { get; set; }
        public int? IDKhachHang { get; set; }
        public DateTime? NgayTaoDon { get; set; }
        public string SoDonHang { get; set; }
        public int? IDNhanVien { get; set; }
        public DateTime? ThoiHanGiaoHang { get; set; }
        public int TrangThaiDon { get; set; }
        public decimal TongTien { get; set; }
        public decimal PhiBocXep { get; set; }
        public string GhiChu { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
    }
}
