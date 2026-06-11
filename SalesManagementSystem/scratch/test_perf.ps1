$connectionString = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

$sw = [System.Diagnostics.Stopwatch]::StartNew()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "
DECLARE @Total INT;
EXEC [dbo].[sp_KHO_PhieuNhap_GetList] 
    @TuNgay = NULL,
    @DenNgay = NULL,
    @SoChungTu = NULL,
    @IDKho = NULL,
    @IDNhaCungCap = NULL,
    @TrangThai = NULL,
    @IDNhanSuNhan = NULL,
    @Offset = 0,
    @PageSize = 20,
    @TotalRecords = @Total OUTPUT;
SELECT @Total AS Total;
"
$reader = $cmd.ExecuteReader()
$count = 0
while ($reader.Read()) {
    $count++
}
if ($reader.NextResult()) {
    if ($reader.Read()) {
        $total = $reader.GetValue(0)
    }
}
$reader.Close()

$sw.Stop()
Write-Output "Executed in $($sw.ElapsedMilliseconds) ms. Retrieved $count rows, Total records: $total"

# Let's also retrieve the first few rows of data to check if NguoiTaoText and NgayTao are loaded!
$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = "
DECLARE @Total INT;
EXEC [dbo].[sp_KHO_PhieuNhap_GetList] 
    @TuNgay = NULL,
    @DenNgay = NULL,
    @SoChungTu = NULL,
    @IDKho = NULL,
    @IDNhaCungCap = NULL,
    @TrangThai = NULL,
    @IDNhanSuNhan = NULL,
    @Offset = 0,
    @PageSize = 5,
    @TotalRecords = @Total OUTPUT;
"
$reader2 = $cmd2.ExecuteReader()
while ($reader2.Read()) {
    $soChungTu = $reader2["SoChungTu"]
    $nguoiTaoText = $reader2["NguoiTaoText"]
    $ngayTao = $reader2["NgayTao"]
    Write-Output "SoChungTu: $soChungTu | NguoiTaoText: $nguoiTaoText | NgayTao: $ngayTao"
}
$reader2.Close()

$conn.Close()
