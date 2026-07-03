using System;
using System.IO;

class Program {
    static void Main() {
        string path = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Scripts\tab-manager.js";
        string content = File.ReadAllText(path);
        string badCode = "// X? lý n?p n?i dung khi chuy?n sang tab chua loadr tabId = $(e.target).attr('id');";
        string goodCode = "// X? lý n?p n?i dung khi chuy?n sang tab chua load (ví d? Dashboard)\r\n        $(document).on('shown.bs.tab', 'button[data-bs-toggle=\"tab\"]', function (e) {\r\n            var tabId = $(e.target).attr('id');";
        string newContent = content.Replace(badCode, goodCode);
        File.WriteAllText(path, newContent);
    }
}
