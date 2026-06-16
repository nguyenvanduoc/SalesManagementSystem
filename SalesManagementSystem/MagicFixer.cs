using System;
using System.IO;
using System.Text;
using System.Linq;

public class MagicFixer {
    public static void Run() {
        string dir = @".";
        var files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
                             .Where(f => f.EndsWith(".cs") || f.EndsWith(".cshtml")).ToList();
        
        Encoding w1252 = Encoding.GetEncoding(1252);
        int count = 0;

        string[] knownGoodStrings = new string[] {
            "Đơn hàng đã giao không được chỉnh sửa.",
            "Đơn hàng đã hủy không được chỉnh sửa.",
            "Không thể xóa đơn đặt hàng đã giao.",
            "Một số đơn đặt hàng đã giao, không thể xóa.",
            "Xóa đơn đặt hàng thành công",
            "Không tìm thấy đơn hàng.",
            "Không thể chuyển trạng thái đơn hàng đã giao.",
            "Đơn hàng này đã bị hủy trước đó.",
            "Đơn đặt hàng",
            "Chuyển trạng thái đơn hàng thành công.",
            "Lỗi khi chuyển trạng thái đơn hàng.",
            "Chỉnh sửa đơn đặt hàng",
            "Tạo đơn đặt hàng",
            "Danh sách đơn đặt hàng",
            "Vui lòng chọn khách hàng",
            "Số đơn hàng đã tồn tại trong hệ thống",
            "Vui lòng chọn nhân viên phụ trách",
            "Vui lòng thêm ít nhất một sản phẩm vào đơn hàng",
            "Vui lòng chọn sản phẩm",
            "Đơn giá không được âm",
            "Số lượng không được âm",
            "Thuế GTGT không được âm",
            "Vui lòng nhập số đơn hàng",
            "Không tìm thấy",
            "Chuyển trạng thái đơn hàng thành công.",
            "Thao tác",
            "Bạn có chắc chắn muốn xóa",
            "kho hàng đã chọn không?",
            "Nhập tên hoặc mã",
            "kho hàng",
            "Đã giao hàng",
            "Hủy",
            "Không thể xóa",
            "Người dùng",
            "Tên đăng nhập",
            "Thêm mới",
            "Cập nhật",
            "Biểu mẫu",
            "Khách hàng",
            "Nhân sự",
            "Nhật ký",
            "Phân quyền",
            "Phòng ban",
            "Sản phẩm",
            "Phiếu xuất kho",
            "Phiếu nhập kho",
            "Chứng từ bán hàng"
        };

        foreach (var file in files) {
            string originalText = File.ReadAllText(file, Encoding.UTF8);
            string fixedText = originalText;
            bool changed = false;

            foreach (var good in knownGoodStrings) {
                // Replicate the bug: UTF8 bytes were interpreted as Windows-1252 characters
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(good);
                string corrupted = w1252.GetString(utf8Bytes);
                
                if (fixedText.Contains(corrupted)) {
                    fixedText = fixedText.Replace(corrupted, good);
                    changed = true;
                }
            }
            
            if (changed) {
                File.WriteAllText(file, fixedText, new UTF8Encoding(true));
                Console.WriteLine("Fixed " + file);
                count++;
            }
        }
        Console.WriteLine("Total fixed: " + count);
    }
}
