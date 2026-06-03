namespace SalesManagementSystem.Models.Entities
{
    /// <summary>
    /// Hành động/chức năng của từng màn hình.
    /// Ánh xạ bảng ACL_Action.
    /// </summary>
    public class AclAction
    {
        public int ID { get; set; }
        public int IDManHinh { get; set; }

        /// <summary>Tên action MVC, VD: "Index", "Create", "Edit"</summary>
        public string TenAction { get; set; }

        /// <summary>Tên controller MVC, VD: "Product", "Order", "Inventory"</summary>
        public string TenController { get; set; }

        /// <summary>Mã tổ chức năng, VD: "Xem danh sách sản phẩm"</summary>
        public string GhiChu { get; set; }

        /// <summary>1=Xem, 2=Thêm, 3=Sửa, 4=Xóa, 5=Tùy chọn</summary>
        public int LoaiPhanQuyen { get; set; }
    }
}
