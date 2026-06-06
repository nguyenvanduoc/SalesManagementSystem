using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IKhachHangRepository
    {
        IEnumerable<KhachHangViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        IEnumerable<KhachHangViewModel> GetAll();
        NS_KhachHang GetById(int id);
        bool IsDuplicateCode(string maKhachHang, int currentId = 0);
        int Insert(NS_KhachHang entity);
        int Update(NS_KhachHang entity);
        int Delete(int id);
    }
}
