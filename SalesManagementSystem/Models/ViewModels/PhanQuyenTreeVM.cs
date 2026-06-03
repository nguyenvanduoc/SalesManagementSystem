using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhanQuyenTreeVM
    {
        public int ID { get; set; }
        public int? IDThamChieu { get; set; }
        public string TenNhanVien { get; set; } // Sẽ hiển thị dạng: MaNhanVien - HoDem Ten
        public string TenDangNhap { get; set; }
        public List<PhanQuyenTreeVM> Children { get; set; }

        public PhanQuyenTreeVM()
        {
            Children = new List<PhanQuyenTreeVM>();
        }
    }
}
