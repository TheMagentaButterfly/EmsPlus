Write-Host "Scanning EmsPlus source code for Localization strings..." -ForegroundColor Cyan

# The Regex pattern to find: Localization.Get("KEY", "DefaultText")
$regex = [regex]'Localization\.Get(?:Format)?\(\s*"([^"]+)"\s*,\s*[\$@]*"([^"\\]*(?:\\.[^"\\]*)*)"'

$keys = @{}
$csFiles = Get-ChildItem -Path ".\" -Filter *.cs -Recurse

foreach ($file in $csFiles) {
    # Read the whole file as a single string
    $content = Get-Content $file.FullName -Raw
    
    if (![string]::IsNullOrWhiteSpace($content)) {
        # Find all matches in the file
        $matches = $regex.Matches($content)
        
        foreach ($match in $matches) {
            $key = $match.Groups[1].Value
            $value = $match.Groups[2].Value
            
            # Add to dictionary (prevents duplicates)
            if (-not $keys.ContainsKey($key)) {
                $keys[$key] = $value
            }
        }
    }
}

# Define output files
$outputIni = ".\English_Master.ini"
$outputCSharp = ".\Localization_CSharp_Block.txt"

# 1. Setup the INI file content
$iniContent = @()
$iniContent += "; ========================================================="
$iniContent += "; EmsPlus Localization File"
$iniContent += "; English"
$iniContent += "; ========================================================="
$iniContent += ""

# 2. Setup the C# block content
$csharpContent = @()

# Sort alphabetically and populate both files
foreach ($key in ($keys.Keys | Sort-Object)) {
    $val = $keys[$key]
    
    # Add to INI
    $iniContent += "$key=$val"
    
    # Add to C# Block format (escaping quotes so it writes w.WriteLine("KEY=Value");)
    $csharpContent += "w.WriteLine(`"$key=$val`");"
}

# Save to files
$iniContent | Set-Content $outputIni -Encoding UTF8
$csharpContent | Set-Content $outputCSharp -Encoding UTF8

Write-Host "Success! Found $($keys.Count) unique translation keys." -ForegroundColor Green
Write-Host "Saved INI to: $outputIni" -ForegroundColor Yellow
Write-Host "Saved C# to:  $outputCSharp" -ForegroundColor Yellow
Pause