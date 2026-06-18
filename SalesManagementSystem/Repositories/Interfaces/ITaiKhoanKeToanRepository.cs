using SalesManagementSystem.Models.Entities;
using System.Collections.Generic;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface ITaiKhoanKeToanRepository
    {
        IEnumerable<KT_TaiKhoanKeToan> GetActive();
    }
}
