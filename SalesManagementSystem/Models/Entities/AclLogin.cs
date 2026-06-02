using System;
using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.Entities
{
    public class AclLogin
    {
        public int ID { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn nhân viên")]
        public int IDNhanVien { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string TenDangNhap { get; set; }
        
        public string MatKhau { get; set; }

        public string HoDem { get; set; }
        public string Ten { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int? IDThamChieu { get; set; }
        
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
        public DateTime? NgayXoa { get; set; }
        public int? NguoiXoa { get; set; }
    }
}
