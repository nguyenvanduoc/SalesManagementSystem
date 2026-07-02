$ErrorActionPreference = 'Stop'
Add-Type -Path 'bin\Newtonsoft.Json.dll'
Add-Type -Path 'bin\SalesManagementSystem.dll'
$json = '[{"donGiaBocXep": 123.45}]'
$list = [Newtonsoft.Json.JsonConvert]::DeserializeObject[System.Collections.Generic.List[SalesManagementSystem.Models.ViewModels.DonDatHangChiTietViewModel]]($json)
$list[0].DonGiaBocXep
