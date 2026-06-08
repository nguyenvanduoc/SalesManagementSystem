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

        public int? STT { get; set; }
    }
}
