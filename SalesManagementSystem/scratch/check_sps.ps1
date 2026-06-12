$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Output "--- Definition of sp_BAN_ChungTuBanHang_GetList ---"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID('sp_BAN_ChungTuBanHang_GetList')) AS Definition"
$reader = $cmd.ExecuteReader()
if ($reader.Read()) {
    Write-Output $reader["Definition"]
}
$reader.Close()

Write-Output "`n--- Definition of sp_KHO_PhieuXuat_GetList ---"
$cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID('sp_KHO_PhieuXuat_GetList')) AS Definition"
$reader = $cmd.ExecuteReader()
if ($reader.Read()) {
    Write-Output $reader["Definition"]
}
$reader.Close()

$conn.Close()
