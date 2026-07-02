$ErrorActionPreference = 'Stop'
Add-Type -Path 'bin\SalesManagementSystem.dll'
Add-Type -Path 'bin\Dapper.dll'
$conn = [SalesManagementSystem.Data.DbConnectionFactory]::new().CreateConnection()
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT name, OBJECT_DEFINITION(object_id) AS definition FROM sys.triggers WHERE parent_id = OBJECT_ID('NS_DonDatHangChiTiet') OR parent_id = OBJECT_ID('NS_DonDatHang')"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host "Trigger: $($reader['name'])"
    Write-Host "$($reader['definition'])"
}
$conn.Close()
