using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class TraHangBanChiTietViewModel
    {
        public int ID { get; set; }
        public int? IDTraHang { get; set; }
        
        public int? IDSanPham { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string DonViTinh { get; set; }
        
        public decimal? SoLuongBan { get; set; }
        public decimal? SoLuongDaTra { get; set; }
        public decimal? SoLuongConLai { get; set; }
        
        public decimal? SoLuongTra { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? ThanhTien { get; set; }
        
        public decimal? PhiBocXep { get; set; }
        public string GhiChu { get; set; }
    }
}
