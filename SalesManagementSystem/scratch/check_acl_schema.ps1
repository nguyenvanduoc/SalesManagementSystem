[System.Reflection.Assembly]::LoadFrom("c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll") | Out-Null

[System.Configuration.ConfigurationManager]::AppSettings["ConfigFile"] = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\system.dat"
[System.Configuration.ConfigurationManager]::AppSettings["KeyPart1"] = "VanDuoc@123123!"

$factory = New-Object SalesManagementSystem.Data.DbConnectionFactory
$conn = $factory.CreateConnection()

try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ACL_Action'"
    $reader = $cmd.ExecuteReader()
    Write-Output "--- ACL_Action columns ---"
    while ($reader.Read()) {
        Write-Output $reader.GetString(0)
    }
    $reader.Close()

    $cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ACL_ManHinh'"
    $reader = $cmd.ExecuteReader()
    Write-Output "--- ACL_ManHinh columns ---"
    while ($reader.Read()) {
        Write-Output $reader.GetString(0)
    }
    $reader.Close()
} catch {
    Write-Error $_.Exception.Message
} finally {
    if ($conn -and $conn.State -eq [System.Data.ConnectionState]::Open) {
        $conn.Close()
    }
}
