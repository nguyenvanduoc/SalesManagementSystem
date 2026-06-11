$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Output "--- KHO_PhieuNhap ---"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT ID, SoChungTu, NguoiTao, NguoiCapNhat FROM KHO_PhieuNhap"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output "ID: $($reader["ID"]) | SoChungTu: $($reader["SoChungTu"]) | NguoiTao: $($reader["NguoiTao"]) | NguoiCapNhat: $($reader["NguoiCapNhat"])"
}
$reader.Close()

Write-Output "`n--- ACL_Login ---"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT ID, IDNhanSu, TenDangNhap, HoDem, Ten FROM ACL_Login"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output "ID (userid): $($reader["ID"]) | IDNhanSu: $($reader["IDNhanSu"]) | TenDangNhap: $($reader["TenDangNhap"]) | Name: $($reader["HoDem"]) $($reader["Ten"])"
}
$reader.Close()

Write-Output "`n--- NS_NhanSu ---"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT ID, HoDem, Ten FROM NS_NhanSu"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output "ID (IDNhanSu): $($reader["ID"]) | Name: $($reader["HoDem"]) $($reader["Ten"])"
}
$reader.Close()

$conn.Close()
