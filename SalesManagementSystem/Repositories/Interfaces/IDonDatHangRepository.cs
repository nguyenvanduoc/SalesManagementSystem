using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IDonDatHangRepository
    {
        IEnumerable<DonDatHangViewModel> GetPaged(
            int page, int pageSize,
            string tuNgay, string denNgay,
            int? idKhachHang, int? idNhanVien,
            int? trangThai, string soDonHang,
            out int totalRecords);

        NS_DonDatHang GetById(int id);

        List<DonDatHangChiTietViewModel> GetChiTietByDonId(int idDon);

        bool CheckDuplicateSoDon(string soDonHang, int excludeId = 0);

        int Insert(NS_DonDatHang header, List<NS_DonDatHangChiTiet> chiTiets);

        bool Update(NS_DonDatHang header, List<NS_DonDatHangChiTiet> chiTiets);

        bool Delete(int id);
    }
}
