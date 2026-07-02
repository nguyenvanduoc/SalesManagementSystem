$ErrorActionPreference = 'Stop'
Add-Type -Path 'bin\Newtonsoft.Json.dll'
Add-Type -Path 'bin\SalesManagementSystem.dll'
$x = [SalesManagementSystem.Models.ViewModels.DonDatHangChiTietViewModel]::new()
$x.DonGiaBocXep = 123.45
[Newtonsoft.Json.JsonConvert]::SerializeObject($x)
