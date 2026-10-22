$connectionString = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Host "=== TEST sp_KT_SoQuy_GetTaiKhoanSummary SAU KHI SYNC ==="
$cmd = $conn.CreateCommand()
$cmd.CommandText = "EXEC sp_KT_SoQuy_GetTaiKhoanSummary @TuNgay='2026-10-01', @DenNgay='2026-10-31 23:59:59'"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$da.Fill($dt) | Out-Null
$dt | Select-Object ID, TenTaiKhoan, SoTaiKhoan, SoDuDauKy, ThuTrongKy, ChiTrongKy, @{Name="SoDuCuoiKy";Expression={$_.SoDuDauKy + $_.ThuTrongKy - $_.ChiTrongKy}} | Format-Table -AutoSize | Out-String | Write-Host

$sumDauKy = ($dt | Measure-Object -Property SoDuDauKy -Sum).Sum
$sumThu = ($dt | Measure-Object -Property ThuTrongKy -Sum).Sum
$sumChi = ($dt | Measure-Object -Property ChiTrongKy -Sum).Sum
$sumCuoiKy = $sumDauKy + $sumThu - $sumChi

Write-Host "Tong So Du Dau Ky : $("{0:N0}" -f $sumDauKy)"
Write-Host "Tong Thu Trong Ky : $("{0:N0}" -f $sumThu)"
Write-Host "Tong Chi Trong Ky : $("{0:N0}" -f $sumChi)"
Write-Host "Tong So Du Cuoi Ky: $("{0:N0}" -f $sumCuoiKy)"

$conn.Close()
