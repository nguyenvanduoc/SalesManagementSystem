using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DbConnectionFactory _db;

        public DashboardRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public DashboardDataViewModel GetDashboardData(DateTime? tuNgay, DateTime? denNgay)
        {
            var data = new DashboardDataViewModel();
            using (var conn = _db.CreateConnection())
            {
                // Xử lý Ngày
                if (!tuNgay.HasValue) tuNgay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                if (!denNgay.HasValue) denNgay = tuNgay.Value.AddMonths(1).AddDays(-1);
                
                DateTime dtTuNgay = tuNgay.Value.Date;
                DateTime dtDenNgay = denNgay.Value.Date.AddDays(1).AddSeconds(-1); // Cuối ngày
                
                // Kỳ trước (để tính % tăng giảm)
                TimeSpan span = dtDenNgay - dtTuNgay;
                DateTime dtTuNgayKyTruoc = dtTuNgay.AddDays(-span.TotalDays);
                DateTime dtDenNgayKyTruoc = dtDenNgay.AddDays(-span.TotalDays);

                var p = new DynamicParameters();
                p.Add("@TuNgay", dtTuNgay);
                p.Add("@DenNgay", dtDenNgay);
                p.Add("@TuNgayKyTruoc", dtTuNgayKyTruoc);
                p.Add("@DenNgayKyTruoc", dtDenNgayKyTruoc);

                using (var multi = conn.QueryMultiple("sp_Dashboard_GetData", p, commandType: System.Data.CommandType.StoredProcedure))
                {
                    // 1. Summary Metrics
                    var summary = multi.ReadFirstOrDefault();
                    if (summary != null)
                    {
                        data.Summary.DoanhThu = (decimal)(summary.DoanhThu ?? 0m);
                        data.Summary.DoanhThuKyTruoc = (decimal)(summary.DoanhThuKyTruoc ?? 0m);
                        data.Summary.CongNoKhachHang = (decimal)(summary.CongNoKhachHang ?? 0m);
                        data.Summary.TongTienHangNCC = (decimal)(summary.TongTienHangNCC ?? 0m);
                        data.Summary.DaThanhToanNCC = (decimal)(summary.DaThanhToanNCC ?? 0m);
                        data.Summary.CongNoNhaCungCap = (decimal)(summary.CongNoNhaCungCap ?? 0m);
                        data.Summary.TienHienCo = (decimal)(summary.TienHienCo ?? 0m);
                        data.Summary.LoiNhuan = (decimal)(summary.LoiNhuan ?? 0m);
                        data.Summary.LoiNhuanKyTruoc = (decimal)(summary.LoiNhuanKyTruoc ?? 0m);
                        
                        data.TonKho.TongGiaTriTonKho = (decimal)(summary.TongGiaTriTonKho ?? 0m);
                        data.TonKho.SoSanPhamSapHet = (int)(summary.SoSanPhamSapHet ?? 0);
                        data.TonKho.SoLuongSanPhamTon = (int)(summary.SoLuongSanPhamTon ?? 0);
                        data.TonKho.TongSoLuongTon = (decimal)(summary.TongSoLuongTon ?? 0m);
                        data.Summary.TienMat = (decimal)(summary.TienMat ?? 0m);
                        data.Summary.TongSoDuTaiKhoan = (decimal)(summary.TongSoDuTaiKhoan ?? 0m);
                        
                        data.ThuChi.TongThu = (decimal)(summary.TongThu ?? 0m);
                        data.ThuChi.TongChi = (decimal)(summary.TongChi ?? 0m);
                        
                        data.CanhBao.DonHangQuaHanGiao = (int)(summary.DonHangQuaHanGiao ?? 0);
                        data.CanhBao.PhieuNhapChuaThanhToan = (int)(summary.PhieuNhapChuaThanhToan ?? 0);
                        data.CanhBao.SanPhamSapHetHang = (int)(summary.SoSanPhamSapHet ?? 0);
                        data.CanhBao.ChungTuChuaGhi = (int)(summary.ChungTuChuaGhi ?? 0);
                        data.CanhBao.TaiKhoanAmQuy = 0; // Calculated after loading accounts
                    }

                    // 2. DoanhThuTheoThoiGian
                    data.DoanhThuTheoThoiGian = multi.Read<DashboardChartItem>().ToList();

                    // 3. GiaVonTheoThoiGian
                    data.GiaVonTheoThoiGian = multi.Read<DashboardChartItem>().ToList();

                    // 4. TrangThaiDonHang
                    data.TrangThaiDonHang = multi.Read<DashboardChartItem>().ToList();
                    data.TongSoDonHang = (int)data.TrangThaiDonHang.Sum(x => x.Value);

                    // 5. TopTonKho
                    data.TonKho.TopTonKho = multi.Read<DashboardChartItem>().ToList();

                    // 6. TopBanChay
                    data.TopBanChay = multi.Read<DashboardChartItem>().ToList();

                    // 7. ThuChiTheoNgay
                    data.ThuChi.ThuChiTheoNgay = multi.Read<DashboardChartItem>().ToList();

                    // 8. TaiKhoanThanhToan
                    data.TaiKhoanThanhToan = multi.Read<DashboardTaiKhoanViewModel>().ToList();
                    data.CanhBao.TaiKhoanAmQuy = data.TaiKhoanThanhToan.Count(x => x.SoDuHienTai < 0);

                    // 9. CongNoKhachHangQuaHan Summary
                    var cnkhSum = multi.ReadFirstOrDefault();
                    if (cnkhSum != null)
                    {
                        data.CongNoKhachHangQuaHan.TongNoQuaHan = (decimal)(cnkhSum.TongNoQuaHan ?? 0m);
                        data.CongNoKhachHangQuaHan.SoDoiTuongQuaHan = (int)(cnkhSum.SoDoiTuongQuaHan ?? 0);
                        data.CongNoKhachHangQuaHan.TenDoiTuongNoLonNhat = cnkhSum.TenDoiTuongNoLonNhat;
                        data.CongNoKhachHangQuaHan.NoLonNhat = (decimal)(cnkhSum.NoLonNhat ?? 0m);
                    }

                    // 10. CongNoKhachHangQuaHan List
                    data.CongNoKhachHangQuaHan.TopKhachHangQuaHan = multi.Read<CongNoKhachHangItem>().ToList();

                    // 11. CongNoNccQuaHan Summary
                    var cnnccSum = multi.ReadFirstOrDefault();
                    if (cnnccSum != null)
                    {
                        data.CongNoNccQuaHan.TongNoQuaHan = (decimal)(cnnccSum.TongNoQuaHan ?? 0m);
                        data.CongNoNccQuaHan.SoDoiTuongQuaHan = (int)(cnnccSum.SoDoiTuongQuaHan ?? 0);
                        data.CongNoNccQuaHan.TenDoiTuongNoLonNhat = cnnccSum.TenDoiTuongNoLonNhat;
                        data.CongNoNccQuaHan.NoLonNhat = (decimal)(cnnccSum.NoLonNhat ?? 0m);
                    }

                    // 12. CongNoNccQuaHan List
                    data.CongNoNccQuaHan.TopNccQuaHan = multi.Read<CongNoNccItem>().ToList();

                    // 13. HoatDongGanDay
                    data.HoatDongGanDay = multi.Read<DashboardHoatDongItem>().ToList();

                    // 14. TopKhachHang
                    data.TopKhachHang = multi.Read<DashboardTopDoiTuongItem>().ToList();

                    // 15. TopNhaCungCap
                    data.TopNhaCungCap = multi.Read<DashboardTopDoiTuongItem>().ToList();

                    // 16. DonHangGanDay (Recent Orders)
                    data.DonHangGanDay = multi.Read<DonDatHangViewModel>().ToList();

                    // 17. DonHangDangDiDuong
                    data.DonHangDangDiDuong = multi.Read<DonDatHangViewModel>().ToList();
                }
            }

            return data;
        }
    }
}
