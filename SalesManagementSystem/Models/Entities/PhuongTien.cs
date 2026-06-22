using System;
using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.Entities
{
    public class PhuongTien
    {
        public int ID { get; set; } // Maps to IDPhuongTien in SQL

        [Required(ErrorMessage = "Mã phương tiện là bắt buộc")]
        [StringLength(50)]
        public string MaPhuongTien { get; set; }

        [Required(ErrorMessage = "Tên phương tiện là bắt buộc")]
        [StringLength(200)]
        public string TenPhuongTien { get; set; }

        public int? STT { get; set; }
        
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
    }
}
