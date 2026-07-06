using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuNhapChiTietDetailViewModel
    {
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string DVT { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public decimal DonGiaVanChuyen { get; set; }
        public decimal TienVanChuyen { get; set; }
        public decimal TongSauThue { get; set; }
        public string GhiChu { get; set; }
    }
}
