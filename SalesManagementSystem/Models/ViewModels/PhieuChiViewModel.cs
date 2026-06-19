using System;
using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuChiViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Số phiếu chi không được rỗng.")]
        [StringLength(50)]
        public string SoPhieuChi { get; set; }

        [Required(ErrorMessage = "Ngày chi không được rỗng.")]
        public DateTime NgayChi { get; set; } = DateTime.Today;

        public int? IDKhoanMucChi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tài khoản thanh toán.")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn tài khoản thanh toán.")]
        public int IDTaiKhoanThanhToan { get; set; }

        public int? IDNguoiNhan { get; set; }

        [StringLength(255)]
        public string NguoiNhanTien { get; set; }

        [StringLength(50)]
        public string SoDienThoaiNguoiNhan { get; set; }

        public int? IDNhaCungCap { get; set; }
        public int? IDPhieuNhap { get; set; }

        [Required(ErrorMessage = "Số tiền chi không được rỗng.")]
        [Range(1, double.MaxValue, ErrorMessage = "Số tiền chi phải lớn hơn 0.")]
        public decimal? SoTienChi { get; set; }

        [StringLength(1000)]
        public string DienGiai { get; set; }

        public int TrangThai { get; set; } = 1;
        public string LyDoHuy { get; set; }

        // Display fields (read-only)
        public string TenKhoanMuc { get; set; }
        public string TenTaiKhoanThanhToan { get; set; }
        public string TenNguoiNhan { get; set; }
        public string TenNhaCungCap { get; set; }
        public string SoPhieuNhap { get; set; }
    }
}
