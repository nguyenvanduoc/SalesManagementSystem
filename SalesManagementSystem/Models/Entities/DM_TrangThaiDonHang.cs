using System;
using System.Collections.Generic;

namespace SalesManagementSystem.Models.Entities
{
    public class DM_TrangThaiDonHang
    {
        public int ID { get; set; }
        public string TenTrangThai { get; set; }
        public int ThuTuHienThi { get; set; }
        public bool KichHoat { get; set; }
    }
}
