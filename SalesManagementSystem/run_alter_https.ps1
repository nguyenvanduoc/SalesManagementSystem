[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
$wc = New-Object System.Net.WebClient
try {
    $r = $wc.DownloadString('https://localhost:44326/run_alter.aspx')
    Write-Host "Alter Schema: $r"
} catch {
    Write-Host "Alter ERROR: $($_.Exception.Message)"
}
