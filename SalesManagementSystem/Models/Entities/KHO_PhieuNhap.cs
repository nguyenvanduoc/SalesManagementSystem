using System;

namespace SalesManagementSystem.Models.Entities
{
    public class KHO_PhieuNhap
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayNhap { get; set; }
        public int IDKho { get; set; }
        public int IDNhaCungCap { get; set; }
        public string SoHoaDon { get; set; }
        public DateTime? NgayHoaDon { get; set; }
        public string TenNguoiGiao { get; set; }
        public string SoDienThoaiNguoiGiao { get; set; }
        public string TenNguoiNhan { get; set; }
        public string GhiChu { get; set; }
        public int TrangThai { get; set; } // 1: Nháp, 2: Ghi sổ, 3: Hủy
        public DateTime? NgayGhiSo { get; set; }
        public int? NguoiGhiSo { get; set; }
        public DateTime? NgayHuy { get; set; }
        public int? NguoiHuy { get; set; }
        public string LyDoHuy { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
        public bool IsDeleted { get; set; }
    }
}
