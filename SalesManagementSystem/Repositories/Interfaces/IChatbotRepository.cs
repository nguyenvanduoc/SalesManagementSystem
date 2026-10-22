using System;
using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IChatbotRepository
    {
        IEnumerable<ChatbotDanhMucItem> GetDanhMucKhachHang();

        IEnumerable<ChatbotDanhMucItem> GetDanhMucSanPham();

        IEnumerable<ChatbotDanhMucItem> GetDanhMucNhaCungCap();

        IEnumerable<ChatbotDanhMucItem> GetDanhMucKho();

        IEnumerable<ChatbotCongNoNccItem> GetCongNoNhaCungCap(int? idNhaCungCap);

        IEnumerable<ChatbotHangDiDuongItem> GetHangDangVeKho(int? idSanPham);

        IEnumerable<ChatbotHangDiDuongItem> GetHangDangGiao(int? idSanPham);

        IEnumerable<ChatbotChungTuKhoItem> GetNhapKho(DateTime tuNgay, DateTime denNgay, int? idSanPham, int? idKho);

        IEnumerable<ChatbotChungTuKhoItem> GetXuatKho(DateTime tuNgay, DateTime denNgay, int? idSanPham, int? idKho);
    }
}
