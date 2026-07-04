$files = Get-ChildItem -Path C:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Views -Filter *.cshtml -Recurse

foreach ($f in $files) {
    $content = Get-Content $f.FullName -Raw
    
    # We look for lines containing '.btn-reset' and 'click' and 'on('
    # Then we find the start of the function block and match brackets
    
    $lines = $content -split "?
"
    $newLines = @()
    $inResetBlock = $false
    $bracketDepth = 0
    
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i]
        
        if ($inResetBlock) {
            # Count braces in this line
            $bracketDepth += ($line.Split('{').Length - 1)
            $bracketDepth -= ($line.Split('}').Length - 1)
            
            if ($bracketDepth -le 0) {
                $inResetBlock = $false
            }
            continue
        }
        
        if ($line -match "\.on\s*\(\s*['"]click.*?\.btn-reset" -or $line -match "\.click\s*\(\s*function" -and $line -match "btnReset") {
            $inResetBlock = $true
            $bracketDepth = ($line.Split('{').Length - 1) - ($line.Split('}').Length - 1)
            if ($bracketDepth -le 0) {
                $inResetBlock = $false # Single line case
            }
            
            # also remove previous line if it is a .off for the same event
            if ($newLines.Length -gt 0 -and $newLines[-1] -match "\.off\s*\(\s*['"]click.*\.btn-reset") {
                $newLines = $newLines[0..($newLines.Length-2)]
            }
            continue
        }
        
        $newLines += $line
    }
    
    $newContent = $newLines -join "
"
    if ($newContent -ne $content) {
        Set-Content -Path $f.FullName -Value $newContent
        Write-Output "Cleaned $($f.Name)"
    }
}
