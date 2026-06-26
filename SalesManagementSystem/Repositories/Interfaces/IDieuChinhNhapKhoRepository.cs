using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IDieuChinhNhapKhoRepository
    {
        IEnumerable<DieuChinhNhapKhoListViewModel> GetPaged(
            int page, int pageSize,
            string tuNgay, string denNgay,
            int? idLoaiNhap, int? idKho,
            int? idNhaCungCap, int? idKhachHang,
            string soChungTu, bool chiDonDieuChinh,
            out int totalRecords);

        IEnumerable<DieuChinhNhapKhoHistoryViewModel> GetAdjustHistory(int idPhieuNhap);

        void SaveAdjustment(DieuChinhNhapKhoPostModel model, int userId);
    }
}
