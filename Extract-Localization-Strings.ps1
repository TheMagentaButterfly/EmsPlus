Write-Host "Scanning EmsPlus source code for Localization strings..." -ForegroundColor Cyan

# Matches:
# Localization.Get("KEY", "Backup")
# Localization.Get(@"KEY", @"Backup")
# Localization.Get($"KEY", $"Backup")
# Localization.GetFormat(...)
$regex = [regex]'Localization\.Get(?:Format)?\(\s*"([^"]+)"\s*,\s*[\$@]*"([^"\\]*(?:\\.[^"\\]*)*)"'

# Matches any Localization.Get/GetFormat call so we can detect missing backup text
$missingBackupRegex = [regex]'Localization\.Get(?:Format)?\(\s*"([^"]+)"\s*(?:,|\))'

$keys = @{}
$csFiles = Get-ChildItem -Path ".\" -Filter *.cs -Recurse

foreach ($file in $csFiles) {

    $content = Get-Content $file.FullName -Raw

    if ([string]::IsNullOrWhiteSpace($content)) {
        continue
    }

    # Find all localization entries that have backup text
    $matches = $regex.Matches($content)

    foreach ($match in $matches) {
        $key = $match.Groups[1].Value
        $value = $match.Groups[2].Value

        if (-not $keys.ContainsKey($key)) {
            $keys[$key] = $value
        }
    }

    # Warn about missing backup text (includes line numbers)
    $lines = Get-Content $file.FullName

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]

        $match = $missingBackupRegex.Match($line)

        if ($match.Success -and $line -notmatch ',\s*[\$@]*"') {
            Write-Host ("WARNING: Missing backup text for localization key '{0}' in {1}:{2}" -f `
                $match.Groups[1].Value,
                $file.FullName,
                ($i + 1)
            ) -ForegroundColor Yellow
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

Write-Host ""
Write-Host "Success! Found $($keys.Count) unique translation keys." -ForegroundColor Green
Write-Host "Saved INI to: $outputIni" -ForegroundColor Yellow
Write-Host "Saved C# to:  $outputCSharp" -ForegroundColor Yellow

Pause