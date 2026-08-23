using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class DonHangChungTuViewModel
    {
        public int IDDonDatHang { get; set; }
        public string SoDonHang { get; set; }
        public DateTime? NgayTaoDon { get; set; }
        public string TenKhachHang { get; set; }
        public decimal TongTien { get; set; }
        public decimal ThanhTienHang { get; set; }
        
        public int? IDChungTuBanHang { get; set; }
        public string SoChungTu { get; set; }
        public DateTime? NgayChungTu { get; set; }
        public int? TrangThaiChungTu { get; set; }
        public decimal PhiBocXep { get; set; }
        public string SoDienThoaiTaiXe { get; set; }
        public string HoTenTaiXe { get; set; }

        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public decimal SoLuong { get; set; }
    }
}
