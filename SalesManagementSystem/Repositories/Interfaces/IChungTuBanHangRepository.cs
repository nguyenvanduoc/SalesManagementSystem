using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using System.Collections.Generic;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IChungTuBanHangRepository
    {
        IEnumerable<ChungTuBanHangListViewModel> GetList(string tuNgay, string denNgay, string soChungTu, int? idKhachHang, int? idKho, int? trangThai);
        IEnumerable<DonHangChungTuViewModel> GetDonHangList(string tuNgay, string denNgay, string soDonHang, int? idKhachHang, int? trangThaiChungTu);
        ChungTuBanHangViewModel GetById(int id);
        string GenerateSoChungTu();
        int Insert(ChungTuBanHangViewModel model, int nguoiTao);
        void UpdateStatus(int id, int trangThai, int nguoiCapNhat);
        void Cancel(int id, int nguoiHuy, string lyDo);
        IEnumerable<CheckTonKhoResponseViewModel> CheckTonKhoByKho(int idKho, List<CheckTonKhoRequestItem> sanPhams);
        IEnumerable<CheckTonKhoResponseViewModel> CheckTonKhoAllKho(List<CheckTonKhoRequestItem> sanPhams);
    }
}
