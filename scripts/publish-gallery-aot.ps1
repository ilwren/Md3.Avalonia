<#
.SYNOPSIS
Publishes the desktop Gallery as a native AOT binary.

.DESCRIPTION
Mirrors scripts/publish-gallery-aot.sh for Windows PowerShell. Publishes
gallery/Md3.Avalonia.Gallery.Desktop with PublishAot=true, asserts that the output is
a native executable with no managed copy of the app assembly beside it, and cleans
bin/obj afterwards unless -KeepIntermediate is passed.

.PARAMETER Rid
Target runtime identifier. Defaults to the current platform (win-x64/win-arm64 on
Windows, osx-x64/osx-arm64 on macOS).

.PARAMETER Output
Output directory. Defaults to artifacts/aot under the repository root.

.PARAMETER KeepIntermediate
Skip deleting bin/ and obj/ after the publish.

.EXAMPLE
powershell -File scripts/publish-gallery-aot.ps1 -Rid win-x64
#>
param(
    [string]$Rid = "",
    [string]$Output = "",
    [string]$Configuration = "Release",
    [switch]$KeepIntermediate
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $Root "gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj"

if ($Rid -eq "") {
    $Rid = if ($IsMacOS) {
        if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq "Arm64") { "osx-arm64" } else { "osx-x64" }
    }
    else {
        if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq "Arm64") { "win-arm64" } else { "win-x64" }
    }
}
if ($Output -eq "") { $Output = Join-Path $Root "artifacts/aot" }
if (-not [System.IO.Path]::IsPathRooted($Output)) { $Output = Join-Path $Root $Output }

$null = Get-Command dotnet -ErrorAction Stop
$sdkMajor = [int]((dotnet --version) -split '\.')[0]
if ($sdkMajor -lt 10) { throw ".NET SDK 10 or newer is required; selected SDK is $(dotnet --version)" }

function Cleanup-Intermediate {
    if ($KeepIntermediate) { return }
    foreach ($dir in @("src", "gallery")) {
        Get-ChildItem -Path (Join-Path $Root $dir) -Directory -Recurse -Include bin, obj |
            Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    }
}

try {
    if (Test-Path $Output) { Remove-Item $Output -Recurse -Force }
    New-Item -ItemType Directory -Path $Output | Out-Null

    Write-Host "Publishing native AOT ($Rid) ..."
    dotnet publish $Project -c $Configuration -r $Rid `
        -p:PublishAot=true -p:TreatWarningsAsErrors=false --nologo -o $Output
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }

    $appName = "Md3.Avalonia.Gallery.Desktop"
    $bin = if ($Rid -like "win-*") { Join-Path $Output "$appName.exe" } else { Join-Path $Output $appName }
    if (-not (Test-Path $bin)) { throw "no native binary was produced at $bin" }

    $managed = Join-Path $Output "$appName.dll"
    if (Test-Path $managed) { throw "managed assembly still present at $managed - this was not an AOT publish" }

    Write-Host ""
    Write-Host "Native AOT publish: $bin"
}
finally {
    Cleanup-Intermediate
}
