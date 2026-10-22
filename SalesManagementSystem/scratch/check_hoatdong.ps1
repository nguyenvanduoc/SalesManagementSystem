$connStr = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

# 1. Chạy thử phần HoatDongGanDay trực tiếp từ sp_Dashboard_GetData
$cmd = $conn.CreateCommand()
$cmd.CommandText = "EXEC sp_Dashboard_GetData @Thang=10, @Nam=2026"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$ds = New-Object System.Data.DataSet
$da.Fill($ds) | Out-Null

Write-Host "Tổng số bảng kết quả trả về: $($ds.Tables.Count)"
# Bảng thứ 13 (index 12) là HoatDongGanDay
if ($ds.Tables.Count -ge 13) {
    Write-Host "=== Bảng 13: HoatDongGanDay ==="
    $ds.Tables[12] | Format-Table -AutoSize | Out-String | Write-Host
}

# 2. Xem definition của sp_Dashboard_GetData trong CSDL quanh đoạn HoatDongGanDay
$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = "SELECT m.definition FROM sys.sql_modules m INNER JOIN sys.objects o ON m.object_id = o.object_id WHERE o.name = 'sp_Dashboard_GetData'"
$def = $cmd2.ExecuteScalar()

$idx = $def.IndexOf("HoatDongGanDay")
if ($idx -gt 0) {
    Write-Host "=== Definition đoạn HoatDongGanDay ==="
    Write-Host $def.Substring($idx, [Math]::Min(1000, $def.Length - $idx))
}

$conn.Close()
