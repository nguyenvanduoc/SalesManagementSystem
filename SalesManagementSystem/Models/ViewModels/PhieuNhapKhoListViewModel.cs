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
        public string TenLoaiNhap { get; set; }
        public string MaLoaiNhap { get; set; }
        public string TenKhoNguon { get; set; }
        public string TenKhachHang { get; set; }
        public int IDNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; }
        public string MaNhaCungCap { get; set; }
        public string SoHoaDon { get; set; }
        public DateTime? NgayHoaDon { get; set; }
        public string TenNguoiGiao { get; set; }
        public string SoDienThoaiNguoiGiao { get; set; }
        public string TenNguoiNhan { get; set; }
        public int TrangThai { get; set; } // 1: Nháp, 2: ghi, 3: Hủy
        public int TrangThaiThanhToan { get; set; } // 0: Chưa thanh toán, 1: Thanh toán một phần, 2: Đã thanh toán
        public decimal DaThanhToan { get; set; }
        public decimal ConLai { get; set; }
        public decimal TongTienHang { get; set; }
        public decimal TongTienThue { get; set; }
        public decimal TongCong { get; set; }
        public decimal TienVanChuyen { get; set; }
        public decimal TongSoLuong { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public string NguoiTaoText { get; set; }
        public int? IDPhuongTien { get; set; }
        public string TenPhuongTien { get; set; }
        public string HoTenTaiXe { get; set; }
        public string SoDienThoaiTaiXe { get; set; }
    }
}
