using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IDmSanPhamRepository
    {
        List<DmSanPhamViewModel> GetPaged(int pageIndex, int pageSize, string keyword, out int totalRecords);
        DM_SanPham GetById(int id);
        bool CheckDuplicateCode(string maSanPham, int id = 0);
        int Insert(DM_SanPham entity);
        bool Update(DM_SanPham entity);
        bool Delete(int id);
    }
}
