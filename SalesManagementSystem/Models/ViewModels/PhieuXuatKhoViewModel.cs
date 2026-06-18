using System;
using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuXuatKhoViewModel
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayXuat { get; set; }
        
        public int? IDDonDatHang { get; set; }
        public string SoDonHang { get; set; }
        public int IDKho { get; set; }
        public string TenKhoHang { get; set; }
        public string TenKhachHang { get; set; }
        public string MaKhachHang { get; set; }
        public string DiaChiKhachHang { get; set; }
        public string SoDienThoaiKhachHang { get; set; }
        public string MaSoThueKhachHang { get; set; }
        
        public int? IDNhanSuNhan { get; set; }
        public string TenNguoiNhan { get; set; }
        public string SoDienThoaiNguoiNhan { get; set; }
        public string GhiChu { get; set; }
        
        public decimal TongTienHang { get; set; }
        public decimal TongTienThue { get; set; }
        public decimal TongCong { get; set; }
        public int TrangThai { get; set; }
        
        public List<PhieuXuatKhoChiTietViewModel> ChiTiets { get; set; } = new List<PhieuXuatKhoChiTietViewModel>();
    }

    public class PhieuXuatKhoChiTietViewModel
    {
        public int ID { get; set; }
        public int IDPhieuXuat { get; set; }
        public int IDSanPham { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string DVT { get; set; }
        public int STT { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public decimal ThueGTGT { get; set; }
        public decimal TienThue { get; set; }
        public decimal TongSauThue { get; set; }
        public string GhiChu { get; set; }
    }
}
