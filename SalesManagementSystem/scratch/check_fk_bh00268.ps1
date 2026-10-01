$connectionString = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Host "=== 1. CHECK PHIEU THU ==="
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%PhieuThu%'
"@
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$da.Fill($dt) | Out-Null
$dt | Format-Table -AutoSize

Write-Host "`n=== 2. CHECK KHO_PhieuXuat_ChiTiet ==="
$cmd.CommandText = "SELECT * FROM KHO_PhieuXuat_ChiTiet WHERE IDPhieuXuat = 268"
$dt2 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt2) | Out-Null
$dt2 | Format-Table -AutoSize

Write-Host "`n=== 3. CHECK KT_NhatKyChung ==="
$cmd.CommandText = "SELECT ID, SoChungTu, NgayChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai FROM KT_NhatKyChung WHERE SoChungTu IN ('BH00268', 'PX00268')"
$dt3 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt3) | Out-Null
$dt3 | Format-Table -AutoSize

Write-Host "`n=== 4. CHECK KHO_GiaoDichKho columns ==="
$cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KHO_GiaoDichKho'"
$dt4 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt4) | Out-Null
$dt4 | Format-Table -AutoSize

$conn.Close()
