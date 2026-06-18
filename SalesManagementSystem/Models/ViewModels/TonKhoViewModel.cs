using System;
using System.Collections.Generic;
namespace SalesManagementSystem.Models.ViewModels
{
    public class TonKhoFilterViewModel
    {
        public int? IDKho { get; set; }
        public int? IDSanPham { get; set; }
        public string TuNgay { get; set; }
        public string DenNgay { get; set; }
        public bool ChiConTon { get; set; }
    }

    public class TonKhoListViewModel
    {
        public int IDKho { get; set; }
        public string MaKho { get; set; }
        public string TenKho { get; set; }
        public int IDSanPham { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string DVT { get; set; }
        public decimal TongNhap { get; set; }
        public decimal TongXuat { get; set; }
        public decimal TonKho { get; set; }
        public decimal DonGiaTon { get; set; }
        public decimal DonGiaCuoi => DonGiaTon; // Alias for Excel mapping
        public decimal GiaTriTon { get; set; }
        public DateTime? NgayNhapCuoi { get; set; }
        public DateTime? NgayXuatCuoi { get; set; }
        public decimal MucTonToiThieu { get; set; }
    }

    public class TheKhoListViewModel
    {
        public int ID { get; set; }
        public DateTime NgayChungTu { get; set; }
        public string SoChungTu { get; set; }
        public int LoaiChungTu { get; set; }
        public string DienGiai { get; set; }
        public decimal Nhap { get; set; }
        public decimal Xuat { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public decimal TonLuyKe { get; set; }
    }

    public class PrintTheKhoMultiViewModel
    {
        public string TenKho { get; set; }
        public string TenSanPham { get; set; }
        public IEnumerable<TheKhoListViewModel> TheKhoList { get; set; }
    }

    public class TonKhoDashboardViewModel
    {
        public int TongSoSanPham { get; set; }
        public decimal TongSoLuongTon { get; set; }
        public decimal TongGiaTriTon { get; set; }
        public int SoSanPhamAmKho { get; set; }
        public int SoSanPhamSapHetHang { get; set; }
    }
}
