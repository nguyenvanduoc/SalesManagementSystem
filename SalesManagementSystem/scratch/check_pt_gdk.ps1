$connectionString = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Host "=== 1. CHECK BAN_PhieuThuKhachHang ==="
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT * FROM BAN_PhieuThuKhachHang WHERE IDChungTuBanHang = 274 OR GhiChu LIKE '%BH00268%'"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$da.Fill($dt) | Out-Null
$dt | Format-Table -AutoSize

Write-Host "`n=== 2. CHECK KT_PhieuThuChiTiet ==="
$cmd.CommandText = "SELECT * FROM KT_PhieuThuChiTiet WHERE IDChungTu = 274 OR SoChungTu = 'BH00268'"
$dt2 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt2) | Out-Null
$dt2 | Format-Table -AutoSize

Write-Host "`n=== 3. CHECK KHO_GiaoDichKho ==="
$cmd.CommandText = "SELECT * FROM KHO_GiaoDichKho WHERE SoChungTu IN ('BH00268', 'PX00268')"
$dt3 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt3) | Out-Null
$dt3 | Format-Table -AutoSize

$conn.Close()
