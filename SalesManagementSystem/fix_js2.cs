using System;
using System.IO;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Views\Dashboard\Index.cshtml";
        string[] lines = File.ReadAllLines(path);
        
        string newCode = @"                    tkHtml += `
                        <tr style=""background-color: #fff;"">
                            <td class=""text-center"" style=""color: #2c3e50;"">${tk.TenTaiKhoan}</td>
                            <td class=""text-end"" style=""color: #2c3e50;"">${formatNumber(tk.SoDuDauKy)}</td>
                            <td class=""text-end"" style=""color: #2c3e50;"">${thuStr}</td>
                            <td class=""text-end"" style=""color: #2c3e50;"">${chiStr}</td>
                            <td class=""text-end ${amQuyColor}"">${formatNumber(tk.SoDuCuoiKy)}</td>
                            <td>${tk.GhiChu || ''}</td>
                        </tr>
                    `;
                });
                
                var tfHtml = `
                    <tr>
                        <td class=""text-center"">Tổng</td>
                        <td class=""text-end"">${formatNumber(totalDauKy)}</td>
                        <td class=""text-end"">${formatNumber(totalThu)}</td>
                        <td class=""text-end"">${formatNumber(totalChi)}</td>
                        <td class=""text-end"">${formatNumber(totalCuoiKy)}</td>
                        <td></td>
                    </tr>
                `;
                $('#tfTaiKhoan_@uid').html(tfHtml);
            } else {
                tkHtml = '<tr><td colspan=""6"" class=""text-center text-muted small py-3"">Không có tài khoản hoạt động.</td></tr>';
                $('#tfTaiKhoan_@uid').empty();
            }
            $('#tbTaiKhoan_@uid').html(tkHtml);";

        var newLines = newCode.Replace("\r\n", "\n").Split('\n');
        
        var result = new System.Collections.Generic.List<string>();
        for (int i = 0; i < lines.Length; i++)
        {
            if (i >= 937 && i <= 945)
            {
                if (i == 937) {
                    result.AddRange(newLines);
                }
                continue;
            }
            result.Add(lines[i]);
        }
        
        File.WriteAllLines(path, result);
    }
}
