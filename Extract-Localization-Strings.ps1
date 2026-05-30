Write-Host "Scanning EmsPlus source code for Localization strings..." -ForegroundColor Cyan

$regex = [regex]'Localization\.Get(?:Format)?\(\s*"([^"]+)"\s*,\s*[\$@]*"([^"\\]*(?:\\.[^"\\]*)*)"'

$keys = @{}
$csFiles = Get-ChildItem -Path ".\" -Filter *.cs -Recurse

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    
    if (![string]::IsNullOrWhiteSpace($content)) {
        $matches = $regex.Matches($content)
        
        foreach ($match in $matches) {
            $key = $match.Groups[1].Value
            $value = $match.Groups[2].Value
            
            if (-not $keys.ContainsKey($key)) {
                $keys[$key] = $value
            }
        }
    }
}

$outputIni = ".\English.ini"
$outputCSharp = ".\Localization_CSharp_Block.txt"

$iniContent = @()
$iniContent += "; ========================================================="
$iniContent += "; EmsPlus Localization File"
$iniContent += "; English"
$iniContent += "; ========================================================="
$iniContent += ""

$iniContent += "STATUS_AVAILABLE=~g~Available"
$iniContent += "STATUS_AVAILABLEATSTATION=~g~Available at Station"
$iniContent += "STATUS_ENROUTE=~r~En Route"
$iniContent += "STATUS_ONSCENE=~r~On Scene"
$iniContent += "STATUS_REQUESTTOSPEAK=~r~Request to speak"
$iniContent += "STATUS_OFFDUTY=~u~Off Duty"
$iniContent += "STATUS_TRANSPORTING=~r~Transporting"
$iniContent += "STATUS_ATDESTINATION=~y~At Destination"
$iniContent += "STATUS_BUSY=~m~Busy"
$iniContent += "STATUS_URGENTREQUESTTOSPEAK=~h~~p~Urgent request to speak"
$iniContent += "STATUS_EMERGENCY=~h~~p~Emergency"
$iniContent += ""
$iniContent += "CONSC_PAIN=Pain"
$iniContent += "CONSC_ALERT=Alert"
$iniContent += "CONSC_VERBAL=Verbal"
$iniContent += "CONSC_UNRESPONSIVE=Unresponsive"
$iniContent += ""

$csharpContent = @()

foreach ($key in ($keys.Keys | Sort-Object)) {
    $val = $keys[$key]
    
    $iniContent += "$key=$val"
    
    $csharpContent += "w.WriteLine(`"$key=$val`");"
}

$iniContent | Set-Content $outputIni -Encoding UTF8
$csharpContent | Set-Content $outputCSharp -Encoding UTF8

Write-Host "Success! Found $($keys.Count) unique translation keys." -ForegroundColor Green
Write-Host "Saved INI to: $outputIni" -ForegroundColor Yellow
Write-Host "Saved C# to:  $outputCSharp" -ForegroundColor Yellow
Pause