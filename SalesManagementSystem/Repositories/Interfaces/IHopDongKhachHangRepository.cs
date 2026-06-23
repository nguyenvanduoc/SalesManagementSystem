using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IHopDongKhachHangRepository
    {
        // Hop Dong Khach Hang
        IEnumerable<HopDongKhachHang> GetList(
            System.DateTime? tuNgay, 
            System.DateTime? denNgay, 
            string soHopDong, 
            string tenHopDong, 
            int? idKhachHang, 
            int? trangThai, 
            bool chiHienThiSapHetHan, 
            int pageNumber, 
            int pageSize,
            out int totalRecords,
            out int tongHopDong,
            out int dangHieuLuc,
            out int sapHetHan,
            out int daThanhLy);

        HopDongKhachHang GetByID(int id);
        
        bool CheckDuplicate(int id, string soHopDong);
        
        int Save(HopDongKhachHang model, int nguoiThaoTac);
        
        void Delete(int id, int nguoiThaoTac);
        
        void ThanhLy(int id, int nguoiThaoTac);
        
        void Huy(int id, int nguoiThaoTac);

        // Hop Dong Khach Hang File
        IEnumerable<HopDongKhachHangFile> File_GetList(int idHopDong);
        
        HopDongKhachHangFile File_GetByID(int id);
        
        void File_Save(HopDongKhachHangFile model, int nguoiThaoTac);
        
        void File_Delete(int id, int nguoiThaoTac);
    }
}
