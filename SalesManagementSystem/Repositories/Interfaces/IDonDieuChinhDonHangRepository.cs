using System;
using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IDonDieuChinhDonHangRepository
    {
        IEnumerable<DonDieuChinhListViewModel> GetPaged(
            int page, int pageSize,
            string tuNgay, string denNgay,
            int? idKhachHang, string soDonHang,
            bool chiDonDieuChinh,
            out int totalRecords);

        IEnumerable<DonDieuChinhHistoryViewModel> GetAdjustHistory(int idDonHang);

        void SaveAdjustment(DonDieuChinhPostModel model, int userId);
    }
}
