$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123"
$sqlPath = "c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Alter_sp_KHO_PhieuNhap_GetList.sql"

$sqlText = [System.IO.File]::ReadAllText($sqlPath)
# Split script by "GO" batches
$batches = [System.Text.RegularExpressions.Regex]::Split($sqlText, "^\s*GO\s*$", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Multiline)

$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

foreach ($batch in $batches) {
    $cleanBatch = $batch.Trim()
    if ($cleanBatch -ne "") {
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $cleanBatch
        try {
            $cmd.ExecuteNonQuery() > $null
            Write-Output "Successfully executed batch starting with: $( $cleanBatch.Substring(0, [Math]::Min(50, $cleanBatch.Length)) )"
        } catch {
            Write-Output "Error executing batch: $_"
            Write-Output "Batch content:`r`n$cleanBatch`r`n"
        }
    }
}

$conn.Close()
