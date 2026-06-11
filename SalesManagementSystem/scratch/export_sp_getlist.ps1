$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID('sp_KHO_PhieuNhap_GetList'))"
$definition = $cmd.ExecuteScalar()
if ($definition -eq [System.DBNull]::Value -or $definition -eq $null) {
    Write-Output "SP not found or has no definition"
} else {
    $outFile = "C:\Users\duoc0\.gemini\antigravity-ide\brain\a199250e-a8e4-4a0a-a1a0-6c4855877da8\scratch\current_sp_KHO_PhieuNhap_GetList.sql"
    [System.IO.File]::WriteAllText($outFile, $definition)
    Write-Output "Exported definition of sp_KHO_PhieuNhap_GetList to $outFile"
}
$conn.Close()
