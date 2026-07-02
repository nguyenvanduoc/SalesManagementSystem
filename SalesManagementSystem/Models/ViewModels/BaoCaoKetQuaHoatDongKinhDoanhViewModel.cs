using System;
using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public class BaoCaoKetQuaHoatDongKinhDoanhFilterModel
    {
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public int? IDKho { get; set; }
        public int? IDSanPham { get; set; }
        public string DonViTinh { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
    }

    public class BaoCaoKetQuaHoatDongKinhDoanhRowModel
    {
        public int? STT { get; set; }
        public int IDSanPham { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string DonViTinh { get; set; }
        public int? IDSanPhamCha { get; set; }

        public decimal SoLuongDoanhThu { get; set; }
        public decimal ThanhTienDoanhThu { get; set; }

        public decimal SoLuongGiaVon { get; set; }
        public decimal ThanhTienGiaVon { get; set; }

        public decimal ChiPhiVanChuyen { get; set; }
        public decimal ChiPhiBaoBi { get; set; }

        public decimal LoiNhuanGop { get; set; }
        public decimal LoiNhuanThuan { get; set; }
        public decimal TySuatLoiNhuan { get; set; }
        
        public bool IsGroup { get; set; }
    }

    public class BaoCaoKetQuaHoatDongKinhDoanhViewModel
    {
        public BaoCaoKetQuaHoatDongKinhDoanhFilterModel Filter { get; set; }
        public List<BaoCaoKetQuaHoatDongKinhDoanhRowModel> Data { get; set; }

        // Totals for cards
        public decimal TotalDoanhThu { get; set; }
        public decimal TotalGiaVon { get; set; }
        public decimal TotalLoiNhuanGop { get; set; }
        public decimal TotalChiPhiVanChuyen { get; set; }
        public decimal TotalChiPhiBaoBi { get; set; }
        public decimal TotalLoiNhuanThuan { get; set; }
        public decimal TotalTySuatLN { get; set; }

        public BaoCaoKetQuaHoatDongKinhDoanhViewModel()
        {
            Filter = new BaoCaoKetQuaHoatDongKinhDoanhFilterModel();
            Data = new List<BaoCaoKetQuaHoatDongKinhDoanhRowModel>();
        }
    }
}
