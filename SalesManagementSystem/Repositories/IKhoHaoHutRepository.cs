using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories
{
    public interface IKhoHaoHutRepository
    {
        List<HaoHutHangHoaViewModel> GetList(HaoHutHangHoaFilter filter);
        HaoHutHangHoaViewModel GetByID(int id);
        int Insert(HaoHutHangHoaViewModel model, int userID);
        void Update(HaoHutHangHoaViewModel model, int userID);
        void Delete(int id, int userID);
        void DeleteDetails(int idHaoHut);
        void InsertDetail(HaoHutHangHoaChiTietViewModel detail, int userID);
        void GhiNhan(int id, int userID);
        void Huy(int id, int userID);
        
        List<dynamic> GetDonHang(string keyword);
        List<dynamic> GetChiTietDonHang(int idDonHang);
        decimal GetTonKho(int idKho, int idSanPham);
        List<dynamic> GetAllTonKhoByKho(int idKho);
        decimal GetGiaNhapGanNhat(int idSanPham);
    }
}
