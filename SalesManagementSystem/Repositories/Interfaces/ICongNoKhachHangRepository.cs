using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface ICongNoKhachHangRepository
    {
        IEnumerable<CongNoKhachHangViewModel> GetList(
            string tuNgay,
            string denNgay,
            int? idKhachHang,
            int? trangThaiCongNo
        );
        
        IEnumerable<CongNoKhachHangSP02ViewModel> GetExportSP02(
            string tuNgay,
            string denNgay
        );

        CongNoKhachHangDashboardViewModel GetDashboard(
            string tuNgay,
            string denNgay,
            int? idKhachHang
        );

        IEnumerable<CongNoKhachHangDetailViewModel> GetDetail(
            int idKhachHang,
            string tuNgay,
            string denNgay
        );

        IEnumerable<dynamic> GetHistory(int idChungTuBanHang);

        IEnumerable<dynamic> GetKhachHangDropdown();
    }
}
