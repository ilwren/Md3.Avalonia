# Build the release NuGet packages for all seven Md3.Avalonia packages.
# Requires Java 17 and the .NET Android workload: dotnet workload install android.
[CmdletBinding()]
param(
    [string]$Output = "$PSScriptRoot/../artifacts/nuget",
    [string]$Configuration = "Release",
    [string]$Version = "",
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
    (Join-Path $root "src/Md3.Avalonia.Extra/Md3.Avalonia.Extra.csproj"),
    (Join-Path $root "src/Md3.Avalonia.DataGrid/Md3.Avalonia.DataGrid.csproj"),
    (Join-Path $root "src/Md3.Avalonia.RichEditor/Md3.Avalonia.RichEditor.csproj"),
    (Join-Path $root "src/Md3.Avalonia.Notifications/Md3.Avalonia.Notifications.csproj")
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

if ($Version -and $Local) {
    throw "-Version and -Local cannot be combined."
}
if ($Version -and $Version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$') {
    throw "Invalid package version '$Version'. Use SemVer such as 0.4.6 or 0.4.6-preview.1."
}

$versionArgs = @()
if ($Version) {
    $versionArgs = @("-p:Version=$Version")
    Write-Host "Package version override: $Version" -ForegroundColor Cyan
} elseif ($Local) {
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
    $projectArgs = @()
    if ($proj -eq (Join-Path $root "src/Md3.Avalonia.Notifications/Md3.Avalonia.Notifications.csproj")) {
        # Include the Android TFM only in release packaging, after the script's documented
        # Android workload prerequisite has been satisfied.
        $projectArgs = @("-p:IncludeAndroidTarget=true")
    }

    if (-not $NoRestore) {
        $restoreArgs = @("restore", $proj, "--nologo") + $projectArgs + $versionArgs
        dotnet @restoreArgs
        if ($LASTEXITCODE -ne 0) { throw "Restore failed for $proj" }
    }
    $buildArgs = @("build", $proj, "-c", $Configuration, "--no-restore", "--nologo") + $projectArgs + $versionArgs
    dotnet @buildArgs
    if ($LASTEXITCODE -ne 0) { throw "Build failed for $proj" }

    $packArgs = @("pack", $proj, "-c", $Configuration, "--no-build", "--no-restore", "--nologo", "-o", $outputDir) + $projectArgs + $versionArgs
    dotnet @packArgs
    if ($LASTEXITCODE -ne 0) { throw "Pack failed for $proj" }
}

Write-Host ""
Write-Host "Built packages:" -ForegroundColor Green
Get-ChildItem $outputDir -Filter *.nupkg | Sort-Object LastWriteTime -Descending |
    Select-Object -First 8 | ForEach-Object { Write-Host "  $($_.FullName)" -ForegroundColor Green }
if (-not $Local -and -not $Version) {
    Write-Host ""
    Write-Host "WARNING: the version is unchanged. NuGet will keep restoring the previously cached" -ForegroundColor Yellow
    Write-Host "copy from %USERPROFILE%\.nuget\packages instead of this build. Either pass -Local, or" -ForegroundColor Yellow
    Write-Host "delete the cache first, e.g.:  rmdir /s /q `"%USERPROFILE%\.nuget\packages\md3.avalonia`"" -ForegroundColor Yellow
}
