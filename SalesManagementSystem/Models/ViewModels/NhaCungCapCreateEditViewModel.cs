using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.ViewModels
{
    public class NhaCungCapCreateEditViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Mã nhà cung cấp không được để trống")]
        [StringLength(50, ErrorMessage = "Mã nhà cung cấp không được vượt quá 50 ký tự")]
        public string MaNhaCungCap { get; set; }

        [Required(ErrorMessage = "Tên nhà cung cấp không được để trống")]
        [StringLength(255, ErrorMessage = "Tên nhà cung cấp không được vượt quá 255 ký tự")]
        public string TenNhaCungCap { get; set; }

        [StringLength(50, ErrorMessage = "Điện thoại không được vượt quá 50 ký tự")]
        [RegularExpression(@"^[0-9\-\+\s\(\)]{9,15}$", ErrorMessage = "Điện thoại không đúng định dạng")]
        public string DienThoai { get; set; }

        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
        public string DiaChi { get; set; }
    }
}
