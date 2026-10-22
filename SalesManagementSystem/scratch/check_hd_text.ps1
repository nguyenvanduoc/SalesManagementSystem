$connStr = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

# Lấy definition chính xác
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT m.definition FROM sys.sql_modules m INNER JOIN sys.objects o ON m.object_id = o.object_id WHERE o.name = 'sp_Dashboard_GetData'"
$def = $cmd.ExecuteScalar()

$idx = $def.IndexOf("HoatDongGanDay")
if ($idx -gt 0) {
    $sub = $def.Substring($idx, [Math]::Min(1500, $def.Length - $idx))
    [System.IO.File]::WriteAllText("$PSScriptRoot/def_hoatdong.txt", $sub, [System.Text.Encoding]::UTF8)
    Write-Host "Da ghi def_hoatdong.txt"
}

# Chạy thử query HoatDongGanDay trực tiếp bằng ADO.NET
$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = @"
    SELECT TOP 5 * FROM (
        SELECT NgayTao AS ThoiGian, 
               ISNULL((SELECT LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) FROM ACL_Login WHERE ID = NguoiTao), N'Hệ thống') AS NguoiThucHien,
               N'Tạo đơn hàng ' + SoDonHang AS NoiDung, 
               'TaoDonHang' AS LoaiHoatDong
        FROM NS_DonDatHang
        UNION ALL
        SELECT NgayGhiSo, 
               ISNULL((SELECT LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) FROM ACL_Login WHERE ID = NguoiGhiSo), N'Hệ thống') AS NguoiThucHien,
               N'Ghi sổ nhập kho ' + SoChungTu AS NoiDung, 
               'NhapKho' AS LoaiHoatDong
        FROM KHO_PhieuNhap WHERE IsDeleted = 0 AND TrangThai = 2
    ) t
    WHERE ThoiGian IS NOT NULL
    ORDER BY ThoiGian DESC;
"@
$da2 = New-Object System.Data.SqlClient.SqlDataAdapter($cmd2)
$dt2 = New-Object System.Data.DataTable
$da2.Fill($dt2) | Out-Null

foreach ($row in $dt2.Rows) {
    Write-Host "NguoiThucHien: $($row['NguoiThucHien']) | NoiDung: $($row['NoiDung'])"
}

$conn.Close()
