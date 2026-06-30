using System;

namespace SalesManagementSystem.Models.Entities
{
    public class TraHangBan
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime? NgayChungTu { get; set; }
        public int? IDDonDatHang { get; set; }
        public int? IDKhachHang { get; set; }
        public int? IDKho { get; set; }
        public string LyDoTraHang { get; set; }
        public decimal? TongSoLuong { get; set; }
        public decimal? TongTienHang { get; set; }
        public decimal? TongTienDaHoan { get; set; }
        public decimal? ConPhaiHoan { get; set; }
        public int? TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
        public decimal? PhiBocXep { get; set; }
    }
}
