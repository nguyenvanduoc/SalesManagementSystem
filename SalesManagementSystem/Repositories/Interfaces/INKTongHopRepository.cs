using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface INKTongHopRepository
    {
        IEnumerable<NKTongHopViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        NKTongHopViewModel GetById(int id);
    }
}
