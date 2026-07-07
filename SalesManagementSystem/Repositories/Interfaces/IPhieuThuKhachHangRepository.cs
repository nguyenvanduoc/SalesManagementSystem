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
            string soPhieuThu, 
            int? idKhachHang, 
            int? trangThai,
            string nguoiNopTien,
            int? idTaiKhoanThanhToan
        );
        dynamic GetDashboardData(
            string tuNgay, string denNgay, string soPhieuThu, 
            int? idKhachHang, int? trangThai, string nguoiNopTien, int? idTaiKhoanThanhToan
        );
        PhieuThuKhachHangViewModel GetByID(int id);
        int Save(PhieuThuKhachHangViewModel model, int userId);
        void DieuChinhPhanBo(PhieuThuKhachHangViewModel model, List<PhieuThuKhachHangChiTietViewModel> newChiTiets, int nguoiCapNhat);
        void GhiSo(int id, int userId);
        void Huy(int id, int userId, string lyDo);
        void Delete(int id, int userId);
        string GenerateSoPhieuThu();
        
        IEnumerable<dynamic> GetChungTuBanHangDropdown(int? idKhachHang = null);
        IEnumerable<dynamic> GetKhachHangDropdown();
        IEnumerable<dynamic> GetTaiKhoanThanhToanDropdown();
        IEnumerable<dynamic> GetNhanSuDropdown();

        IEnumerable<dynamic> GetChungTuBanHangCongNo(int idKhachHang);
        decimal GetTienTraTruocKhachHang(int idKhachHang);

        IEnumerable<PhieuThuKhachHangFile> File_GetList(int idPhieuThu);
        PhieuThuKhachHangFile File_GetByID(int id);
        void File_Save(PhieuThuKhachHangFile model, int nguoiThaoTac);
        void File_Delete(int id, int nguoiThaoTac);
    }
}
