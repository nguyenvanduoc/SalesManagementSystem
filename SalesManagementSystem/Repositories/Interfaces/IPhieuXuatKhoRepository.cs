using SalesManagementSystem.Models.ViewModels;
using System.Collections.Generic;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IPhieuXuatKhoRepository
    {
        List<PhieuXuatKhoListViewModel> GetList(int page, int pageSize, string tuNgay, string denNgay, string soChungTu, int? idKho, int? trangThai, int? idNhanSuNhan, int? idSanPham, int? idNhaCungCap, string tenNguoiGiao, int? idPhuongTien, string tenNguoiNhan, out int totalRecords);
        PhieuXuatKhoViewModel GetById(int id);
        List<PhieuXuatKhoChiTietViewModel> GetChiTiet(int idPhieuXuat);
        string GenerateSoChungTu();
        int Save(PhieuXuatKhoViewModel model, int userId);
        void GhiSo(int id, int userId);
        int Insert(PhieuXuatKhoViewModel model, int userId);
        void UpdateStatus(int id, int status, int userId);
        void Cancel(int id, int userId, string reason);
    }
}
