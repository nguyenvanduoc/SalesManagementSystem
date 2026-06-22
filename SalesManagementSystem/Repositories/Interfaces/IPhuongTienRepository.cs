using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IPhuongTienRepository
    {
        IEnumerable<PhuongTien> GetAll();
        IEnumerable<PhuongTien> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        PhuongTien GetById(int id);
        bool IsDuplicateCode(string maPhuongTien, int currentId = 0);
        int Insert(PhuongTien entity);
        int Update(PhuongTien entity);
        int Delete(int id);
    }
}
