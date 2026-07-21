using System;
using System.IO;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Views\Dashboard\Index.cshtml";
        string text = File.ReadAllText(path);
        string oldStr = @"                    tkHtml += `
                            <div class=""text-end fw-bold ${soDuColor}"" style=""font-size:0.95rem"">${formatMoney(tk.SoDuHienTai)}</div>
                        </div>
                    `;
                });
            } else {
                tkHtml = '<div class=""text-center text-muted small py-3"">Không có tài khoản hoạt động.</div>';
            }
            $('#listTaiKhoan_@uid').html(tkHtml);";

        string newStr = @"                    tkHtml += `
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
            $('#tbTaiKhoan_@uid').html(tkHtml);
            
            var today = new Date();
            var monthStr = 'THÁNG ' + (today.getMonth() + 1) + '/' + today.getFullYear();
            var dateStr = today.getDate().toString().padStart(2, '0') + '/' + (today.getMonth() + 1).toString().padStart(2, '0') + '/' + today.getFullYear();
            $('#lblThang_@uid').text(monthStr);
            $('#lblNgay_@uid').text(dateStr);";

        text = text.Replace(oldStr.Replace("\r\n", "\n"), newStr.Replace("\r\n", "\n"));
        text = text.Replace(oldStr, newStr);

        File.WriteAllText(path, text);
    }
}
