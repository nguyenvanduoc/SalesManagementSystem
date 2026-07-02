$ErrorActionPreference = 'Stop'
Add-Type -Path 'bin\SalesManagementSystem.dll'
Add-Type -Path 'bin\Dapper.dll'
$conn = [SalesManagementSystem.Data.DbConnectionFactory]::new().CreateConnection()
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TOP 5 IDDonDatHang, IDSanPham, SoLuong, DonGia, DonGiaBocXep, ThanhTienBocXep FROM NS_DonDatHangChiTiet ORDER BY ID DESC"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host "Don=$($reader['IDDonDatHang']), SP=$($reader['IDSanPham']), DGBX=$($reader['DonGiaBocXep'])"
}
$conn.Close()
