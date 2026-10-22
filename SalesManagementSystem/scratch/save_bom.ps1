$path = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\sp_Dashboard_GetData.sql"
$bytes = [System.IO.File]::ReadAllBytes($path)
if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
    Write-Host "File ALREADY HAS UTF-8 BOM"
} else {
    Write-Host "File NO BOM -> Ghi lai voi UTF-8 BOM..."
    $content = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
    $utf8WithBom = New-Object System.Text.UTF8Encoding($true)
    [System.IO.File]::WriteAllText($path, $content, $utf8WithBom)
    Write-Host "Da luu thanh cong voi UTF-8 BOM!"
}
