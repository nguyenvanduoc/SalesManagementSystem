using SalesManagementSystem.Models.ViewModels;
using System.Collections.Generic;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IPhieuXuatKhoRepository
    {
        List<PhieuXuatKhoListViewModel> GetList(int page, int pageSize, string tuNgay, string denNgay, string soChungTu, int? idKho, int? trangThai, int? idNhanSuNhan, out int totalRecords);
        PhieuXuatKhoViewModel GetById(int id);
        string GenerateSoChungTu();
        int Insert(PhieuXuatKhoViewModel model, int userId);
        void UpdateStatus(int id, int status, int userId);
        void Cancel(int id, int userId, string reason);
    }
}
