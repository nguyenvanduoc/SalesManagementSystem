using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface INhanVienRepository
    {
        IEnumerable<NhanVien> GetAll();
        IEnumerable<NhanVien> GetPaged(int page, int pageSize, string keyword, bool? gender, out int totalRecords);
        NhanVien GetById(int id);
        bool IsDuplicateCode(string code, int id = 0);
        int Insert(NhanVien employee);
        void Update(NhanVien employee);
        void Delete(int id);
    }
}
