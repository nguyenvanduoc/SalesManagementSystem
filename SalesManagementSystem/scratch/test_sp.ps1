$datPath = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\system.dat"
$bytes = [System.IO.File]::ReadAllBytes($datPath)
$b64 = [System.Text.Encoding]::UTF8.GetString($bytes)
$raw = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($b64))
$parts = $raw -split "@@"
$connStr = $parts[1]
Write-Host "Connection string from system.dat: $connStr"

$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

Write-Host "`n--- 1. sp_Dashboard_GetData Result ---"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "EXEC sp_Dashboard_GetData '2026-08-01', '2026-08-31', '2026-07-01', '2026-07-31'"
$reader = $cmd.ExecuteReader()
if ($reader.Read()) {
    Write-Host "TongTienHangNCC :" $reader["TongTienHangNCC"]
    Write-Host "DaThanhToanNCC  :" $reader["DaThanhToanNCC"]
    Write-Host "CongNoNhaCungCap:" $reader["CongNoNhaCungCap"]
}
$reader.Close()

Write-Host "`n--- 2. Màn hình Công nợ NCC (sp_CongNo_PhaseTra_NCC_GetList NULL, NULL) ---"
$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = "EXEC sp_CongNo_PhaseTra_NCC_GetList NULL, NULL, NULL, NULL"
$reader2 = $cmd2.ExecuteReader()
$sumHang = 0
$sumDaTra = 0
$sumConLai = 0
$count = 0
while ($reader2.Read()) {
    $count++
    $sumHang += [decimal]$reader2["TongTienHang"]
    $sumDaTra += [decimal]$reader2["DaThanhToan"]
    $sumConLai += [decimal]$reader2["ConLai"]
}
$reader2.Close()
Write-Host "Record count     : $count"
Write-Host "TongTienHang sum :" $sumHang
Write-Host "DaThanhToan sum  :" $sumDaTra
Write-Host "ConLai sum       :" $sumConLai

Write-Host "`n--- 3. Màn hình Công nợ NCC với từ ngày 01/08/2026 đến 31/08/2026 ---"
$cmd3 = $conn.CreateCommand()
$cmd3.CommandText = "EXEC sp_CongNo_PhaseTra_NCC_GetList '2026-08-01', '2026-08-31', NULL, NULL"
$reader3 = $cmd3.ExecuteReader()
$sumHang3 = 0
$sumDaTra3 = 0
$sumConLai3 = 0
$count3 = 0
while ($reader3.Read()) {
    $count3++
    $sumHang3 += [decimal]$reader3["TongTienHang"]
    $sumDaTra3 += [decimal]$reader3["DaThanhToan"]
    $sumConLai3 += [decimal]$reader3["ConLai"]
}
$reader3.Close()
Write-Host "Record count     : $count3"
Write-Host "TongTienHang sum :" $sumHang3
Write-Host "DaThanhToan sum  :" $sumDaTra3
Write-Host "ConLai sum       :" $sumConLai3

$conn.Close()
