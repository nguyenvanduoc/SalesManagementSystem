using System;
using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    /// <summary>
    /// Kết quả trả về cho widget Trợ lý ảo.
    /// </summary>
    public class ChatbotResponseViewModel
    {
        // Mã ý định đã nhận diện (CONG_NO_KH, TON_KHO...) — client gửi lại để hiểu câu hỏi nối tiếp
        public string Intent { get; set; }
        public string Text { get; set; }
        public List<ChatbotTableViewModel> Tables { get; set; } = new List<ChatbotTableViewModel>();
        public string Note { get; set; }
        public List<ChatbotLinkViewModel> Links { get; set; } = new List<ChatbotLinkViewModel>();
        public List<string> Suggestions { get; set; } = new List<string>();
    }

    public class ChatbotLinkViewModel
    {
        public string Url { get; set; }
        // Tên controller — dùng làm key khi mở màn hình bằng TabManager (trùng với tab mở từ menu)
        public string Key { get; set; }
        public string Title { get; set; }
    }

    public class ChatbotTableViewModel
    {
        public string Title { get; set; }
        public List<string> Headers { get; set; } = new List<string>();
        public List<bool> AlignRight { get; set; } = new List<bool>();
        public List<List<string>> Rows { get; set; } = new List<List<string>>();
    }

    /// <summary>
    /// Danh mục rút gọn (Khách hàng / Sản phẩm / NCC) để nhận diện tên trong câu hỏi.
    /// </summary>
    public class ChatbotDanhMucItem
    {
        public int ID { get; set; }
        public string Ma { get; set; }
        public string Ten { get; set; }
    }

    public class ChatbotCongNoNccItem
    {
        public int? ID { get; set; }
        public string Ten { get; set; }
        public decimal TongPhatSinh { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo { get; set; }
        public decimal QuaHan { get; set; }
        public int SoPhieuConNo { get; set; }
    }

    /// <summary>
    /// Một dòng hàng trên phiếu nhập / phiếu xuất kho đã ghi sổ (tra cứu nhập - xuất theo khoảng ngày).
    /// </summary>
    public class ChatbotChungTuKhoItem
    {
        public int IDChungTu { get; set; }
        public string SoChungTu { get; set; }
        public DateTime? Ngay { get; set; }
        public int? IDKho { get; set; }
        public string TenKho { get; set; }
        public string DoiTac { get; set; }
        public string Loai { get; set; }
        public int? IDSanPham { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string DVT { get; set; }
        public decimal SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
    }

    /// <summary>
    /// Một dòng hàng hóa đang đi đường (phiếu nhập chưa ghi sổ hoặc đơn hàng đang giao).
    /// </summary>
    public class ChatbotHangDiDuongItem
    {
        public int IDChungTu { get; set; }
        public string SoChungTu { get; set; }
        public DateTime? Ngay { get; set; }
        public string DoiTac { get; set; }
        public string PhuongTien { get; set; }
        public string TaiXe { get; set; }
        public int? IDSanPham { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string DVT { get; set; }
        public decimal SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
    }
}
