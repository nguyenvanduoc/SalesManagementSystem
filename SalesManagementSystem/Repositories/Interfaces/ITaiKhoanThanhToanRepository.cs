using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface ITaiKhoanThanhToanRepository
    {
        IEnumerable<TaiKhoanThanhToanListViewModel> GetList(int page, int pageSize, string keyword, int? isHoatDong, out int totalRecords);
        TaiKhoanThanhToanViewModel GetByID(int id);
        int Save(TaiKhoanThanhToanViewModel model, int userId);
        void Delete(int id);
        bool IsDuplicateCode(string code, int currentId = 0);
    }
}
