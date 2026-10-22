$connStr = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT s.name AS SchemaName, o.name AS ObjectName, o.object_id, o.create_date, o.modify_date
FROM sys.objects o
INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
WHERE o.name = 'sp_Dashboard_GetData'
"@
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$da.Fill($dt) | Out-Null
$dt | Format-Table -AutoSize | Out-String | Write-Host

foreach ($row in $dt.Rows) {
    Write-Host "=== Schema: $($row['SchemaName']), ModifyDate: $($row['modify_date']) ==="
    $cmd2 = $conn.CreateCommand()
    $cmd2.CommandText = "SELECT definition FROM sys.sql_modules WHERE object_id = $($row['object_id'])"
    $def = $cmd2.ExecuteScalar()
    Write-Host "Length: $($def.Length)"
    if ($def.Contains("HoatDongGanDay") -or $def.Contains("HoatDong") -or $def.Contains("NoiDung")) {
        Write-Host "Contains HoatDong/NoiDung!"
        $idx = $def.IndexOf("NoiDung")
        if ($idx -gt 0) {
            Write-Host $def.Substring([Math]::Max(0, $idx - 100), [Math]::Min(500, $def.Length - $idx + 100))
        }
    }
}

$conn.Close()
