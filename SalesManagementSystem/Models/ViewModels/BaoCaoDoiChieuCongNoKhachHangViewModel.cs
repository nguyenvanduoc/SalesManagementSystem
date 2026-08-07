using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class BaoCaoDoiChieuCongNoKhachHangViewModel
    {
        public int STT { get; set; }
        public DateTime? NgayPhatSinh { get; set; }
        public string SoChungTu { get; set; }
        public string TenNhanVien { get; set; }
        public string TenKhuVuc { get; set; }
        public string TenTinh { get; set; }
        public string TenKhachHang { get; set; }
        public string LoaiPhatSinh { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string DienGiai { get; set; }
        public decimal SoLuongBan { get; set; }
        public decimal DonGiaBan { get; set; }
        public decimal PhaiThu { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNoLuyKe { get; set; }
        public string GhiChu { get; set; }
        public int LoaiDong { get; set; } // 0: Nợ đầu kỳ, 1: Bán hàng, 2: Trả hàng, 3: Thu tiền
        public int ThuTuSapXep { get; set; }
        public int IDPhatSinh { get; set; }
    }
}
