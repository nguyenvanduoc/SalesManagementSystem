using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesManagementSystem.Models.Entities
{
    [Table("DM_KhoHang")]
    public class DM_KhoHang
    {
        [Key]
        public int ID { get; set; }

        [StringLength(500)]
        public string TenKhoHang { get; set; }

        [StringLength(100)]
        public string MaKhoHang { get; set; }

        [StringLength(500)]
        public string DiaChi { get; set; }

        [StringLength(500)]
        public string NguoiDaiDien { get; set; }

        public int? STT { get; set; }

        public DateTime? NgayTao { get; set; }

        public int? NguoiTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        public int? NguoiCapNhat { get; set; }
    }
}
