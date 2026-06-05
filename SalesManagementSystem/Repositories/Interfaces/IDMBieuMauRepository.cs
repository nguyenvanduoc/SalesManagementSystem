using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IDMBieuMauRepository
    {
        IEnumerable<DMBieuMauViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        DM_BieuMau GetById(int id);
        DM_BieuMau GetByMa(string maBieuMau);
        bool CheckDuplicateCode(string maBieuMau, int currentId = 0);
        int Insert(DM_BieuMau bieuMau);
        void Update(DM_BieuMau bieuMau);
        void Delete(int id);
    }
}
