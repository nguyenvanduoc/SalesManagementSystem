using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IAclManHinhRepository
    {
        IEnumerable<AclManHinh> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        IEnumerable<AclManHinh> GetAll();
        AclManHinh GetById(int id);
        int Insert(AclManHinh entity);
        void Update(AclManHinh entity);
        void Delete(int id);
    }
}
