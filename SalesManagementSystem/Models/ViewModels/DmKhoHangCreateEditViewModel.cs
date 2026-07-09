using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.ViewModels
{
    public class DmKhoHangCreateEditViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Mã kho hàng không được để trống")]
        [StringLength(100, ErrorMessage = "Mã kho hàng không được vượt quá 100 ký tự")]
        public string MaKhoHang { get; set; }

        [Required(ErrorMessage = "Tên kho hàng không được để trống")]
        [StringLength(500, ErrorMessage = "Tên kho hàng không được vượt quá 500 ký tự")]
        public string TenKhoHang { get; set; }

        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
        public string DiaChi { get; set; }

        [StringLength(500, ErrorMessage = "Người đại diện không được vượt quá 500 ký tự")]
        public string NguoiDaiDien { get; set; }

        public int? STT { get; set; }

        public bool IsKhoChinh { get; set; }
    }
}
