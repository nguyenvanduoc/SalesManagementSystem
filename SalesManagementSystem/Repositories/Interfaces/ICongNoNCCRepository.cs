using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface ICongNoNCCRepository
    {
        IEnumerable<CongNoNCCViewModel> GetList(
            string tuNgay,
            string denNgay,
            int? idNhaCungCap,
            int? trangThaiCongNo);
    }
}
