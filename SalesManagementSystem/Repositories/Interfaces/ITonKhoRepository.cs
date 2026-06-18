using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface ITonKhoRepository
    {
        IEnumerable<TonKhoListViewModel> GetList(
            int? idKho, 
            int? idSanPham, 
            string tuNgay, 
            string denNgay, 
            bool chiConTon);

        IEnumerable<TheKhoListViewModel> GetTheKho(
            int idKho, 
            int idSanPham, 
            string tuNgay, 
            string denNgay);

        TonKhoDashboardViewModel GetDashboard(
            int? idKho, 
            int? idSanPham,
            string tuNgay, 
            string denNgay,
            bool chiConTon);
    }
}
