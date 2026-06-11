$vsPath = "C:\Program Files\Microsoft Visual Studio"
$msbuild = Get-ChildItem -Path $vsPath -Filter "msbuild.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
if ($msbuild) {
    Write-Host "Found MSBuild at: $($msbuild.FullName)"
    & $msbuild.FullName SalesManagementSystem.csproj /t:Build /p:Configuration=Debug
} else {
    Write-Error "MSBuild.exe was not found in $vsPath"
}
