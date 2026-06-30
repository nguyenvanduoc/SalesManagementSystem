using System;
using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.ViewModels
{
    public class TraHangBanViewModel
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? NgayChungTu { get; set; }
        
        public int? IDDonDatHang { get; set; }
        public string SoDonHang { get; set; }
        
        public int? IDKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string MaKhachHang { get; set; }
        
        public int? IDKho { get; set; }
        public string TenKho { get; set; }
        
        public string LyDoTraHang { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? TongSoLuong { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? TongTienHang { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? TongTienDaHoan { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? ConPhaiHoan { get; set; }
        
        public int? TrangThai { get; set; }
        public decimal? PhiBocXep { get; set; }
        
        public string NguoiTaoName { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? NgayTao { get; set; }
        
        public decimal DaThanhToan { get; set; }
        public decimal TongTienDonHang { get; set; }
        public decimal TongTien { get; set; }
        public decimal DaTraHang { get; set; }
    }
}
