using System;
using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IPhieuNhapKhoRepository
    {
        IEnumerable<PhieuNhapKhoListViewModel> GetPaged(
            int page, int pageSize,
            string tuNgay, string denNgay,
            string soChungTu, int? idKho, int? idNhaCungCap, 
            int? trangThai, string tenNguoiNhan,
            out int totalRecords);

        KHO_PhieuNhap GetByID(int id);
        List<PhieuNhapKhoChiTietViewModel> GetChiTiet(int idPhieuNhap);
        int Save(PhieuNhapKhoViewModel model, int userId);
        void UpdateMaster(PhieuNhapKhoViewModel model, int userId);
        void GhiSo(int id, int userId);
        void HuyPhieu(int id, string lyDoHuy, int userId);
        void Delete(int id, int userId);
        string GenerateSoChungTu();

        IEnumerable<dynamic> GetKhoForDropdown(string keyword);
        IEnumerable<dynamic> GetNhaCungCapForDropdown(string keyword);
        IEnumerable<dynamic> GetNhanSuForDropdown(string keyword);
        IEnumerable<dynamic> GetSanPhamForDropdown(string keyword);
        IEnumerable<dynamic> GetPhuongTienForDropdown(string keyword);
        IEnumerable<dynamic> GetLoaiNhapKhoForDropdown();
        IEnumerable<dynamic> GetKhachHangForDropdown(string keyword);
        IEnumerable<dynamic> CheckTonKhoChuyenKho(int idKhoNguon, string chiTietsJson);
    }
}
