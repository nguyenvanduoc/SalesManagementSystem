namespace SalesManagementSystem.Models.Entities
{
    /// <summary>
    /// Màn hình/module trong hệ thống ACL.
    /// Ánh xạ bảng ACL_ManHinh.
    /// </summary>
    public class AclManHinh
    {
        public int ID { get; set; }
        public string TenManHinh { get; set; }

        /// <summary>Tên nhóm cha trên sidebar, VD: "BÁN HÀNG", "KHO BÃI"</summary>
        public string NhomChaManHinh { get; set; }

        /// <summary>1 = đang sử dụng, 0 = ẩn khỏi menu</summary>
        public int IsSuDung { get; set; }

        /// <summary>ID tham chiếu (dự phòng liên kết cha-con nếu cần)</summary>
        public int? IDThamChieu { get; set; }

        /// <summary>Số thứ tự hiển thị</summary>
        public int? STT { get; set; }
    }
}
