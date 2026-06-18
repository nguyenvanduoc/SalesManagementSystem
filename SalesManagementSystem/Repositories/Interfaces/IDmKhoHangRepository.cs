using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IDmKhoHangRepository
    {
        IEnumerable<DmKhoHangViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        DM_KhoHang GetById(int id);
        int Insert(DM_KhoHang kh);
        void Update(DM_KhoHang kh);
        void Delete(int id);
        bool CheckDuplicateCode(string maKhoHang, int excludeId = 0);
    }
}
