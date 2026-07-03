$path = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Views\Dashboard\Index.cshtml"
$content = Get-Content -Path $path -Raw -Encoding UTF8

if ($content -notmatch "string uid = Guid.NewGuid\(\).ToString") {
    $content = $content.Replace('ViewBag.Title = "Dashboard";', 'ViewBag.Title = "Dashboard";
    string uid = Guid.NewGuid().ToString("N");')
}

$ids = @('dbTuNgay', 'dbDenNgay', 'btnLoadDashboard', 'btnResetDashboard', 'valDonHang', 'valDoanhThu', 'trendDoanhThu', 'valSpTon', 'trendTongSLTon', 'trendSpSapHet', 'valLoiNhuan', 'trendLoiNhuan', 'valCongNoKH', 'valCongNoNCC', 'valTienMat', 'valTongSoDu', 'tbRecentOrders', 'chartDoanhThu', 'valTongDonHang', 'chartDonHang', 'valGiaTriTonKho', 'valSpSapHet', 'chartBanChay', 'chartTonKho', 'valTongThu', 'valTongChi', 'valDongTien', 'chartThuChi', 'listTaiKhoan', 'wDonHang', 'wPhieuNhap', 'wTaiKhoan', 'wTonKho', 'wChungTu', 'listHoatDong', 'cnkhTongNo', 'cnkhSoDoiTuong', 'cnkhNoLonNhat', 'cnkhTenNoLonNhat', 'tbCongNoKH', 'cnnccTongNo', 'cnnccSoDoiTuong', 'cnnccNoLonNhat', 'cnnccTenNoLonNhat', 'tbCongNoNCC', 'tbTopKH', 'tbTopNCC', 'currentDateText')

foreach ($i in $ids) {
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "id=`"$i`"", "id=`"${i}_@uid`"")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\$\('#$i'\)", "$('#${i}_@uid')")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "document\.getElementById\('$i'\)", "document.getElementById('${i}_@uid')")
}

Set-Content -Path $path -Value $content -Encoding UTF8
Write-Host "Done!"
