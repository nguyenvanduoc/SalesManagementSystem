[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
$wc = New-Object System.Net.WebClient
try {
    $r1 = $wc.DownloadString('https://localhost:44326/run_sql.aspx')
    Write-Host "Result: $r1"
} catch {
    Write-Host "ERROR: $($_.Exception.Message)"
}
