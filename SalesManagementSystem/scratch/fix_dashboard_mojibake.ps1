$sqlPath = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\sp_Dashboard_GetData.sql"
$sqlContent = [System.IO.File]::ReadAllText($sqlPath, [System.Text.Encoding]::UTF8)

# Chuẩn bị script cho duocnguy_sa
$sql_duocnguy = $sqlContent -replace 'ALTER PROCEDURE sp_Dashboard_GetData', 'CREATE OR ALTER PROCEDURE duocnguy_sa.sp_Dashboard_GetData'

# Chuẩn bị script cho dbo
$sql_dbo = $sqlContent -replace 'ALTER PROCEDURE sp_Dashboard_GetData', 'CREATE OR ALTER PROCEDURE dbo.sp_Dashboard_GetData'

$connStr = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

Write-Host "1. Cập nhật duocnguy_sa.sp_Dashboard_GetData..."
$cmd1 = $conn.CreateCommand()
$cmd1.CommandText = $sql_duocnguy
$cmd1.ExecuteNonQuery() | Out-Null
Write-Host "   -> Thành công!"

Write-Host "2. Cập nhật dbo.sp_Dashboard_GetData..."
$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = $sql_dbo
$cmd2.ExecuteNonQuery() | Out-Null
Write-Host "   -> Thành công!"

Write-Host "3. Kiểm tra kết quả gọi sp_Dashboard_GetData..."
$cmdTest = $conn.CreateCommand()
$cmdTest.CommandText = "EXEC sp_Dashboard_GetData @TuNgay='2026-10-01', @DenNgay='2026-10-31', @TuNgayKyTruoc='2026-09-01', @DenNgayKyTruoc='2026-09-30'"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmdTest)
$ds = New-Object System.Data.DataSet
$da.Fill($ds) | Out-Null

Write-Host "Số bảng kết quả: $($ds.Tables.Count)"
# Bảng 13 (index 12) là HoatDongGanDay
if ($ds.Tables.Count -ge 13) {
    Write-Host "=== Bảng 13: HoatDongGanDay ==="
    $tableHD = $ds.Tables[12]
    foreach ($row in $tableHD.Rows) {
        $thoiGian = if ($row["ThoiGian"] -ne [DBNull]::Value) { ([DateTime]$row["ThoiGian"]).ToString("dd/MM/yyyy HH:mm") } else { "" }
        Write-Host "$thoiGian - [$($row['NguoiThucHien'])] : $($row['NoiDung'])"
    }
}

$conn.Close()
