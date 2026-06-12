$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Output "--- Check DON_DonDatHang ---"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT OBJECT_ID('DON_DonDatHang') AS ID, (SELECT type_desc FROM sys.objects WHERE name = 'DON_DonDatHang') AS Type"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ("ID: " + $reader["ID"] + " | Type: " + $reader["Type"])
}
$reader.Close()

Write-Output "`n--- Check sys.synonyms or sys.views for DON ---"
$cmd.CommandText = "SELECT name, type_desc FROM sys.objects WHERE name LIKE '%DON%' OR name LIKE '%Don%'"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ("Name: " + $reader["name"] + " | Type: " + $reader["type_desc"])
}
$reader.Close()

$conn.Close()
