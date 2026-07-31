[System.Reflection.Assembly]::LoadFrom("c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll") | Out-Null

[System.Configuration.ConfigurationManager]::AppSettings["ConfigFile"] = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\system.dat"
[System.Configuration.ConfigurationManager]::AppSettings["KeyPart1"] = "VanDuoc@123123!"

$factory = New-Object SalesManagementSystem.Data.DbConnectionFactory
$conn = $factory.CreateConnection()

try {
    $conn.Open()
    $sqlPath = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\create_sp_CongNoKhachHang.sql"
    $script = [System.IO.File]::ReadAllText($sqlPath)
    
    $batches = $script -split "(?m)^GO\s*$"
    
    foreach ($batch in $batches) {
        $cmdText = $batch.Trim()
        if (-not [string]::IsNullOrWhiteSpace($cmdText)) {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $cmdText
            $cmd.ExecuteNonQuery() | Out-Null
        }
    }
    Write-Output "SUCCESS: Applied sp_CongNoKhachHang_GetList procedure!"
} catch {
    Write-Error $_.Exception.Message
} finally {
    if ($conn -and $conn.State -eq [System.Data.ConnectionState]::Open) {
        $conn.Close()
    }
}
