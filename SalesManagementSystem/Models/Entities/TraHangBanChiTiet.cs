using System;

namespace SalesManagementSystem.Models.Entities
{
    public class TraHangBanChiTiet
    {
        public int ID { get; set; }
        public int? IDTraHang { get; set; }
        public int? IDSanPham { get; set; }
        public decimal? SoLuongBan { get; set; }
        public decimal? SoLuongDaTra { get; set; }
        public decimal? SoLuongConLai { get; set; }
        public decimal? SoLuongTra { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? ThanhTien { get; set; }
        public string GhiChu { get; set; }
        public decimal? PhiBocXep { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
    }
}
