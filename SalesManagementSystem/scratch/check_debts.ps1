# Load the web application assembly to use its decryption and config logic
[System.Reflection.Assembly]::LoadFrom("c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll") | Out-Null

# Set the configuration keys in memory for ConfigurationManager to read
[System.Configuration.ConfigurationManager]::AppSettings["ConfigFile"] = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\system.dat"
[System.Configuration.ConfigurationManager]::AppSettings["KeyPart1"] = "VanDuoc@123123!"

$factory = New-Object SalesManagementSystem.Data.DbConnectionFactory
$conn = $factory.CreateConnection()

try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    
    Write-Output "--- Test STRING_AGG in SQL Server ---"
    $cmd.CommandText = @"
        SELECT 
            pn.ID,
            pn.SoChungTu,
            (
                SELECT STRING_AGG(pc.SoPhieuChi, ', ')
                FROM KT_PhieuChiChiTiet ct
                INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                WHERE ct.IDPhieuNhap = pn.ID
                  AND ct.LoaiChi = 1
                  AND pc.IsDeleted = 0
                  AND pc.TrangThai = 2
                  AND pc.IDPhuongTien IS NOT NULL
            ) AS SoPhieuChiList
        FROM KHO_PhieuNhap pn
        WHERE pn.ID IN (19, 20, 30)
"@
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Output ("ID: {0} | So: {1} | PhieuChiList: {2}" -f $reader[0], $reader[1], $reader[2])
    }
    $reader.Close()

} catch {
    Write-Error $_.Exception.Message
} finally {
    $conn.Close()
}
