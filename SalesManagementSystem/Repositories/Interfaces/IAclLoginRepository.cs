using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IAclLoginRepository
    {
        IEnumerable<AclLoginVM> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        AclLogin GetById(int id);
        bool IsDuplicateUsername(string username, int id = 0);
        int Insert(AclLogin login);
        void Update(AclLogin login);
        void Delete(int id);
        IEnumerable<NhanVien> GetEmployeesWithoutAccount();
        NhanVien GetEmployeeById(int id);
        AclLogin GetByEmployeeId(int empId);
        IEnumerable<AclLoginVM> GetManagers();
        AclLogin Login(string userName, string passWord);
    }
}
