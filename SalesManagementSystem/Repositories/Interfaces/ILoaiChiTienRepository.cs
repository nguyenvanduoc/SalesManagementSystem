using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface ILoaiChiTienRepository
    {
        IEnumerable<LoaiChiTienViewModel> GetAllActive();
    }
}
