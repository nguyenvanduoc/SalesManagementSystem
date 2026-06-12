$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Output "--- ACL_ManHinh matching ChungTu ---"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT * FROM ACL_ManHinh WHERE ID IN (SELECT IDManHinh FROM ACL_Action WHERE TenController = 'ChungTuBanHang' OR TenController = 'NhatKyChung')"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ("ID: " + $reader["ID"] + " | TenManHinh: " + $reader["TenManHinh"] + " | NhomCha: " + $reader["NhomChaManHinh"] + " | IsSuDung: " + $reader["IsSuDung"])
}
$reader.Close()

Write-Output "`n--- ACL_Action matching ChungTu / NhatKyChung ---"
$cmd.CommandText = "SELECT * FROM ACL_Action WHERE TenController = 'ChungTuBanHang' OR TenController = 'NhatKyChung'"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ("ID: " + $reader["ID"] + " | IDManHinh: " + $reader["IDManHinh"] + " | TenAction: " + $reader["TenAction"] + " | TenController: " + $reader["TenController"] + " | LoaiPhanQuyen: " + $reader["LoaiPhanQuyen"])
}
$reader.Close()

$conn.Close()
