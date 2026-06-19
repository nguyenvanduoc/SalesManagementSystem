using System;
using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.ViewModels
{
    public class TaiKhoanThanhToanViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mã tài khoản")]
        [MaxLength(50, ErrorMessage = "Mã tài khoản tối đa 50 ký tự")]
        public string MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Tên tài khoản")]
        [MaxLength(250, ErrorMessage = "Tên tài khoản tối đa 250 ký tự")]
        public string TenTaiKhoan { get; set; }

        [MaxLength(250, ErrorMessage = "Ngân hàng tối đa 250 ký tự")]
        public string NganHang { get; set; }

        [MaxLength(50, ErrorMessage = "Số tài khoản tối đa 50 ký tự")]
        public string SoTaiKhoan { get; set; }

        [MaxLength(250, ErrorMessage = "Chủ tài khoản tối đa 250 ký tự")]
        public string ChuTaiKhoan { get; set; }

        public bool IsHoatDong { get; set; } = true;

        public int? IDTaiKhoanKeToan { get; set; }
    }

    public class TaiKhoanThanhToanListViewModel
    {
        public int ID { get; set; }
        public string MaTaiKhoan { get; set; }
        public string TenTaiKhoan { get; set; }
        public string NganHang { get; set; }
        public string SoTaiKhoan { get; set; }
        public string ChuTaiKhoan { get; set; }
        public bool IsHoatDong { get; set; }
        public int? IDTaiKhoanKeToan { get; set; }
        public string SoTaiKhoanKeToan { get; set; }
        public string TenTaiKhoanKeToan { get; set; }
    }
}
