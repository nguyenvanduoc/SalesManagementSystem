$webConfigPath = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Web.config"
$xml = [xml](Get-Content $webConfigPath)
$connStr = $xml.configuration.connectionStrings.add | Where-Object { $_.name -ne "LocalSqlServer" } | Select-Object -ExpandProperty connectionString -First 1

if ([string]::IsNullOrEmpty($connStr)) {
    $connStr = "Server=localhost;Database=SalesManagementSystem;Trusted_Connection=True;"
}

Write-Host "Using connection string: $connStr"

$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

Write-Host "--- sp_CongNo_PhaseTra_NCC_GetList ---"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "EXEC sp_helptext 'sp_CongNo_PhaseTra_NCC_GetList'"
try {
    $reader = $cmd.ExecuteReader()
    while($reader.Read()) {
        Write-Host -NoNewline $reader[0]
    }
    $reader.Close()
} catch {
    Write-Host "Error getting sp_CongNo_PhaseTra_NCC_GetList: $_"
}

Write-Host "`n--- sp_Dashboard_GetData ---"
$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = "EXEC sp_helptext 'sp_Dashboard_GetData'"
try {
    $reader2 = $cmd2.ExecuteReader()
    while($reader2.Read()) {
        Write-Host -NoNewline $reader2[0]
    }
    $reader2.Close()
} catch {
    Write-Host "Error getting sp_Dashboard_GetData: $_"
}

$conn.Close()
