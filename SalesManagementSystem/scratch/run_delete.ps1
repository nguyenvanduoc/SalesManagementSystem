$connectionString = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

$sql = Get-Content -Path "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\delete_duplicate_bh00268.sql" -Raw
$cmd = $conn.CreateCommand()
$cmd.CommandText = $sql
$cmd.ExecuteNonQuery() | Out-Null
Write-Host "=== EXECUTED SQL SUCCESSFULLY! ==="

Write-Host "`n=== CHECK AFTER DELETION: BAN_ChungTuBanHang for DH26000268 ==="
$cmd.CommandText = @"
SELECT c.ID, c.SoChungTu, c.NgayChungTu, c.IDDonDatHang, c.TongCong, c.TrangThai, c.NgayTao
FROM BAN_ChungTuBanHang c
WHERE c.IDDonDatHang = 271 OR c.SoChungTu IN ('BH00267', 'BH00268')
"@
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$da.Fill($dt) | Out-Null
$dt | Format-Table -AutoSize

Write-Host "`n=== CHECK sp_BAN_ChungTuBanHang_GetDonHangList for DH26000268 ==="
$cmd.CommandText = "EXEC sp_BAN_ChungTuBanHang_GetDonHangList @SoDonHang = 'DH26000268'"
$dt2 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt2) | Out-Null
$dt2 | Format-Table SoDonHang, TenKhachHang, MaSanPham, TenSanPham, SoLuong, ThanhTienHang, TongTien, SoChungTu, TrangThaiChungTu -AutoSize

$conn.Close()
