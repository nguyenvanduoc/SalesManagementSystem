using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface INKTongHopRepository
    {
        IEnumerable<NKTongHopVM> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        NKTongHopVM GetById(int id);
    }
}
