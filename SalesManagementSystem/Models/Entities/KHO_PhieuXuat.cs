using System;

namespace SalesManagementSystem.Models.Entities
{
    public class KHO_PhieuXuat
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayXuat { get; set; }
        public int? IDChungTuBanHang { get; set; }
        public int? IDDonDatHang { get; set; }
        public int IDKho { get; set; }
        public int? IDNhanSuNhan { get; set; }
        public string TenNguoiNhan { get; set; }
        public string SoDienThoaiNguoiNhan { get; set; }
        public string GhiChu { get; set; }
        public decimal TongTienHang { get; set; }
        public decimal TongTienThue { get; set; }
        public decimal TongCong { get; set; }
        public int TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
        public int NguoiTao { get; set; }
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
