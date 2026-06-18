using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.ViewModels
{
    public class DmSanPhamCreateEditViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(500, ErrorMessage = "Tên sản phẩm không được vượt quá 500 ký tự")]
        public string TenSanPham { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã sản phẩm")]
        [StringLength(100, ErrorMessage = "Mã sản phẩm không được vượt quá 100 ký tự")]
        public string MaSanPham { get; set; }

        [StringLength(100, ErrorMessage = "Đơn vị tính không được vượt quá 100 ký tự")]
        public string DVT { get; set; }

        public int? STT { get; set; }
    }
}
