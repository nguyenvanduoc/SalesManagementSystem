[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
$wc = New-Object System.Net.WebClient
try {
    $r1 = $wc.DownloadString('https://localhost:44326/run_insert.aspx')
    Write-Host "Insert: $r1"
} catch {
    Write-Host "Insert ERROR: $($_.Exception.Message)"
}

try {
    $r2 = $wc.DownloadString('https://localhost:44326/run_sp.aspx')
    Write-Host "SP: $r2"
} catch {
    Write-Host "SP ERROR: $($_.Exception.Message)"
}
