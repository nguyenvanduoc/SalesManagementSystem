using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuNhapKhoChiTietViewModel
    {
        public int ID { get; set; }
        public int IDPhieuNhap { get; set; }
        public int IDSanPham { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string DVT { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public decimal ThueGTGT { get; set; }
        public decimal TienThue { get; set; }
        public decimal TongSauThue { get; set; }
        public string GhiChu { get; set; }
        public DateTime? NgaySanXuat { get; set; }
        public DateTime? HanSuDung { get; set; }
    }
}
