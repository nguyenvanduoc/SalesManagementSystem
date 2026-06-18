using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IAclLoginRepository
    {
        IEnumerable<AclLoginViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        AclLogin GetById(int id);
        bool IsDuplicateUsername(string username, int id = 0);
        int Insert(AclLogin login);
        void Update(AclLogin login);
        void Delete(int id, int userId);
        void TransferManager(int newManagerId, int updateBy);
        IEnumerable<NhanSu> GetEmployeesWithoutAccount();
        NhanSu GetEmployeeById(int id);
        AclLogin GetByEmployeeId(int empId);
        IEnumerable<AclLoginViewModel> GetManagers();
        AclLogin Login(string userName, string passWord);
    }
}
