$dllPath = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll"
$asm = [System.Reflection.Assembly]::LoadFrom($dllPath)
$factoryType = $asm.GetType("SalesManagementSystem.Data.DbConnectionFactory")
$factory = [Activator]::CreateInstance($factoryType)
$createConnMethod = $factoryType.GetMethod("CreateConnection")
$conn = $createConnMethod.Invoke($factory, $null)
$conn.Open()

Write-Host "=== SQL CONNECTION SUCCESSFUL! ==="

Write-Host "`n=== 1. DON HANG DH26000268 ==="
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT * FROM NS_DonDatHang WHERE SoDonHang = 'DH26000268'"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$da.Fill($dt) | Out-Null
$dt | Format-List

Write-Host "`n=== 2. CHUNG TU BAN HANG CO IDDonDatHang HOAC SoChungTu BH00267/BH00268 ==="
$cmd.CommandText = @"
SELECT c.ID, c.SoChungTu, c.NgayChungTu, c.IDDonDatHang, c.IDKhachHang, c.IDKho, 
       c.TongTienHang, c.PhiBocXep, c.TongTienKhac, c.TongCong, c.TrangThai, c.IsDeleted,
       c.NguoiTao, u1.TenDangNhap as TenNguoiTao, c.NgayTao,
       c.NguoiSua, u2.TenDangNhap as TenNguoiSua, c.NgaySua
FROM BAN_ChungTuBanHang c
LEFT JOIN HT_NguoiDung u1 ON c.NguoiTao = u1.ID
LEFT JOIN HT_NguoiDung u2 ON c.NguoiSua = u2.ID
WHERE c.SoChungTu IN ('BH00267', 'BH00268') 
   OR c.IDDonDatHang IN (SELECT ID FROM NS_DonDatHang WHERE SoDonHang = 'DH26000268')
ORDER BY c.ID ASC
"@
$dt2 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt2) | Out-Null
$dt2 | Format-List

Write-Host "`n=== 3. BAN_ChungTuBanHang_ChiTiet ==="
$cmd.CommandText = @"
SELECT ct.ID, ct.IDChungTuBanHang, c.SoChungTu, ct.IDSanPham, sp.MaSanPham, sp.TenSanPham, 
       ct.SoLuong, ct.DonGia, ct.ThanhTienHang, ct.ThanhTienBocXep, ct.TongSauThue
FROM BAN_ChungTuBanHang_ChiTiet ct
LEFT JOIN BAN_ChungTuBanHang c ON ct.IDChungTuBanHang = c.ID
LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
WHERE ct.IDChungTuBanHang IN (SELECT ID FROM BAN_ChungTuBanHang WHERE SoChungTu IN ('BH00267', 'BH00268') OR IDDonDatHang IN (SELECT ID FROM NS_DonDatHang WHERE SoDonHang = 'DH26000268'))
ORDER BY ct.IDChungTuBanHang, ct.STT
"@
$dt3 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt3) | Out-Null
$dt3 | Format-Table -AutoSize

Write-Host "`n=== 4. PHIEU XUAT KHO LIEN QUAN ==="
$cmd.CommandText = @"
SELECT px.ID, px.MaPhieuXuat, px.NgayXuat, px.IDChungTuBanHang, c.SoChungTu, px.IDDonHang, px.TrangThai, px.NguoiTao, px.NgayTao, px.GhiChu
FROM KHO_PhieuXuat px
LEFT JOIN BAN_ChungTuBanHang c ON px.IDChungTuBanHang = c.ID
WHERE px.IDChungTuBanHang IN (SELECT ID FROM BAN_ChungTuBanHang WHERE SoChungTu IN ('BH00267', 'BH00268') OR IDDonDatHang IN (SELECT ID FROM NS_DonDatHang WHERE SoDonHang = 'DH26000268'))
   OR px.IDDonHang IN (SELECT ID FROM NS_DonDatHang WHERE SoDonHang = 'DH26000268')
ORDER BY px.ID ASC
"@
$dt4 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt4) | Out-Null
$dt4 | Format-Table -AutoSize

Write-Host "`n=== 5. AUDIT / LOG (HT_NhatKyHeThong hoac KT_NhatKyChung) ==="
$cmd.CommandText = @"
SELECT TOP 20 * FROM KT_NhatKyChung 
WHERE SoChungTu IN ('BH00267', 'BH00268', 'DH26000268') 
   OR DienGiai LIKE '%DH26000268%' 
   OR DienGiai LIKE '%BH00267%' 
   OR DienGiai LIKE '%BH00268%'
"@
$dt5 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt5) | Out-Null
$dt5 | Format-Table -AutoSize

$conn.Close()
