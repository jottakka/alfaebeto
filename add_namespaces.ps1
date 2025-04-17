# === CONFIGURATION ===
# Change this to match your base/root namespace
$baseNamespace = "AlfaEBetto"

# Path to the root of your C# project
$root = "D:\git\alfaebeto\alfaebeto"

# ======================

$root = [System.IO.Path]::GetFullPath($root)

Get-ChildItem -Path $root -Recurse -Filter *.cs | Where-Object {
    ($_ | Select-String -Pattern '\bclass\b') -and
    -not ($_ | Select-String -Pattern '^\s*namespace\b')
} | ForEach-Object {
    $file = $_.FullName
    $relativePath = $file.Substring($root.Length).TrimStart('\') -replace '\\', '/'
    $folderPath = (Split-Path $relativePath -Parent) -replace '/', '.'

    $namespace = if ($folderPath -eq '') { $baseNamespace } else { "$baseNamespace.$folderPath" }

    Write-Host "Fixing: $file → namespace $namespace"

    $originalLines = Get-Content $file
    $indentedLines = $originalLines | ForEach-Object { "    $_" }

    $newContent = @()
    $newContent += "namespace $namespace"
    $newContent += "{"
    $newContent += $indentedLines
    $newContent += "}"

    # Optional backup
    Copy-Item $file "$file.bak"

    Set-Content -Path $file -Value $newContent -Encoding UTF8
}