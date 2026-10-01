$connectionString = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Host "=== 1. DON HANG DH26000268 ==="
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT * FROM NS_DonDatHang WHERE SoDonHang = 'DH26000268'"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$da.Fill($dt) | Out-Null
$dt | Format-List

Write-Host "`n=== 2. BAN_ChungTuBanHang (ID = 273, 274 hoặc IDDonDatHang = 271) ==="
$cmd.CommandText = "SELECT * FROM BAN_ChungTuBanHang WHERE ID IN (273, 274) OR IDDonDatHang = 271"
$dt2 = New-Object System.Data.DataTable
$da.SelectCommand = $cmd
$da.Fill($dt2) | Out-Null
$dt2 | Format-List

$conn.Close()
