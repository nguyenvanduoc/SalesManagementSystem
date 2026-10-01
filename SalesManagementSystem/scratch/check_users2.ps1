$connectionString = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%Nguoi%' OR TABLE_NAME LIKE '%User%' OR TABLE_NAME LIKE '%TaiKhoan%'"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$da.Fill($dt) | Out-Null
$dt | Format-Table -AutoSize

Write-Host "NS_NhanSu 7 and 11:"
$cmd.CommandText = "SELECT ID, MaNhanSu, HoDem, Ten, DienThoai, Email FROM NS_NhanSu WHERE ID IN (7, 11)"
$dt2 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt2) | Out-Null
$dt2 | Format-Table -AutoSize

$conn.Close()
