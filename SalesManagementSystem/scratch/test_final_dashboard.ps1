$dllPath = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll"
$connectionString = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Host "=== TEST TRUY VAN DU LIEU QUA Dll / SP sp_Dashboard_GetData ==="
$cmd = $conn.CreateCommand()
$cmd.CommandText = "EXEC sp_Dashboard_GetData @TuNgay='2026-10-01', @DenNgay='2026-10-31', @TuNgayKyTruoc='2026-09-01', @DenNgayKyTruoc='2026-09-30'"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$ds = New-Object System.Data.DataSet
$da.Fill($ds) | Out-Null

$summary = $ds.Tables[0].Rows[0]
Write-Host "TienMat        : $("{0:N0}" -f $summary['TienMat'])"
Write-Host "TienGuiNH      : $("{0:N0}" -f $summary['TienGuiNH'])"
Write-Host "TongSoDuTaiKhoan: $("{0:N0}" -f $summary['TongSoDuTaiKhoan'])"

Write-Host "`n=== DANH SACH TAI KHOAN (Table 8) ==="
$tbTk = $ds.Tables[7]
$tbTk | Select-Object TenTaiKhoan, NganHang, @{N="SoDuDauKy";E={$_.SoDuDauKy.ToString("N0")}}, @{N="ThuTrongKy";E={$_.ThuTrongKy.ToString("N0")}}, @{N="ChiTrongKy";E={$_.ChiTrongKy.ToString("N0")}}, @{N="SoDuCuoiKy";E={$_.SoDuCuoiKy.ToString("N0")}} | Format-Table -AutoSize | Out-String | Write-Host

$conn.Close()
