$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=master;User ID=sa;Password=VanDuoc@123"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Output "--- Databases ---"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT name FROM sys.databases"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ($reader["name"])
}
$reader.Close()

$conn.Close()
