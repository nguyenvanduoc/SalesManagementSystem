using System;

namespace SalesManagementSystem.Models.Entities
{
    public class KHO_DieuChinhPhieuNhapChiTiet
    {
        public int ID { get; set; }
        public int IDDieuChinh { get; set; }
        public int IDPhieuNhapChiTiet { get; set; }
        public int? IDSanPhamCu { get; set; }
        public int? IDSanPhamMoi { get; set; }
        public decimal? SoLuongCu { get; set; }
        public decimal? SoLuongMoi { get; set; }
        public decimal? DonGiaCu { get; set; }
        public decimal? DonGiaMoi { get; set; }
        public decimal? ThanhTienCu { get; set; }
        public decimal? ThanhTienMoi { get; set; }
        public string LoaiThayDoi { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
    }
}
