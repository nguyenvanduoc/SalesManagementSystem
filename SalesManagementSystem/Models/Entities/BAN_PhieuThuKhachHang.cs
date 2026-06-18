using System;

namespace SalesManagementSystem.Models.Entities
{
    public class BAN_PhieuThuKhachHang
    {
        public int ID { get; set; }
        public string SoPhieuThu { get; set; }
        public DateTime NgayThu { get; set; }
        public int IDChungTuBanHang { get; set; }
        public int IDKhachHang { get; set; }
        public int IDTaiKhoanThanhToan { get; set; }
        public decimal SoTienThu { get; set; }
        public string GhiChu { get; set; }
        public int TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
        public DateTime? NgayGhi { get; set; }
        public int? NguoiGhi { get; set; }
        public DateTime? NgayHuy { get; set; }
        public int? NguoiHuy { get; set; }
        public string LyDoHuy { get; set; }
        public bool IsDeleted { get; set; }
        public int? IDNguoiThu { get; set; }
    }
}
