using System;
using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public class ChungTuBanHangViewModel
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayChungTu { get; set; }
        
        public int? IDDonDatHang { get; set; }
        public string SoDonHang { get; set; }
        public DateTime? NgayTaoDon { get; set; }
        public DateTime? ThoiHanGiaoHang { get; set; }
        public int? IDNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public int? TrangThaiDon { get; set; }
        
        public int IDKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string MaKhachHang { get; set; }
        public string MaSoThue { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public string SoDienThoaiTaiXe { get; set; }
        public string HoTenTaiXe { get; set; }
        public int? IDPhuongTien { get; set; }
        public string GhiChuDonHang { get; set; }
        
        public int IDKho { get; set; }
        public string TenKhoHang { get; set; }
        
        public int? IDTaiKhoanThanhToan { get; set; }
        public string SoTaiKhoanThanhToan { get; set; }
        
        public decimal TongTienHang { get; set; }
        public decimal TongTienThue { get; set; }
        public decimal PhiBocXep { get; set; }
        public decimal TongCong { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConLai { get; set; }
        
        public int TrangThai { get; set; }
        public string GhiChu { get; set; } // For form, not in DB maybe? Let's omit if not in DB.

        public List<ChungTuBanHangChiTietViewModel> ChiTiets { get; set; }

        public ChungTuBanHangViewModel()
        {
            ChiTiets = new List<ChungTuBanHangChiTietViewModel>();
            NgayChungTu = DateTime.Now;
        }
    }

    public class CheckTonKhoRequestItem
    {
        public int IDSanPham { get; set; }
        public decimal SoLuongCanXuat { get; set; }
    }

    public class CheckTonKhoResponseViewModel
    {
        public int IDKho { get; set; }
        public string TenKhoHang { get; set; }
        public int IDSanPham { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public decimal SoLuongCanXuat { get; set; }
        public decimal SoLuongTon { get; set; }
        public decimal ChenhLech { get; set; }
        public bool IsDuTon { get; set; }
    }
}
