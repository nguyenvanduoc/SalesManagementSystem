using System;
using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IPhieuThuKhachHangRepository
    {
        IEnumerable<PhieuThuKhachHangListViewModel> GetList(
            string tuNgay, 
            string denNgay, 
            string soChungTu, 
            int? idKhachHang, 
            int? trangThaiCongNo
        );
        PhieuThuKhachHangViewModel GetByID(int id);
        int Save(PhieuThuKhachHangViewModel model, int userId);
        void GhiSo(int id, int userId);
        void Huy(int id, int userId, string lyDo);
        void Delete(int id, int userId);
        string GenerateSoPhieuThu();
        IEnumerable<dynamic> GetChungTuBanHangDropdown();
        dynamic GetCongNoChungTuByID(int id);
        IEnumerable<dynamic> GetTaiKhoanThanhToanDropdown();
        IEnumerable<dynamic> GetNhanSuDropdown();

        // New Redesign Methods
        IEnumerable<dynamic> GetHistoryByChungTuID(int idChungTuBanHang);
        decimal GetCreditInfo(int idKhachHang);
        IEnumerable<dynamic> GetRecentActivities(int idChungTuBanHang);

        IEnumerable<PhieuThuKhachHangFile> File_GetList(int idChungTuBanHang);
        PhieuThuKhachHangFile File_GetByID(int id);
        void File_Save(PhieuThuKhachHangFile model, int nguoiThaoTac);
        void File_Delete(int id, int nguoiThaoTac);
    }
}
