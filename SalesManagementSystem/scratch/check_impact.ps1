$connectionString = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Host "=== 1. KT_NhatKyChung for BH00267, BH00268 ==="
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT * FROM KT_NhatKyChung WHERE SoChungTu IN ('BH00267', 'BH00268', 'DH26000268')"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$da.Fill($dt) | Out-Null
$dt | Format-Table SoChungTu, NgayChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai -AutoSize

Write-Host "`n=== 2. KHO_PhieuXuat (XUAT KHO) ==="
$cmd.CommandText = "SELECT * FROM KHO_PhieuXuat WHERE IDChungTuBanHang IN (273, 274) OR GhiChu LIKE '%BH00267%' OR GhiChu LIKE '%BH00268%' OR GhiChu LIKE '%DH26000268%'"
$dt2 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt2) | Out-Null
$dt2 | Format-List

Write-Host "`n=== 3. KHO_GiaoDichKho (THE KHO / XUAT KHO) ==="
$cmd.CommandText = "SELECT * FROM KHO_GiaoDichKho WHERE SoChungTu IN ('BH00267', 'BH00268', 'DH26000268') OR IDChungTu IN (273, 274)"
$dt3 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt3) | Out-Null
$dt3 | Format-Table -AutoSize

Write-Host "`n=== 4. CONG NO KHACH HANG ==="
$cmd.CommandText = "SELECT * FROM BAN_CongNoKhachHang WHERE IDChungTuBanHang IN (273, 274)"
$dt4 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt4) | Out-Null
$dt4 | Format-Table -AutoSize

Write-Host "`n=== 5. CHECK IF OTHER ORDERS HAVE MULTIPLE CHUNG TU BAN HANG ==="
$cmd.CommandText = @"
SELECT IDDonDatHang, COUNT(ID) as SoLuongChungTu, STRING_AGG(SoChungTu, ', ') as DanhSachChungTu
FROM BAN_ChungTuBanHang
WHERE IDDonDatHang IS NOT NULL AND IsDeleted = 0
GROUP BY IDDonDatHang
HAVING COUNT(ID) > 1
"@
$dt5 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt5) | Out-Null
$dt5 | Format-Table -AutoSize

$conn.Close()
