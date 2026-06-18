using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface INhaCungCapRepository
    {
        IEnumerable<NhaCungCapViewModel> GetPaged(
            int page, int pageSize,
            string ma, string ten, string dt, string email,
            out int totalRecords);

        DM_NhaCungCap GetById(int id);
        
        int Save(DM_NhaCungCap ncc);
        
        bool Delete(int id, out string message);
        
        bool CheckDuplicate(string code, int excludeId = 0);

        IEnumerable<dynamic> GetForDropdown(string keyword);
    }
}
