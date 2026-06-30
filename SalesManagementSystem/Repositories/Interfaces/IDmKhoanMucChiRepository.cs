using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IDmKhoanMucChiRepository
    {
        IEnumerable<DmKhoanMucChiViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        DM_KhoanMucChi GetById(int id);
        int Insert(DM_KhoanMucChi entity);
        void Update(DM_KhoanMucChi entity);
        void Delete(int id);
        bool CheckDuplicateCode(string code, int excludeId = 0);
    }
}
