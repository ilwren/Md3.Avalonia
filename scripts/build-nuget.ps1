# Build the release NuGet packages for Md3.Avalonia, Md3.Avalonia.Icons, Md3.Avalonia.Icons.Lite, Md3.Avalonia.Extra
[CmdletBinding()]
param(
    [string]$Output = "$PSScriptRoot/../artifacts/nuget",
    [string]$Configuration = "Release",
    [switch]$NoRestore,
    # Local iteration helper: every run stamps a unique version so NuGet cannot silently keep
    # serving a previously cached package of the same version from %USERPROFILE%\.nuget\packages.
    [switch]$Local,
    # Skip the embedded-font verification (needs Python); releases must not skip it.
    [switch]$SkipFontCheck
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

if ($SkipFontCheck) {
    Write-Host "Skipping embedded font verification (-SkipFontCheck). Do not use for release builds." -ForegroundColor Yellow
} else {
    $python = Get-Command python3 -ErrorAction SilentlyContinue
    if (-not $python) { $python = Get-Command python -ErrorAction SilentlyContinue }
    if (-not $python) { throw "Python is required to verify the embedded official fonts (or pass -SkipFontCheck for local test builds)." }
    & $python.Source (Join-Path $root "scripts/verify-fonts.py")
    if ($LASTEXITCODE -ne 0) { throw "Official Material Symbols font verification failed." }
}

$versionArgs = @()
if ($Local) {
    # Read the base version from the library csproj and stamp a unique local prerelease on top.
    [xml]$csproj = Get-Content (Join-Path $root "src/Md3.Avalonia/Md3.Avalonia.csproj")
    $base = $csproj.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    $stamp = Get-Date -Format "yyyyMMddHHmmss"
    $localVersion = "$base-local.$stamp"
    $versionArgs = @("-p:Version=$localVersion")
    Write-Host "Local build version: $localVersion" -ForegroundColor Cyan
}

if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

foreach ($proj in $projects) {
    if (-not $NoRestore) {
        dotnet restore $proj --nologo
        if ($LASTEXITCODE -ne 0) { throw "Restore failed for $proj" }
    }
    dotnet build $proj -c $Configuration --no-restore --nologo $versionArgs
    if ($LASTEXITCODE -ne 0) { throw "Build failed for $proj" }
    dotnet pack $proj -c $Configuration --no-build --no-restore --nologo -o $outputDir $versionArgs
    if ($LASTEXITCODE -ne 0) { throw "Pack failed for $proj" }
}

Write-Host ""
Write-Host "Built packages:" -ForegroundColor Green
Get-ChildItem $outputDir -Filter *.nupkg | Sort-Object LastWriteTime -Descending |
    Select-Object -First 8 | ForEach-Object { Write-Host "  $($_.FullName)" -ForegroundColor Green }
if (-not $Local) {
    Write-Host ""
    Write-Host "WARNING: the version is unchanged. NuGet will keep restoring the previously cached" -ForegroundColor Yellow
    Write-Host "copy from %USERPROFILE%\.nuget\packages instead of this build. Either pass -Local, or" -ForegroundColor Yellow
    Write-Host "delete the cache first, e.g.:  rmdir /s /q `"%USERPROFILE%\.nuget\packages\md3.avalonia`"" -ForegroundColor Yellow
}
