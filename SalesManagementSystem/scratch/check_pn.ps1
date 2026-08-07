$c = New-Object System.Data.SqlClient.SqlConnection("Data Source=localhost;Initial Catalog=SalesWarehouseDB;Integrated Security=True;TrustServerCertificate=True")
$c.Open()

Write-Host "=== DM_SanPham Columns ==="
$cmd = $c.CreateCommand()
$cmd.CommandText = "SELECT TOP 1 * FROM DM_SanPham"
$r = $cmd.ExecuteReader()
if ($r.Read()) {
    for ($i=0; $i -lt $r.FieldCount; $i++) {
        Write-Host ($r.GetName($i) + ": " + $r.GetValue($i))
    }
}
$r.Close()

$c.Close()
