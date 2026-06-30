using System;
using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface ITraHangBanRepository
    {
        IEnumerable<TraHangBanViewModel> GetPaged(int page, int pageSize, string tuNgay, string denNgay, int? idKhachHang, int? trangThai, string soChungTu, out int totalRecords);
        TraHangBanViewModel GetById(int id);
        IEnumerable<TraHangBanChiTietViewModel> GetChiTietByTraHangId(int id);
        
        string GenerateSoChungTu();
        
        int Insert(TraHangBan traHang, List<TraHangBanChiTiet> chiTiets);
        void Update(TraHangBan traHang, List<TraHangBanChiTiet> chiTiets);
        void Delete(int id);
        void GhiSo(int id, int nguoiThucHien);
        void Huy(int id, int nguoiThucHien);
        
        IEnumerable<TraHangBanViewModel> LoadDonHangTra(string tuNgay, string denNgay, string soDonHang);
        IEnumerable<TraHangBanChiTietViewModel> LoadChiTietDonHang(int idDonDatHang);
    }
}
