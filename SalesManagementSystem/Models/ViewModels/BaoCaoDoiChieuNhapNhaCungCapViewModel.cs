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
        public string TenNhaCungCap { get; set; }
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

    /// <summary>
    /// Một dòng của Bảng tổng hợp công nợ phải trả (mỗi dòng = 1 nhà cung cấp).
    /// Nợ = phát sinh phải trả (phiếu nhập), Có = đã thanh toán (phiếu chi).
    /// </summary>
    public class BaoCaoTongHopCongNoPhaiTraViewModel
    {
        public int IDNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; }
        public decimal SoDuDauKy { get; set; }
        public decimal PhatSinhNo { get; set; }
        public decimal PhatSinhCo { get; set; }
        public decimal SoDuCuoiKy => SoDuDauKy + PhatSinhNo - PhatSinhCo;
    }
}
