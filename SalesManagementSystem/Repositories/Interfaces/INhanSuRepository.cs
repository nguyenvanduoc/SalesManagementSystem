using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface INhanSuRepository
    {
        IEnumerable<NhanSu> GetAll();
        IEnumerable<NhanSu> GetAllWithChucVu();
        IEnumerable<NhanSu> GetPaged(int page, int pageSize, string keyword, bool? gender, out int totalRecords);
        NhanSu GetById(int id);
        bool IsDuplicateCode(string code, int id = 0);
        int Insert(NhanSu employee);
        void Update(NhanSu employee);
        void Delete(int id);
    }
}
