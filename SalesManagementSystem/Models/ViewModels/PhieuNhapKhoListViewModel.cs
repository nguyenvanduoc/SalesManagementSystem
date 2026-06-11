using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuNhapKhoListViewModel
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayNhap { get; set; }
        public int IDKho { get; set; }
        public string TenKho { get; set; }
        public string MaKhoHang { get; set; }
        public int IDNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; }
        public string MaNhaCungCap { get; set; }
        public string SoHoaDon { get; set; }
        public DateTime? NgayHoaDon { get; set; }
        public string TenNguoiGiao { get; set; }
        public string SoDienThoaiNguoiGiao { get; set; }
        public int IDNhanSuNhan { get; set; }
        public string TenNhanSuNhan { get; set; }
        public int TrangThai { get; set; } // 1: Nháp, 2: Ghi sổ, 3: Hủy
        public decimal TongTienHang { get; set; }
        public decimal TongTienThue { get; set; }
        public decimal TongCong { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public string NguoiTaoText { get; set; }
    }
}
