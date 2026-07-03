using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuChiChiTietViewModel
    {
        public int ID { get; set; }
        public int? IDPhieuChi { get; set; }
        public int? IDPhieuNhap { get; set; }
        public int LoaiChi { get; set; }
        public decimal SoTienPhanBo { get; set; }
        public string DienGiai { get; set; }
        
        // For UI display
        public string SoPhieuNhap { get; set; }
        public DateTime? NgayNhap { get; set; }
        public decimal? TongTien { get; set; }
        public decimal? DaThanhToan { get; set; }
        public decimal? ConLai { get; set; }
    }
}
