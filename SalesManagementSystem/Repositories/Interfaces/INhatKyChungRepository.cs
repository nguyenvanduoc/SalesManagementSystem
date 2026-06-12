using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using System.Collections.Generic;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface INhatKyChungRepository
    {
        IEnumerable<NhatKyChungListViewModel> GetList(string tuNgay, string denNgay, string soChungTu, string taiKhoanNo, string taiKhoanCo, string loaiChungTu);
        void Insert(KT_NhatKyChung entity);
        void Cancel(string loaiChungTu, int idChungTu, int nguoiHuy);
    }
}
