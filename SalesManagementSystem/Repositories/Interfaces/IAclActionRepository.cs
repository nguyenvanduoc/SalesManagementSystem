using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IAclActionRepository
    {
        IEnumerable<AclAction> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        IEnumerable<AclAction> GetAll();
        AclAction GetById(int id);
        int Insert(AclAction entity);
        void Update(AclAction entity);
        void Delete(int id);
    }
}
