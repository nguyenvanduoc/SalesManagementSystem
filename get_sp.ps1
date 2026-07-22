$conn = New-Object System.Data.SqlClient.SqlConnection('Server=(localdb)\MSSQLLocalDB;Database=SalesManagementSystem;Integrated Security=True;')
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID('sp_KT_PhieuChi_GetList'))"
$res = $cmd.ExecuteScalar()
[IO.File]::WriteAllText('GetListSP.sql', $res)
$conn.Close()
