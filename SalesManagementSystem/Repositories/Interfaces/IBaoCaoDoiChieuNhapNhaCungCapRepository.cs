using System;
using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IBaoCaoDoiChieuNhapNhaCungCapRepository
    {
        IEnumerable<BaoCaoDoiChieuNhapNhaCungCapViewModel> GetList(int? idNhaCungCap, DateTime tuNgay, DateTime denNgay);
        IEnumerable<dynamic> GetNhaCungCapDropdown();
    }
}
