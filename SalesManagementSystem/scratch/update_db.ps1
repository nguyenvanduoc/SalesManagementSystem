$sql = Get-Content -Path "SQL_Scripts\BAN_TraHangBan.sql" -Raw
$servers = @(".", ".\SQLEXPRESS", "(localdb)\MSSQLLocalDB", "localhost")

foreach ($srv in $servers) {
    try {
        $c = New-Object System.Data.SqlClient.SqlConnection("Data Source=$srv;Initial Catalog=SalesWarehouseDB;Integrated Security=True;TrustServerCertificate=True")
        $c.Open()
        Write-Host "Connected to $srv"
        $batches = $sql -split "(?i)\r?\nGO\r?\n"
        foreach ($b in $batches) {
            if ($b.Trim()) {
                $cmd = $c.CreateCommand()
                $cmd.CommandText = $b
                $cmd.ExecuteNonQuery() | Out-Null
            }
        }
        Write-Host "Stored procedures updated successfully!"
        $c.Close()
        exit 0
    } catch {
        Write-Host "Failed on $srv : $($_.Exception.Message)"
    }
}
