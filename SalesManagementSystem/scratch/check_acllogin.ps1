$connectionString = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT l.ID, l.Username, l.IDNhanSu, ns.HoDem, ns.Ten FROM AclLogin l LEFT JOIN NS_NhanSu ns ON l.IDNhanSu = ns.ID WHERE l.IDNhanSu IN (7, 11) OR l.ID IN (7, 11)"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$da.Fill($dt) | Out-Null
$dt | Format-Table -AutoSize

$conn.Close()
