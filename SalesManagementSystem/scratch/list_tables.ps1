$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Output "--- Base Tables in SalesWarehouseDB ---"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ($reader["TABLE_SCHEMA"] + "." + $reader["TABLE_NAME"])
}
$reader.Close()

$conn.Close()
