$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT name FROM sys.procedures WHERE name LIKE '%sp_KHO_PhieuNhap%'"
$r = $cmd.ExecuteReader()
while ($r.Read()) {
    Write-Output $r.GetValue(0)
}
$r.Close()
$conn.Close()
