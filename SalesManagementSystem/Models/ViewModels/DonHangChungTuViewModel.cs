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
        
        public int? IDChungTuBanHang { get; set; }
        public string SoChungTu { get; set; }
        public DateTime? NgayChungTu { get; set; }
        public int? TrangThaiChungTu { get; set; }
    }
}
