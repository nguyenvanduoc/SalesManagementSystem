$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT MatKhau FROM ACL_Login WHERE TenDangNhap = 'a'"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ("MD5 Hash: " + $reader["MatKhau"])
}
$reader.Close()

$conn.Close()
