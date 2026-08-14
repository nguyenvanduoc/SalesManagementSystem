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
        public decimal? TongTienKhac { get; set; }
        public decimal? TongTienVanChuyen { get; set; }
        public decimal? TongTienChietKhau { get; set; }
        public decimal? TongChuongTrinhTichLuySale { get; set; }
        public decimal? ThanhTienHang { get; set; }
        public decimal? ThanhTienThue { get; set; }
        public string GhiChu { get; set; }
        public string SoDienThoaiTaiXe { get; set; }
        public string HoTenTaiXe { get; set; }
        public int? IDPhuongTien { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
    }
}
