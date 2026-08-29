$webConfigPath = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Web.config"
$xml = [xml](Get-Content $webConfigPath)
$connStr = $xml.configuration.connectionStrings.add | Where-Object { $_.name -eq "DefaultConnection" } | Select-Object -ExpandProperty connectionString -First 1

if ([string]::IsNullOrEmpty($connStr)) {
    $connStr = "Server=.;Database=SalesManagementSystem;Trusted_Connection=True;"
}

Write-Host "Using connection string: $connStr"

$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

# Query 1: Call sp_Dashboard_GetData summary for August 2026
Write-Host "=== 1. DASHBOARD SP DATA (01/08/2026 -> 31/08/2026) ==="
$cmd = $conn.CreateCommand()
$cmd.CommandText = "EXEC sp_Dashboard_GetData @TuNgay='2026-08-01 00:00:00', @DenNgay='2026-08-31 23:59:59', @TuNgayKyTruoc='2026-07-01 00:00:00', @DenNgayKyTruoc='2026-07-31 23:59:59'"
$adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$ds = New-Object System.Data.DataSet
$adapter.Fill($ds)

$summary = $ds.Tables[0].Rows[0]
Write-Host ("DoanhThu          : {0:N0}" -f $summary["DoanhThu"])
Write-Host ("LoiNhuan          : {0:N0}" -f $summary["LoiNhuan"])
Write-Host ("DoanhThuKyTruoc   : {0:N0}" -f $summary["DoanhThuKyTruoc"])
Write-Host ("LoiNhuanKyTruoc   : {0:N0}" -f $summary["LoiNhuanKyTruoc"])

# Query 2: Breakdown of Sales Documents in Aug 2026 (BAN_ChungTuBanHang)
Write-Host "`n=== 2. BAN_ChungTuBanHang DETAILS IN AUG 2026 ==="
$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = @"
SELECT bh.ID, bh.SoChungTu, bh.NgayChungTu, bh.TongCong,
       ISNULL(SUM(ct.ThanhTienVon), 0) AS SumThanhTienVon,
       ISNULL(SUM(ct.SoLuong * ct.DonGiaVon), 0) AS SumDonGiaVonCalc,
       ISNULL(SUM(ct.SoLuong * ap.AvgDonGia), 0) AS SumAvgDonGiaCalc
FROM BAN_ChungTuBanHang bh
LEFT JOIN BAN_ChungTuBanHang_ChiTiet ct ON bh.ID = ct.IDChungTuBanHang
OUTER APPLY (
    SELECT SUM(pn_ct.SoLuong * pn_ct.DonGia) / NULLIF(SUM(pn_ct.SoLuong), 0) AS AvgDonGia
    FROM KHO_PhieuNhap_ChiTiet pn_ct
    INNER JOIN KHO_PhieuNhap pn ON pn_ct.IDPhieuNhap = pn.ID
    INNER JOIN DM_KhoHang kh_pn ON pn.IDKho = kh_pn.ID AND ISNULL(kh_pn.IsKhoChinh, 0) = 1
    WHERE pn_ct.IDSanPham = ct.IDSanPham 
      AND pn.TrangThai = 2 AND pn.IsDeleted = 0
      AND pn.NgayNhap <= '2026-08-31 23:59:59'
) ap
WHERE bh.TrangThai = 2 AND bh.IsDeleted = 0 
  AND bh.NgayChungTu >= '2026-08-01 00:00:00' AND bh.NgayChungTu <= '2026-08-31 23:59:59'
GROUP BY bh.ID, bh.SoChungTu, bh.NgayChungTu, bh.TongCong
ORDER BY bh.TongCong DESC
"@

$adapter2 = New-Object System.Data.SqlClient.SqlDataAdapter($cmd2)
$dt2 = New-Object System.Data.DataTable
$adapter2.Fill($dt2)

foreach ($row in $dt2.Rows) {
    Write-Host ("CT: {0} | Ngay: {1:yyyy-MM-dd} | TongCong(Revenue): {2:N0} | SumThanhTienVon: {3:N0} | SumDonGiaVonCalc: {4:N0} | SumAvgDonGiaCalc: {5:N0}" -f $row["SoChungTu"], $row["NgayChungTu"], $row["TongCong"], $row["SumThanhTienVon"], $row["SumDonGiaVonCalc"], $row["SumAvgDonGiaCalc"])
}

# Query 3: Details of line items in BAN_ChungTuBanHang_ChiTiet causing high cost of goods sold
Write-Host "`n=== 3. TOP LINE ITEMS BY COST IN AUG 2026 ==="
$cmd3 = $conn.CreateCommand()
$cmd3.CommandText = @"
SELECT TOP 20 
    bh.SoChungTu, sp.MaSanPham, sp.TenSanPham, ct.SoLuong, ct.DonGia, ct.ThanhTien,
    ct.ThanhTienVon, ct.DonGiaVon, ap.AvgDonGia,
    CASE 
        WHEN ISNULL(ct.ThanhTienVon, 0) > 0 THEN ct.ThanhTienVon
        WHEN ISNULL(ct.DonGiaVon, 0) > 0 THEN ct.SoLuong * ct.DonGiaVon
        ELSE ct.SoLuong * ISNULL(ap.AvgDonGia, 0)
    END AS GiaVonDauRaCalculated
FROM BAN_ChungTuBanHang_ChiTiet ct
INNER JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
OUTER APPLY (
    SELECT SUM(pn_ct.SoLuong * pn_ct.DonGia) / NULLIF(SUM(pn_ct.SoLuong), 0) AS AvgDonGia
    FROM KHO_PhieuNhap_ChiTiet pn_ct
    INNER JOIN KHO_PhieuNhap pn ON pn_ct.IDPhieuNhap = pn.ID
    INNER JOIN DM_KhoHang kh_pn ON pn.IDKho = kh_pn.ID AND ISNULL(kh_pn.IsKhoChinh, 0) = 1
    WHERE pn_ct.IDSanPham = ct.IDSanPham 
      AND pn.TrangThai = 2 AND pn.IsDeleted = 0
      AND pn.NgayNhap <= '2026-08-31 23:59:59'
) ap
WHERE bh.TrangThai = 2 AND bh.IsDeleted = 0 
  AND bh.NgayChungTu >= '2026-08-01 00:00:00' AND bh.NgayChungTu <= '2026-08-31 23:59:59'
ORDER BY GiaVonDauRaCalculated DESC
"@

$adapter3 = New-Object System.Data.SqlClient.SqlDataAdapter($cmd3)
$dt3 = New-Object System.Data.DataTable
$adapter3.Fill($dt3)

foreach ($row in $dt3.Rows) {
    Write-Host ("CT: {0} | SP: {1} - {2} | SL: {3:N0} | DonGiaBan: {4:N0} | ThanhTienBan: {5:N0} | ThanhTienVon: {6:N0} | DonGiaVon: {7:N0} | AvgDonGia: {8:N0} | GiaVonCalc: {9:N0}" -f $row["SoChungTu"], $row["MaSanPham"], $row["TenSanPham"], $row["SoLuong"], $row["DonGia"], $row["ThanhTien"], $row["ThanhTienVon"], $row["DonGiaVon"], $row["AvgDonGia"], $row["GiaVonDauRaCalculated"])
}

$conn.Close()
