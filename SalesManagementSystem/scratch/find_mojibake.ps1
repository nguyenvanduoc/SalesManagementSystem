$connStr = "Data Source=125.212.224.205;Initial Catalog=duocnguy_SalesWarehouseDB;User ID=duocnguy_sa;Password=SGcl_123Zo@!;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

# 1. Tìm tất cả các procedure có tên chứa Dashboard
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT name FROM sys.objects WHERE type = 'P' AND name LIKE '%Dashboard%'"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$da.Fill($dt) | Out-Null
$dt | Format-Table -AutoSize | Out-String | Write-Host

# 2. Tìm tất cả các procedure chứa chuỗi 'Ghi s' hoặc 'Há»‡' hoặc 'HoatDongGanDay'
$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = @"
SELECT o.name, m.definition 
FROM sys.sql_modules m 
INNER JOIN sys.objects o ON m.object_id = o.object_id 
WHERE m.definition LIKE N'%Ghi s%' OR m.definition LIKE N'%Há»‡%' OR m.definition LIKE N'%Táo¡o%'
"@
$da2 = New-Object System.Data.SqlClient.SqlDataAdapter($cmd2)
$dt2 = New-Object System.Data.DataTable
$da2.Fill($dt2) | Out-Null
foreach ($row in $dt2.Rows) {
    Write-Host "Found procedure with mojibake: $($row['name'])"
}

$conn.Close()
