$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

Write-Output "--- ACL_ManHinh matching ---"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT ID, TenManHinh, NhomChaManHinh, IsSuDung, STT FROM ACL_ManHinh WHERE TenManHinh LIKE N'%Chứng từ%' OR TenManHinh LIKE N'%Chung tu%' OR TenManHinh LIKE N'%bán hàng%'"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ("ID: " + $reader["ID"] + " | TenManHinh: " + $reader["TenManHinh"] + " | NhomCha: " + $reader["NhomChaManHinh"] + " | IsSuDung: " + $reader["IsSuDung"] + " | STT: " + $reader["STT"])
}
$reader.Close()

Write-Output "`n--- ACL_Action matching ---"
$cmd.CommandText = "SELECT ID, IDManHinh, TenAction, TenController, LoaiPhanQuyen FROM ACL_Action WHERE IDManHinh IN (SELECT ID FROM ACL_ManHinh WHERE TenManHinh LIKE N'%Chứng từ%' OR TenManHinh LIKE N'%Chung tu%' OR TenManHinh LIKE N'%bán hàng%')"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ("ID: " + $reader["ID"] + " | IDManHinh: " + $reader["IDManHinh"] + " | TenAction: " + $reader["TenAction"] + " | TenController: " + $reader["TenController"] + " | LoaiPhanQuyen: " + $reader["LoaiPhanQuyen"])
}
$reader.Close()

Write-Output "`n--- Menu Query Logic from MenuRepository ---"
$cmd.CommandText = @"
SELECT
    m.ID            AS IDManHinh,
    m.TenManHinh,
    m.NhomChaManHinh,
    ISNULL(a.TenController, '')  AS TenController,
    ISNULL(a.TenAction, 'Index') AS TenAction
FROM ACL_ManHinh m
LEFT JOIN ACL_Action a
    ON a.ID = (
        SELECT TOP 1 ID
        FROM ACL_Action
        WHERE IDManHinh = m.ID
        ORDER BY LoaiPhanQuyen ASC, ID ASC
    )
WHERE m.IsSuDung = 1
ORDER BY m.STT, m.NhomChaManHinh, m.ID
"@
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ("TenManHinh: " + $reader["TenManHinh"] + " | Controller: " + $reader["TenController"] + " | Action: " + $reader["TenAction"])
}
$reader.Close()

$conn.Close()
