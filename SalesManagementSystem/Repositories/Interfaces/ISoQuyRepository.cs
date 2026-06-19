using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface ISoQuyRepository
    {
        IEnumerable<SoQuyViewModel> GetList(
            string tuNgay,
            string denNgay,
            int? idTaiKhoanThanhToan);
    }
}
