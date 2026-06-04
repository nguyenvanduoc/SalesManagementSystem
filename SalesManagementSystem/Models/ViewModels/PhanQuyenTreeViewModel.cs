using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhanQuyenTreeViewModel
    {
        public int ID { get; set; }
        public int? IDThamChieu { get; set; }
        public string TenNhanVien { get; set; } // Sẽ hiển thị dạng: MaNhanVien - HoDem Ten
        public string TenDangNhap { get; set; }
        public List<PhanQuyenTreeViewModel> Children { get; set; }

        public PhanQuyenTreeViewModel()
        {
            Children = new List<PhanQuyenTreeViewModel>();
        }
    }
}
