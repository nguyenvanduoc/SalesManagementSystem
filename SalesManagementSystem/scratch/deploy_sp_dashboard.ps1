$connectionString = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Host "=== THUC THI CAP NHAT sp_Dashboard_GetData ==="
$sql = Get-Content -Raw -Path "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\sp_Dashboard_GetData.sql"
$batches = $sql -split "(?i)\bGO\b"
foreach ($b in $batches) {
    $t = $b.Trim()
    if (![string]::IsNullOrWhiteSpace($t)) {
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $t
        $cmd.ExecuteNonQuery() | Out-Null
    }
}
Write-Host "Cap nhat sp_Dashboard_GetData thanh cong!"

Write-Host "`n=== TEST sp_Dashboard_GetData (01/10/2026 den 31/10/2026) ==="
$cmd = $conn.CreateCommand()
$cmd.CommandText = "EXEC sp_Dashboard_GetData @TuNgay='2026-10-01', @DenNgay='2026-10-31', @TuNgayKyTruoc='2026-09-01', @DenNgayKyTruoc='2026-09-30'"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$ds = New-Object System.Data.DataSet
$da.Fill($ds) | Out-Null

$summary = $ds.Tables[0].Rows[0]
Write-Host "DoanhThu: $("{0:N0}" -f $summary['DoanhThu'])"
Write-Host "LoiNhuan: $("{0:N0}" -f $summary['LoiNhuan'])"
Write-Host "CongNoKH : $("{0:N0}" -f $summary['CongNoKhachHang'])"
Write-Host "CongNoNCC: $("{0:N0}" -f $summary['CongNoNhaCungCap'])"
Write-Host "----------------------------------------------------"
Write-Host "TienMat        : $("{0:N0}" -f $summary['TienMat'])"
Write-Host "TienGuiNH      : $("{0:N0}" -f $summary['TienGuiNH'])"
Write-Host "TongSoDuTaiKhoan: $("{0:N0}" -f $summary['TongSoDuTaiKhoan'])"
Write-Host "TongThu (Ky)   : $("{0:N0}" -f $summary['TongThu'])"
Write-Host "TongChi (Ky)   : $("{0:N0}" -f $summary['TongChi'])"

$conn.Close()
