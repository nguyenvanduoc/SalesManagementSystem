$connStr = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT m.definition FROM sys.sql_modules m INNER JOIN sys.objects o ON m.object_id = o.object_id WHERE o.name = 'sp_Dashboard_GetData'"
$def = $cmd.ExecuteScalar()
[System.IO.File]::WriteAllText("$PSScriptRoot/current_sp_dashboard.sql", $def, [System.Text.Encoding]::UTF8)

Write-Host "Length: $($def.Length)"
$conn.Close()
