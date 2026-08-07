$sql = Get-Content -Path "SalesManagementSystem\App_Data\sp_CongNoKhachHang_ExportSP02.sql" -Raw
$c = New-Object System.Data.SqlClient.SqlConnection("Data Source=localhost;Initial Catalog=SalesWarehouseDB;Integrated Security=True;TrustServerCertificate=True")
$c.Open()
$cmd = $c.CreateCommand()
$cmd.CommandText = $sql
$cmd.ExecuteNonQuery() | Out-Null
Write-Host "Updated sp_CongNoKhachHang_ExportSP02 successfully!"
$c.Close()
