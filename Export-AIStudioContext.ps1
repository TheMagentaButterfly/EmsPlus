# Export-AIStudioContext.ps1
$outputFile = ".\EmsPlus_Prompt_Context.txt"
if (Test-Path $outputFile) { Remove-Item $outputFile }

$csFiles = Get-ChildItem -Recurse -Filter "*.cs" | Where-Object { 
    $_.FullName -notmatch "\\obj\\" -and 
    $_.FullName -notmatch "\\bin\\" -and 
    $_.Name -ne "AssemblyInfo.cs" 
}

$totalFiles = 0
$builder = [System.Text.StringBuilder]::new()

foreach ($file in $csFiles) {
    $content = [System.IO.File]::ReadAllText($file.FullName)

    # 1. Replace giant CreateDefaultFile / Save default template bodies with a compact stub
    $content = [System.Text.RegularExpressions.Regex]::Replace(
        $content,
        '(void\s+CreateDefaultFile\s*\(\s*(?:string\s+\w+)?\s*\)\s*\{)[\s\S]*?(\n\s*\})',
        '$1 /* [Default XML/INI template stripped for AI context] */ $2'
    )

    # 2. Strip XML doc comments (/// ...) and block comments (/* ... */)
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, '///.*$', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, '/\*[\s\S]*?\*/', '')

    # 3. Collapse multiple blank lines into a single newline
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, '(?m)^\s*[\r\n]+', "`r`n")

    [void]$builder.AppendLine("--- START OF FILE $($file.Name) ---")
    [void]$builder.AppendLine($content.Trim())
    [void]$builder.AppendLine("--- END OF FILE $($file.Name) ---`r`n")
    $totalFiles++
}

[System.IO.File]::WriteAllText($outputFile, $builder.ToString())
Write-Host "Done! Packaged $totalFiles C# files into $outputFile." -ForegroundColor Green