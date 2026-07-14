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
    
    Write-Output "--- Query PT000003 in Database ---"
    $cmd.CommandText = @"
        SELECT ID, SoPhieuThu, SoTienThu, TrangThai
        FROM KT_PhieuThu
        WHERE SoPhieuThu = 'PT000003'
"@
    $reader = $cmd.ExecuteReader()
    $id = $null
    if ($reader.Read()) {
        Write-Output ("ID: {0} | SoPhieuThu: {1} | SoTienThu: {2} | TrangThai: {3}" -f $reader[0], $reader[1], $reader[2], $reader[3])
        $id = $reader[0]
    }
    $reader.Close()

    if ($id) {
        Write-Output "--- Query KT_PhieuThuChiTiet for ID = $id ---"
        $cmd.CommandText = "SELECT ID, IDChungTuBanHang, LoaiThu, SoTienPhanBo FROM KT_PhieuThuChiTiet WHERE IDPhieuThu = $id"
        $reader = $cmd.ExecuteReader()
        while ($reader.Read()) {
            Write-Output ("ID: {0} | IDChungTuBanHang: {1} | LoaiThu: {2} | SoTienPhanBo: {3}" -f $reader[0], $reader[1], $reader[2], $reader[3])
        }
        $reader.Close()
    }

} catch {
    Write-Error $_.Exception.Message
} finally {
    $conn.Close()
}
