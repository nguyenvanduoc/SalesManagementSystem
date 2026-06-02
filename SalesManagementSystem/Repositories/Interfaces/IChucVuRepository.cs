using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IChucVuRepository
    {
        IEnumerable<ChucVu> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        IEnumerable<ChucVu> GetAll();
        ChucVu GetById(int id);
        bool IsDuplicateCode(string code, int id = 0);
        int Insert(ChucVu entity);
        void Update(ChucVu entity);
        void Delete(int id);
    }
}
