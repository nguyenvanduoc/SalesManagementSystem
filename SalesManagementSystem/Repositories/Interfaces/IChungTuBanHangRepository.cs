using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using System.Collections.Generic;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IChungTuBanHangRepository
    {
        IEnumerable<ChungTuBanHangListViewModel> GetList(string tuNgay, string denNgay, string soChungTu, int? idKhachHang, int? idKho, int? trangThai);
        IEnumerable<DonHangChungTuViewModel> GetDonHangList(string tuNgay, string denNgay, string soDonHang, int? idKhachHang, int? trangThaiChungTu, int? idSanPham = null, int? idPhuongTien = null, string hoTenTaiXe = null);
        ChungTuBanHangViewModel GetById(int id);
        string GenerateSoChungTu();
        int Insert(ChungTuBanHangViewModel model, int nguoiTao, bool ghiSo = false, int trangThai = 1);
        void Update(ChungTuBanHangViewModel model, int nguoiCapNhat, bool ghiSo = false, int trangThai = 1);
        void UpdateStatus(int id, int trangThai, int nguoiCapNhat);
        void GhiSo(int id, int nguoiGhi);
        void Cancel(int id, int? idDonDatHang, int nguoiHuy, string lyDo);
        void BoGhi(int id, int nguoiBoGhi);
        IEnumerable<CheckTonKhoResponseViewModel> CheckTonKhoByKho(int idKho, List<CheckTonKhoRequestItem> sanPhams);
        IEnumerable<CheckTonKhoResponseViewModel> CheckTonKhoAllKho(List<CheckTonKhoRequestItem> sanPhams);
    }
}
