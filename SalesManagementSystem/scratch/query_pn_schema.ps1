$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'KHO_PhieuNhap'
ORDER BY ORDINAL_POSITION
"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ($reader.GetValue(0) + " | " + $reader.GetValue(1) + " (" + $reader.GetValue(2) + ")")
}
$reader.Close()
$conn.Close()
