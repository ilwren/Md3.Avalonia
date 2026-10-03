# Build the release NuGet packages for Md3.Avalonia, Md3.Avalonia.Icons, Md3.Avalonia.Icons.Lite, Md3.Avalonia.Extra
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
    (Join-Path $root "src/Md3.Avalonia.Extra/Md3.Avalonia.Extra.csproj")
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

Write-Host "Built packages in $outputDir" -ForegroundColor Green
