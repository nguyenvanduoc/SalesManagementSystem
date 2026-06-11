using System;
using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IPhieuXuatKhoRepository
    {
        IEnumerable<PhieuXuatKhoListViewModel> GetPaged(
            int page, int pageSize,
            string tuNgay, string denNgay,
            string soChungTu, int? idKho,
            int? trangThai, int? idNhanSuNhan,
            out int totalRecords);

        PhieuXuatKhoViewModel GetByID(int id);
        List<PhieuXuatKhoChiTietViewModel> GetChiTiet(int idPhieuXuat);
        int Save(PhieuXuatKhoViewModel model, int userId);
        void GhiSo(int id, int userId);
        void HuyPhieu(int id, string lyDoHuy, int userId);
        void Delete(int id, int userId);
        string GenerateSoChungTu();

        IEnumerable<dynamic> GetKhoForDropdown(string keyword);
        IEnumerable<dynamic> GetNhanSuForDropdown(string keyword);
        IEnumerable<dynamic> GetSanPhamForDropdown(string keyword);
    }
}
