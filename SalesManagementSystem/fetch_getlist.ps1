$ErrorActionPreference = 'Stop'
$wc = New-Object System.Net.WebClient
$wc.Headers.Add('Content-Type', 'application/x-www-form-urlencoded')
try {
    $result = $wc.UploadString('http://localhost:59935/BaoCaoKetQuaHoatDongKinhDoanh/GetList', '')
    Set-Content -Path 'c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\getlist_result.txt' -Value $result
} catch {
    Set-Content -Path 'c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\getlist_result.txt' -Value $_.Exception.ToString()
}
