$connectionString = "Server=localhost;Database=SalesManagementDB;Integrated Security=True;"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()

$command = $connection.CreateCommand()
$command.CommandText = "
SELECT 
    sp.MaSanPham,
    sp.TenSanPham,
    SUM(ct.SoLuong) AS TongSL,
    SUM(ct.ThanhTien) AS TongDoanhThu,
    SUM(ct.ThanhTienVon) AS TongThanhTienVon_BanHang,
    AVG(ct.DonGiaVon) AS AvgDonGiaVon_BanHang,
    AVG(ct.DonGia) AS AvgDonGia_BanHang,
    (
        SELECT SUM(pn_ct.SoLuong * pn_ct.DonGia) / NULLIF(SUM(pn_ct.SoLuong), 0)
        FROM KHO_PhieuNhap_ChiTiet pn_ct
        INNER JOIN KHO_PhieuNhap pn ON pn_ct.IDPhieuNhap = pn.ID
        INNER JOIN DM_KhoHang kh_pn ON pn.IDKho = kh_pn.ID AND kh_pn.IsKhoChinh = 1
        WHERE pn_ct.IDSanPham = ct.IDSanPham 
          AND pn.TrangThai = 2 AND pn.IsDeleted = 0
    ) AS AvgDonGiaNhap_KhoChinh
FROM BAN_ChungTuBanHang_ChiTiet ct
INNER JOIN BAN_ChungTuBanHang hd ON ct.IDChungTuBanHang = hd.ID
INNER JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
WHERE hd.IsDeleted = 0 AND hd.TrangThai = 2
GROUP BY ct.IDSanPham, sp.MaSanPham, sp.TenSanPham
ORDER BY TongDoanhThu DESC
"

$adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
$dataset = New-Object System.Data.DataSet
$adapter.Fill($dataset) | Out-Null

$dataset.Tables[0] | Format-Table -AutoSize

$connection.Close()
