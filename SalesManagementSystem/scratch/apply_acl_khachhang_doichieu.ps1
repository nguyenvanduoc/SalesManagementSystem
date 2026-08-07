[System.Reflection.Assembly]::LoadFrom("c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll") | Out-Null

[System.Configuration.ConfigurationManager]::AppSettings["ConfigFile"] = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\system.dat"
[System.Configuration.ConfigurationManager]::AppSettings["KeyPart1"] = "VanDuoc@123123!"

$factory = New-Object SalesManagementSystem.Data.DbConnectionFactory
$conn = $factory.CreateConnection()

try {
    $conn.Open()
    $sqlPath = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\insert_acl_BaoCaoDoiChieuCongNoKhachHang.sql"
    $fullScript = [System.IO.File]::ReadAllText($sqlPath)
    
    $batches = [System.Text.RegularExpressions.Regex]::Split($fullScript, "^\s*GO\s*$", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Multiline)
    foreach ($batch in $batches) {
        $trimmed = $batch.Trim()
        if ($trimmed.Length -gt 0) {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $trimmed
            $cmd.ExecuteNonQuery() | Out-Null
        }
    }
    Write-Output "SUCCESS: Inserted ACL for BaoCaoDoiChieuCongNoKhachHang!"
} catch {
    Write-Error $_.Exception.Message
} finally {
    if ($conn -and $conn.State -eq [System.Data.ConnectionState]::Open) {
        $conn.Close()
    }
}
