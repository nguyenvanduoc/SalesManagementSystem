using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesManagementSystem.Models.Entities
{
    [Table("DM_KhoanMucChi")]
    public class DM_KhoanMucChi
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string MaKhoanMuc { get; set; }

        [Required]
        [StringLength(255)]
        public string TenKhoanMuc { get; set; }

        public bool IsHoatDong { get; set; }

        public DateTime? NgayTao { get; set; }

        public int? NguoiTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        public int? NguoiCapNhat { get; set; }
    }
}
