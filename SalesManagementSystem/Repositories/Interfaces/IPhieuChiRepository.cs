using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IPhieuChiRepository
    {
        IEnumerable<PhieuChiListViewModel> GetList(
            string tuNgay,
            string denNgay,
            string soPhieuChi,
            int? idNhaCungCap,
            int? idKhoanMucChi,
            int? trangThai,
            string nguoiNhanTien = null,
            int? idTaiKhoanThanhToan = null);

        PhieuChiViewModel GetByID(int id);
        int Save(PhieuChiViewModel model, int userId);
        void GhiSo(int id, int userId);
        void Huy(int id, int userId, string lyDo);
        void Delete(int id, int userId);
        string GenerateSoPhieuChi();

        IEnumerable<dynamic> GetKhoanMucDropdown();
        IEnumerable<dynamic> GetTaiKhoanDropdown();
        IEnumerable<dynamic> GetNhaCungCapDropdown();
        IEnumerable<dynamic> GetPhieuNhapDropdown(int? idNhaCungCap, int? currentPhieuNhapId = null);
        IEnumerable<dynamic> GetNhanSuDropdown();

        // New methods for multiple allocations
        IEnumerable<dynamic> GetPhieuNhapCongNo(int idNhaCungCap);
        decimal GetTienTraTruocNhaCungCap(int idNhaCungCap);
        IEnumerable<PhieuChiChiTietViewModel> GetChiTiet(int idPhieuChi);
        void DieuChinhPhanBo(PhieuChiViewModel model, List<PhieuChiChiTietViewModel> newChiTiets, int userId);

        dynamic GetPhieuNhapDetail(int idPhieuNhap);
        IEnumerable<dynamic> GetLichSuChiTienPhieuNhap(int idPhieuNhap);
        PhieuChiDashboardViewModel GetDashboardData(
            string tuNgay,
            string denNgay,
            string soPhieuChi,
            int? idNhaCungCap,
            int? idKhoanMucChi,
            int? trangThai,
            string nguoiNhanTien = null,
            int? idTaiKhoanThanhToan = null);

        IEnumerable<PhieuChiFile> File_GetList(int idPhieuChi);
        PhieuChiFile File_GetByID(int id);
        void File_Save(PhieuChiFile model, int nguoiThaoTac);
        void File_Delete(int id, int nguoiThaoTac);
    }
}
