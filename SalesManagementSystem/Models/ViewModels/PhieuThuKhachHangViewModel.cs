using System;
using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuThuKhachHangViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Số phiếu thu không được để trống")]
        public string SoPhieuThu { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày thu")]
        public DateTime NgayThu { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Khách hàng không được để trống")]
        public int? IDKhachHang { get; set; }
        public string TenKhachHang { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tài khoản thanh toán")]
        public int IDTaiKhoanThanhToan { get; set; }
        public string TenTaiKhoanThanhToan { get; set; }

        public string NguoiNopTien { get; set; }
        public string SoDienThoaiNguoiNop { get; set; }

        [Required(ErrorMessage = "Số tiền thu không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền thu phải lớn hơn 0")]
        public decimal SoTienThu { get; set; }

        public string DienGiai { get; set; }
        public int TrangThai { get; set; }
        public string LyDoHuy { get; set; }

        public decimal TienTraTruocKhachHang { get; set; }
        
        public System.Collections.Generic.List<PhieuThuKhachHangChiTietViewModel> ChiTiets { get; set; }

        public DateTime NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }

        public PhieuThuKhachHangViewModel()
        {
            NgayThu = DateTime.Today;
            ChiTiets = new System.Collections.Generic.List<PhieuThuKhachHangChiTietViewModel>();
            TrangThai = 1;
        }
    }

    public class PhieuThuKhachHangChiTietViewModel
    {
        public int ID { get; set; }
        public int IDPhieuThu { get; set; }
        public int? IDChungTu { get; set; }
        public int? IDChungTuBanHang { get; set; }
        public string SoChungTu { get; set; }
        public int LoaiThu { get; set; }
        public string DienGiai { get; set; }
        public decimal SoTienPhanBo { get; set; }
        public decimal TongCong { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConLai { get; set; }
    }
}
