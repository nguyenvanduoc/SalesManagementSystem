using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuXuatKhoListViewModel
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayXuat { get; set; }
        
        public int? IDDonDatHang { get; set; }
        public string SoDonHang { get; set; }
        public DateTime? NgayDonHang { get; set; }
        public int? TrangThaiDonHang { get; set; }
        
        public string TenKhachHang { get; set; }
        public string TenKhoHang { get; set; }
        public string TenNguoiNhan { get; set; }
        public string GhiChu { get; set; }
        
        public decimal TongCong { get; set; }
        public decimal TongSoLuong { get; set; }
        public int TrangThai { get; set; }
    }
}
