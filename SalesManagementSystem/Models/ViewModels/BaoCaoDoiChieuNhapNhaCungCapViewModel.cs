using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class BaoCaoDoiChieuNhapNhaCungCapViewModel
    {
        public int STT { get; set; }
        public DateTime? NgayPhatSinh { get; set; }
        public string SoChungTu { get; set; }
        public string LoaiPhatSinh { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string DienGiai { get; set; }
        public decimal SoLuongNhap { get; set; }
        public decimal DonGiaNhap { get; set; }
        public decimal PhaiTra { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNoLuyKe { get; set; }
        public string GhiChu { get; set; }
        public int LoaiDong { get; set; }
        public int ThuTuSapXep { get; set; }
        public int IDPhatSinh { get; set; }
    }
}
