$dllPath = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll"
$asm = [System.Reflection.Assembly]::LoadFrom($dllPath)
$factoryType = $asm.GetType("SalesManagementSystem.Data.DbConnectionFactory")
$factory = [Activator]::CreateInstance($factoryType)
$createConnMethod = $factoryType.GetMethod("CreateConnection")
$conn = $createConnMethod.Invoke($factory, $null)
$conn.Open()

Write-Host "=== SQL CONNECTION SUCCESSFUL! ==="

$cmd = $conn.CreateCommand()
$cmd.CommandText = "
SELECT 
    sp.MaSanPham,
    sp.TenSanPham,
    SUM(ct.SoLuong) AS TongSL,
    SUM(ct.ThanhTien) AS TongDoanhThu,
    SUM(ct.ThanhTienVon) AS TongThanhTienVon,
    SUM(
        CASE 
            WHEN ISNULL(ct.ThanhTienVon, 0) > 0 THEN ct.ThanhTienVon
            ELSE ct.SoLuong * ISNULL(ap.AvgDonGia, 0)
        END
    ) AS GiaVon_SP_BC,
    SUM(ct.SoLuong * ISNULL(ap.AvgDonGia, 0)) AS GiaVon_Avg_NhapKhoChinh
FROM BAN_ChungTuBanHang_ChiTiet ct
INNER JOIN BAN_ChungTuBanHang hd ON ct.IDChungTuBanHang = hd.ID
INNER JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
OUTER APPLY (
    SELECT 
        SUM(pn_ct.SoLuong * pn_ct.DonGia) / NULLIF(SUM(pn_ct.SoLuong), 0) AS AvgDonGia
    FROM KHO_PhieuNhap_ChiTiet pn_ct
    INNER JOIN KHO_PhieuNhap pn ON pn_ct.IDPhieuNhap = pn.ID
    INNER JOIN DM_KhoHang kh_pn ON pn.IDKho = kh_pn.ID AND kh_pn.IsKhoChinh = 1
    WHERE pn_ct.IDSanPham = ct.IDSanPham 
      AND pn.TrangThai = 2 AND pn.IsDeleted = 0
) ap
WHERE hd.IsDeleted = 0 AND hd.TrangThai = 2
GROUP BY sp.MaSanPham, sp.TenSanPham, ap.AvgDonGia
ORDER BY TongDoanhThu DESC
"

$adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dataset = New-Object System.Data.DataSet
$adapter.Fill($dataset) | Out-Null

$dataset.Tables[0] | Format-Table -AutoSize

$conn.Close()
