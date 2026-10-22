$filePath = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\sp_Dashboard_GetData.sql"
$lines = [System.IO.File]::ReadAllLines($filePath, [System.Text.Encoding]::UTF8)

for ($i = 517; $i -le 551; $i++) {
    $line = $lines[$i]
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($line)
    $hex = ($bytes | ForEach-Object { $_.ToString("X2") }) -join " "
    Write-Host "$($i + 1): $line"
    Write-Host "   HEX: $hex"
}
