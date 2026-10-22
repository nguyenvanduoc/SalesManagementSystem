[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$connStr = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "EXEC sp_Dashboard_GetData @TuNgay='2026-10-01', @DenNgay='2026-10-31', @TuNgayKyTruoc='2026-09-01', @DenNgayKyTruoc='2026-09-30'"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$ds = New-Object System.Data.DataSet
$da.Fill($ds) | Out-Null

$tableHD = $ds.Tables[12]
$out = ""
foreach ($row in $tableHD.Rows) {
    $thoiGian = if ($row["ThoiGian"] -ne [DBNull]::Value) { ([DateTime]$row["ThoiGian"]).ToString("dd/MM/yyyy HH:mm") } else { "" }
    $line = "$thoiGian - [$($row['NguoiThucHien'])] : $($row['NoiDung'])"
    $out += $line + "`r`n"
}

[System.IO.File]::WriteAllText("$PSScriptRoot/hoatdong_result.txt", $out, [System.Text.Encoding]::UTF8)
Write-Host "Da luu vao hoatdong_result.txt"
$conn.Close()
