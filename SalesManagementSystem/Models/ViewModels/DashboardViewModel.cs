using System;
using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public class DashboardFilterViewModel
    {
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
    }

    public class DashboardSummaryViewModel
    {
        // Khối 1
        public decimal DoanhThu { get; set; }
        public decimal DoanhThuKyTruoc { get; set; }
        public decimal TyLeTangGiamDoanhThu 
        {
            get 
            {
                if (DoanhThuKyTruoc == 0) return DoanhThu > 0 ? 100 : 0;
                return Math.Round((DoanhThu - DoanhThuKyTruoc) / DoanhThuKyTruoc * 100, 1);
            }
        }
        
        public decimal CongNoKhachHang { get; set; }
        public decimal CongNoNhaCungCap { get; set; }
        public decimal TienHienCo { get; set; }
        public decimal LoiNhuan { get; set; }
        public decimal LoiNhuanKyTruoc { get; set; }
        public decimal TyLeTangGiamLoiNhuan 
        {
            get 
            {
                if (LoiNhuanKyTruoc == 0) return LoiNhuan > 0 ? 100 : 0;
                return Math.Round((LoiNhuan - LoiNhuanKyTruoc) / LoiNhuanKyTruoc * 100, 1);
            }
        }
        // Số dư tiền
        public decimal TienMat { get; set; }
        public decimal TongSoDuTaiKhoan { get; set; }
    }

    public class DashboardChartItem
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }

    public class DashboardTonKhoViewModel
    {
        // Khối 4
        public decimal TongGiaTriTonKho { get; set; }
        public int SoSanPhamSapHet { get; set; }
        public int SoLuongSanPhamTon { get; set; }
        public decimal TongSoLuongTon { get; set; }  // Tổng số lượng tồn (thùng)
        public List<DashboardChartItem> TopTonKho { get; set; } = new List<DashboardChartItem>();
    }

    public class DashboardThuChiViewModel
    {
        // Khối 5
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal DongTienThuan => TongThu - TongChi;
        public List<DashboardChartItem> ThuChiTheoNgay { get; set; } = new List<DashboardChartItem>();
    }

    public class DashboardTaiKhoanViewModel
    {
        // Khối 6
        public int ID { get; set; }
        public string TenTaiKhoan { get; set; }
        public string NganHang { get; set; }
        public string SoTaiKhoan { get; set; }
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal SoDuHienTai => TongThu - TongChi;
    }

    public class DashboardCongNoQuaHanViewModel
    {
        // Khối 7 & 8
        public decimal TongNoQuaHan { get; set; }
        public int SoDoiTuongQuaHan { get; set; }
        public decimal NoLonNhat { get; set; }
        public string TenDoiTuongNoLonNhat { get; set; }
        
        // Grid KH
        public List<CongNoKhachHangItem> TopKhachHangQuaHan { get; set; } = new List<CongNoKhachHangItem>();
        // Grid NCC
        public List<CongNoNccItem> TopNccQuaHan { get; set; } = new List<CongNoNccItem>();
    }

    public class CongNoKhachHangItem
    {
        public string KhachHang { get; set; }
        public string SoChungTu { get; set; }
        public DateTime? NgayChungTu { get; set; }
        public DateTime? HanThanhToan { get; set; }
        public int SoNgayQuaHan { get; set; }
        public decimal TongTien { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo => TongTien - DaThanhToan;
    }

    public class CongNoNccItem
    {
        public string NhaCungCap { get; set; }
        public string SoPhieuNhap { get; set; }
        public DateTime? NgayNhap { get; set; }
        public DateTime? HanThanhToan { get; set; }
        public int SoNgayQuaHan { get; set; }
        public decimal TongTien { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConLai => TongTien - DaThanhToan;
    }

    public class DashboardCanhBaoViewModel
    {
        // Khối 9
        public int DonHangQuaHanGiao { get; set; }
        public int PhieuNhapChuaThanhToan { get; set; }
        public int TaiKhoanAmQuy { get; set; }
        public int SanPhamSapHetHang { get; set; }
        public int ChungTuChuaGhi { get; set; }
    }

    public class DashboardHoatDongItem
    {
        // Khối 10
        public DateTime ThoiGian { get; set; }
        public string NguoiThucHien { get; set; }
        public string NoiDung { get; set; }
        public string LoaiHoatDong { get; set; } // TaoDonHang, GhiChungTu, NhapKho, ThuTien, ChiTien...
    }

    public class DashboardTopDoiTuongItem
    {
        // Khối 11 & 12
        public string TenDoiTuong { get; set; }
        public decimal DoanhThuHoacGiaTriNhap { get; set; }
        public decimal CongNo { get; set; }
    }

    public class DashboardDataViewModel
    {
        public DashboardSummaryViewModel Summary { get; set; } = new DashboardSummaryViewModel();
        
        // Khối 2: Doanh thu theo thời gian
        public List<DashboardChartItem> DoanhThuTheoThoiGian { get; set; } = new List<DashboardChartItem>();
        
        // Khối 3: Trạng thái đơn hàng
        public List<DashboardChartItem> TrangThaiDonHang { get; set; } = new List<DashboardChartItem>();
        public int TongSoDonHang { get; set; }

        public DashboardTonKhoViewModel TonKho { get; set; } = new DashboardTonKhoViewModel();
        public DashboardThuChiViewModel ThuChi { get; set; } = new DashboardThuChiViewModel();
        public List<DashboardTaiKhoanViewModel> TaiKhoanThanhToan { get; set; } = new List<DashboardTaiKhoanViewModel>();
        
        public DashboardCongNoQuaHanViewModel CongNoKhachHangQuaHan { get; set; } = new DashboardCongNoQuaHanViewModel();
        public DashboardCongNoQuaHanViewModel CongNoNccQuaHan { get; set; } = new DashboardCongNoQuaHanViewModel();
        
        public DashboardCanhBaoViewModel CanhBao { get; set; } = new DashboardCanhBaoViewModel();
        public List<DashboardHoatDongItem> HoatDongGanDay { get; set; } = new List<DashboardHoatDongItem>();
        
        public List<DashboardTopDoiTuongItem> TopKhachHang { get; set; } = new List<DashboardTopDoiTuongItem>();
        public List<DashboardTopDoiTuongItem> TopNhaCungCap { get; set; } = new List<DashboardTopDoiTuongItem>();

        // Mới: Biểu đồ bán hàng bổ sung
        public List<DashboardChartItem> TopBanChay { get; set; } = new List<DashboardChartItem>();
        public List<DashboardChartItem> GiaVonTheoThoiGian { get; set; } = new List<DashboardChartItem>();
        public List<DonDatHangViewModel> DonHangGanDay { get; set; } = new List<DonDatHangViewModel>();
    }
}
