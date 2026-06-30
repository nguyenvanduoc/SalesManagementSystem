using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.ViewModels
{
    public class DmKhoanMucChiCreateEditViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Mã khoản mục chi không được để trống")]
        [StringLength(50, ErrorMessage = "Mã khoản mục chi không được vượt quá 50 ký tự")]
        public string MaKhoanMuc { get; set; }

        [Required(ErrorMessage = "Tên khoản mục chi không được để trống")]
        [StringLength(255, ErrorMessage = "Tên khoản mục chi không được vượt quá 255 ký tự")]
        public string TenKhoanMuc { get; set; }

        public bool IsHoatDong { get; set; }
    }
}
