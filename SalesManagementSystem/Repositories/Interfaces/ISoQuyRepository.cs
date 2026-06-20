using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface ISoQuyRepository
    {
        IEnumerable<SoQuyViewModel> GetList(
            string tuNgay,
            string denNgay,
            int? idTaiKhoanThanhToan);

        IEnumerable<TaiKhoanSummaryViewModel> GetTaiKhoanSummary(
            string tuNgay,
            string denNgay,
            int? idTaiKhoanThanhToan);

        decimal GetOpeningBalance(
            string tuNgay,
            int idTaiKhoanThanhToan);

        IEnumerable<GiaoDichChiTietViewModel> GetGiaoDichChiTiet(
            string tuNgay,
            string denNgay,
            int idTaiKhoanThanhToan);
    }
}
