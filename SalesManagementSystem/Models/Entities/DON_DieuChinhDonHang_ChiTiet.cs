using System;

namespace SalesManagementSystem.Models.Entities
{
    public class DON_DieuChinhDonHang_ChiTiet
    {
        public int ID { get; set; }
        public int IDDieuChinh { get; set; }
        public int IDSanPham { get; set; }
        public decimal? SoLuongCu { get; set; }
        public decimal? SoLuongMoi { get; set; }
        public decimal? DonGiaCu { get; set; }
        public decimal? DonGiaMoi { get; set; }
        public decimal? ThanhTienCu { get; set; }
        public decimal? ThanhTienMoi { get; set; }
        public string GhiChu { get; set; }
    }
}
