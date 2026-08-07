$sql = Get-Content -Path "SalesManagementSystem\App_Data\sp_Dashboard_GetData_ALTER.sql" -Raw
$servers = @(".", ".\SQLEXPRESS", "(localdb)\MSSQLLocalDB", "localhost")

foreach ($srv in $servers) {
    try {
        $c = New-Object System.Data.SqlClient.SqlConnection("Data Source=$srv;Initial Catalog=SalesWarehouseDB;Integrated Security=True;TrustServerCertificate=True")
        $c.Open()
        Write-Host "Connected to $srv"
        $cmd = $c.CreateCommand()
        $cmd.CommandText = $sql
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "Updated sp_Dashboard_GetData successfully!"
        $c.Close()
        exit 0
    } catch {
        Write-Host "Failed $srv : $_"
    }
}
