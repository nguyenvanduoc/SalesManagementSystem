using System;
using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IBaoCaoDoiChieuCongNoKhachHangRepository
    {
        IEnumerable<BaoCaoDoiChieuCongNoKhachHangViewModel> GetList(int? idKhachHang, DateTime tuNgay, DateTime denNgay, string soChungTu = null);
        IEnumerable<dynamic> GetKhachHangDropdown();
    }
}
