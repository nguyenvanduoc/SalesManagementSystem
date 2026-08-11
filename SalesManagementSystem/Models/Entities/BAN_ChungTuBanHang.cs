using System;

namespace SalesManagementSystem.Models.Entities
{
    public class BAN_ChungTuBanHang
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayChungTu { get; set; }
        public int? IDDonDatHang { get; set; }
        public int IDKhachHang { get; set; }
        public int IDKho { get; set; }
        public int? IDTaiKhoanThanhToan { get; set; }
        public decimal TongTienHang { get; set; }
        public decimal TongTienThue { get; set; }
        public decimal TongCong { get; set; }
        public decimal PhiBocXep { get; set; }
        public decimal? TongTienChietKhau { get; set; }
        public decimal? TongChuongTrinhTichLuySale { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConLai { get; set; }
        public int TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
        public DateTime? NgayGhi { get; set; }
        public int? NguoiGhi { get; set; }
        public DateTime? NgayHuy { get; set; }
        public int? NguoiHuy { get; set; }
        public string LyDoHuy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
