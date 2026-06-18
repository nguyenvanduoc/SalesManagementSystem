using System;
namespace SalesManagementSystem.Models.Entities
{
    public class AclPhanQuyen
    {
        public int IDLogin { get; set; }
        public int IDAction { get; set; }
        public int IsChoPhep { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
    }
}
