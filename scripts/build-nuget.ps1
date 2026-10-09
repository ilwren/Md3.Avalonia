# Build the release NuGet packages for Md3.Avalonia, Md3.Avalonia.Icons, Md3.Avalonia.Icons.Lite,
# Md3.Avalonia.Extra, Md3.Avalonia.DataGrid and Md3.Avalonia.RichEditor.
[CmdletBinding()]
param(
    [string]$Output = "$PSScriptRoot/../artifacts/nuget",
    [string]$Configuration = "Release",
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path "$PSScriptRoot/..").Path
$outputDir = if ([System.IO.Path]::IsPathRooted($Output)) { $Output } else { Join-Path $root $Output }

$projects = @(
    (Join-Path $root "src/Md3.Avalonia/Md3.Avalonia.csproj"),
    (Join-Path $root "src/Md3.Avalonia.Icons/Md3.Avalonia.Icons.csproj"),
    (Join-Path $root "src/Md3.Avalonia.Icons.Lite/Md3.Avalonia.Icons.Lite.csproj"),
    (Join-Path $root "src/Md3.Avalonia.Extra/Md3.Avalonia.Extra.csproj"),
    (Join-Path $root "src/Md3.Avalonia.DataGrid/Md3.Avalonia.DataGrid.csproj"),
    (Join-Path $root "src/Md3.Avalonia.RichEditor/Md3.Avalonia.RichEditor.csproj")
)

$python = Get-Command python3 -ErrorAction SilentlyContinue
if (-not $python) { $python = Get-Command python -ErrorAction SilentlyContinue }
if (-not $python) { throw "Python is required to verify the embedded official fonts." }
& $python.Source (Join-Path $root "scripts/verify-fonts.py")
if ($LASTEXITCODE -ne 0) { throw "Official Material Symbols font verification failed." }

if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

foreach ($proj in $projects) {
    if (-not $NoRestore) {
        dotnet restore $proj --nologo
    }
    dotnet build $proj -c $Configuration --no-restore --nologo
    dotnet pack $proj -c $Configuration --no-build --no-restore --nologo -o $outputDir
}

# The five Material packages ship net8.0 and net10.0 assets; Md3.Avalonia.RichEditor is
# net10.0-only because its upstream AvaloniaRichEditor dependency is. Same check as
# scripts/build-nuget.sh: a package that loses a target framework fails here, not in the field.
$version = (Select-String -Path $projects[0] -Pattern '<Version>([^<]+)</Version>').Matches[0].Groups[1].Value
$expectations = @(
    "Md3.Avalonia=net8.0,net10.0",
    "Md3.Avalonia.Icons=net8.0,net10.0",
    "Md3.Avalonia.Icons.Lite=net8.0,net10.0",
    "Md3.Avalonia.Extra=net8.0,net10.0",
    "Md3.Avalonia.DataGrid=net8.0,net10.0",
    "Md3.Avalonia.RichEditor=net10.0"
)
$assetArgs = @("--dir", $outputDir, "--version", $version, "--icon", (Join-Path $root "logo.png"))
foreach ($expectation in $expectations) { $assetArgs += @("--expect", $expectation) }
& $python.Source (Join-Path $root "scripts/verify-package-assets.py") @assetArgs
if ($LASTEXITCODE -ne 0) { throw "Packed packages do not carry the expected target framework assets." }

Write-Host "Built packages in $outputDir" -ForegroundColor Green
