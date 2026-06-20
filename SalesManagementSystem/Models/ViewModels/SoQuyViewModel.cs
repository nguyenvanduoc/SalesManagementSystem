using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class SoQuyViewModel
    {
        public DateTime NgayChungTu { get; set; }
        public string SoChungTu { get; set; }
        public string LoaiChungTu { get; set; }   // "THU" hoặc "CHI"
        public int IDTaiKhoanThanhToan { get; set; }
        public string TenTaiKhoanThanhToan { get; set; }
        public string DienGiai { get; set; }
        public decimal SoTienThu { get; set; }
        public decimal SoTienChi { get; set; }
        public int TrangThai { get; set; }
    }

    public class TaiKhoanSummaryViewModel
    {
        public int ID { get; set; }
        public string TenTaiKhoan { get; set; }
        public string NganHang { get; set; }
        public string SoTaiKhoan { get; set; }
        public string ChuTaiKhoan { get; set; }
        public decimal SoDuDauKy { get; set; }
        public decimal ThuTrongKy { get; set; }
        public decimal ChiTrongKy { get; set; }
        public decimal SoDuCuoiKy => SoDuDauKy + ThuTrongKy - ChiTrongKy;
    }

    public class GiaoDichChiTietViewModel
    {
        public int STT { get; set; }
        public DateTime NgayGiaoDich { get; set; }
        public string SoChungTu { get; set; }
        public string LoaiChungTu { get; set; } // "THU" hoặc "CHI"
        public string DienGiai { get; set; }
        public decimal SoTienThu { get; set; }
        public decimal SoTienChi { get; set; }
        public decimal SoDuLuyKe { get; set; }
    }
}
