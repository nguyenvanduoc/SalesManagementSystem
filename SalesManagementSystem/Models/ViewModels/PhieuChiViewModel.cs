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

        [Required(ErrorMessage = "Vui lòng nhập người nhận tiền")]
        [StringLength(100, ErrorMessage = "Người nhận tiền không được vượt quá 100 ký tự")]
        public string NguoiNhanTien { get; set; }

        public int? IDNguoiNhan { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string SoDienThoaiNguoiNhan { get; set; }

        public int? IDNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; }

        public int? IDPhieuNhap { get; set; }
        public string SoPhieuNhap { get; set; }

        public int? IDLoaiChiTien { get; set; }
        public string MaLoaiChiTien { get; set; }
        public string TenLoaiChiTien { get; set; }
        public int? IDPhuongTien { get; set; }
        public string TenPhuongTien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền chi")]
        public decimal SoTienChi { get; set; }

        [StringLength(500, ErrorMessage = "Diễn giải không được vượt quá 500 ký tự")]
        public string DienGiai { get; set; }

        public int TrangThai { get; set; }
        public string LyDoHuy { get; set; }
        
        public decimal TienTraTruocNCC { get; set; }
        
        public System.Collections.Generic.List<PhieuChiChiTietViewModel> ChiTiets { get; set; }

        public PhieuChiViewModel()
        {
            NgayChi = DateTime.Today;
            ChiTiets = new System.Collections.Generic.List<PhieuChiChiTietViewModel>();
        }

        // Display fields (read-only)
        public string TenKhoanMuc { get; set; }
        public string TenTaiKhoanThanhToan { get; set; }
        public string TenNguoiNhan { get; set; }
    }
}
