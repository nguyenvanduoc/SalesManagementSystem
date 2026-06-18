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
        public DateTime NgayThu { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chứng từ bán hàng")]
        public int IDChungTuBanHang { get; set; }
        public string SoChungTuBanHang { get; set; }

        [Required(ErrorMessage = "Khách hàng không được để trống")]
        public int IDKhachHang { get; set; }
        public string TenKhachHang { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tài khoản thanh toán")]
        public int IDTaiKhoanThanhToan { get; set; }
        public string SoTaiKhoan { get; set; }
        public string TenTaiKhoan { get; set; }

        [Required(ErrorMessage = "Số tiền thu không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền thu phải lớn hơn 0")]
        public decimal SoTienThu { get; set; }

        public int? IDNguoiThu { get; set; }
        public string TenNguoiThu { get; set; }

        public string GhiChu { get; set; }
        public int TrangThai { get; set; }

        public DateTime NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
        public DateTime? NgayGhi { get; set; }
        public int? NguoiGhi { get; set; }
        public DateTime? NgayHuy { get; set; }
        public int? NguoiHuy { get; set; }
        public string LyDoHuy { get; set; }

        // Công nợ tham chiếu và các thuộc tính hỗ trợ hiển thị
        public decimal TongChungTu { get; set; }
        public decimal DaThanhToanTruoc { get; set; }
        public decimal ConLaiSauThu { get; set; }
        public string TenNguoiTao { get; set; }

        public PhieuThuKhachHangViewModel()
        {
            NgayThu = DateTime.Now;
            TrangThai = 1; // 1: Đề nghị ghi
        }
    }
}
