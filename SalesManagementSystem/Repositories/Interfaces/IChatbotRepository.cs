using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IChatbotRepository
    {
        IEnumerable<ChatbotDanhMucItem> GetDanhMucKhachHang();

        IEnumerable<ChatbotDanhMucItem> GetDanhMucSanPham();

        IEnumerable<ChatbotDanhMucItem> GetDanhMucNhaCungCap();

        IEnumerable<ChatbotCongNoNccItem> GetCongNoNhaCungCap(int? idNhaCungCap);

        IEnumerable<ChatbotHangDiDuongItem> GetHangDangVeKho(int? idSanPham);

        IEnumerable<ChatbotHangDiDuongItem> GetHangDangGiao(int? idSanPham);
    }
}
