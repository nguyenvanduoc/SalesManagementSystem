using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IPhongBanRepository
    {
        IEnumerable<PhongBan> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        IEnumerable<PhongBan> GetAll();
        PhongBan GetById(int id);
        bool IsDuplicateCode(string maPhongBan, int currentId = 0);
        int Insert(PhongBan entity);
        int Update(PhongBan entity);
        int Delete(int id);
    }
}
