using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SalesManagementSystem.Models.ViewModels
{
    public class TraHangBanCreateEditViewModel
    {
        public int ID { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số chứng từ")]
        public string SoChungTu { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn ngày chứng từ")]
        public DateTime? NgayChungTu { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn đơn đặt hàng")]
        public int? IDDonDatHang { get; set; }
        public string SoDonHang { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public int? IDKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string MaKhachHang { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn kho nhập")]
        public int? IDKho { get; set; }
        
        public string LyDoTraHang { get; set; }
        
        public decimal? TongSoLuong { get; set; }
        public decimal? TongTienHang { get; set; }
        public decimal? TongTienDaHoan { get; set; }
        public decimal? ConPhaiHoan { get; set; }
        
        public int? TrangThai { get; set; }
        public decimal? PhiBocXep { get; set; }
        
        // Thong tin tu don hang de tinh toan
        public decimal DaThanhToan { get; set; }
        public decimal TongTienDonHang { get; set; }
        
        public List<TraHangBanChiTietViewModel> ChiTiets { get; set; }
        
        public SelectList KhoList { get; set; }
        
        public TraHangBanCreateEditViewModel()
        {
            ChiTiets = new List<TraHangBanChiTietViewModel>();
        }
    }
}
