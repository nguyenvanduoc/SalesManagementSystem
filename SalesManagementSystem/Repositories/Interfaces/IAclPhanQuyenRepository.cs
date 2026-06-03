using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IAclPhanQuyenRepository
    {
        IEnumerable<PhanQuyenTreeVM> GetTreeLogin();
        IEnumerable<PhanQuyenMatrixVM> GetMatrixQuyen(int idLogin);
        bool SaveQuyen(int idLogin, List<int> checkedActionIds, int currentUser);
        List<int> GetParentActionIds(int idLogin);
    }
}
