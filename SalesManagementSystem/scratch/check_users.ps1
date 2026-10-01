$connectionString = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT ns.ID, ns.MaNhanSu, ns.HoDem + ' ' + ns.Ten as HoTen, u.TenDangNhap, u.Email
FROM NS_NhanSu ns
LEFT JOIN HT_NguoiDung u ON ns.ID = u.IDNhanSu
WHERE ns.ID IN (7, 11)
"@
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$da.Fill($dt) | Out-Null
$dt | Format-Table -AutoSize

$conn.Close()
