using System;
using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.Entities
{
    public class PhongBan
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Mã phòng ban là bắt buộc")]
        [StringLength(50)]
        public string MaPhongBan { get; set; }

        [Required(ErrorMessage = "Tên phòng ban là bắt buộc")]
        [StringLength(255)]
        public string TenPhongBan { get; set; }

        public int? STT { get; set; }
        
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
    }
}
