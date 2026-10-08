# Publish the desktop Gallery with native AOT and prove it is what it claims to be.
#
# Windows and macOS twin of scripts/publish-gallery-aot.sh. Both scripts perform the same four
# checks: publish, assert the output is a native executable with no managed assembly beside it,
# assert zero ILxxxx trim/AOT diagnostics, and (optionally) start the binary and see if it
# survives. The smoke run is opt-in here because a Windows session may not be available to a
# service account; CI runs the bash twin under xvfb.
#
#   pwsh scripts/publish-gallery-aot.ps1
#   pwsh scripts/publish-gallery-aot.ps1 -Rid win-x64 -Smoke
#   pwsh scripts/publish-gallery-aot.ps1 -AllowDiagnostics -NoSmoke
[CmdletBinding()]
param(
    [string]$Rid = $(if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq 'Arm64') { 'win-arm64' } else { 'win-x64' }),
    [string]$Output = '',
    [string]$Configuration = 'Release',
    [switch]$NoRestore,
    [switch]$AllowDiagnostics,
    [switch]$NoSmoke,
    [switch]$Smoke,
    [int]$SmokeTimeoutSeconds = 25,
    [switch]$KeepIntermediate
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path "$PSScriptRoot/..").Path
$project = Join-Path $root 'gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj'
if (-not (Test-Path $project)) { throw "$project was not found" }

if ([string]::IsNullOrWhiteSpace($Output)) { $Output = "artifacts/aot/$Rid" }
$outputDir = if ([System.IO.Path]::IsPathRooted($Output)) { $Output } else { Join-Path $root $Output }
$logDir = Join-Path $root 'artifacts/aot'
$log = Join-Path $logDir "publish-$Rid.log"

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) { throw 'dotnet was not found in PATH' }
$sdkVersion = & $dotnet.Source --version
if ([int]($sdkVersion.Split('.')[0]) -lt 10) { throw ".NET SDK 10 or newer is required; selected SDK is $sdkVersion" }

$binaryName = (Select-String -Path $project -Pattern '<AssemblyName>([^<]*)</AssemblyName>' |
    Select-Object -First 1).Matches.Groups[1].Value
if ([string]::IsNullOrWhiteSpace($binaryName)) { $binaryName = [System.IO.Path]::GetFileNameWithoutExtension($project) }
if ($Rid.StartsWith('win-')) { $binaryName = "$binaryName.exe" }

function Remove-Intermediate {
    if ($KeepIntermediate) { return }
    foreach ($dir in @('gallery/Md3.Avalonia.Gallery', 'gallery/Md3.Avalonia.Gallery.Desktop')) {
        foreach ($name in @('bin', 'obj')) {
            $path = Join-Path $root "$dir/$name"
            if (Test-Path $path) { Remove-Item -Recurse -Force $path -ErrorAction SilentlyContinue }
        }
    }
}

if (Test-Path $outputDir) { Remove-Item -Recurse -Force $outputDir }
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir -Force | Out-Null }

# TreatWarningsAsErrors stays off: the publish has to reach the end so the produced binary can be
# smoke-tested, and the ILxxxx count below is what has to be zero.
$arguments = @('publish', $project, '-c', $Configuration, '-r', $Rid,
    '-p:PublishAot=true', '-p:TreatWarningsAsErrors=false', '-o', $outputDir)
if ($NoRestore) { $arguments += '--no-restore' }
$arguments += '--nologo'

Write-Host "Publishing $binaryName ($Rid, $Configuration) with native AOT..."
Write-Host "  SDK:    $sdkVersion"
Write-Host "  output: $outputDir`n"

& $dotnet.Source @arguments 2>&1 | Tee-Object -FilePath $log
if ($LASTEXITCODE -ne 0) {
    Remove-Intermediate
    throw "dotnet publish failed with exit code $LASTEXITCODE; see $log"
}

try {
    $binary = Join-Path $outputDir $binaryName
    if (-not (Test-Path $binary)) { throw "no native binary was produced at $binary" }

    # The first four bytes say what the file is; a native AOT publish is never a managed assembly.
    $bytes = [System.IO.File]::ReadAllBytes($binary)[0..3]
    $magic = ($bytes | ForEach-Object { $_.ToString('x2') }) -join ''
    $kind = switch -Regex ($magic) {
        '^7f454c46' { 'ELF' }
        '^(cffaedfe|feedfacf|cafebabe)' { 'Mach-O' }
        '^4d5a' { 'PE' }
        default { throw "$binary does not start with a known executable header (got '$magic')" }
    }
    $size = (Get-Item $binary).Length
    Write-Host "Native binary: $kind, $size bytes"

    $managed = @(Get-ChildItem -Path $outputDir -Filter '*.dll' -File)
    if ($managed.Count -gt 0) {
        throw "$($managed.Count) managed .dll file(s) in $outputDir - this was not an AOT publish"
    }

    # ILxxxx trim/AOT diagnostics: the README claims zero for a native-AOT publish.
    $diagnostics = @(Select-String -Path $log -Pattern '(warning|error) IL[0-9]{4}' |
        ForEach-Object { $_.Line.Trim() } | Sort-Object -Unique)
    if ($diagnostics.Count -gt 0) {
        Write-Host "`nTrim/AOT diagnostics found (the count has to be zero):"
        $diagnostics | Select-Object -First 20 | ForEach-Object { Write-Host "  $_" }
        if ($AllowDiagnostics) {
            Write-Warning 'continuing despite the diagnostics because -AllowDiagnostics was passed'
        }
        else {
            throw 'native AOT must not emit IL diagnostics; fix the call sites or re-run with -AllowDiagnostics'
        }
    }
    else {
        Write-Host 'Trim/AOT diagnostics: 0'
    }

    if ($Smoke -and -not $NoSmoke) {
        Write-Host "`nStarting $binaryName with a ${SmokeTimeoutSeconds}s timeout..."
        $process = Start-Process -FilePath $binary -PassThru
        try {
            if ($process.WaitForExit($SmokeTimeoutSeconds * 1000)) {
                throw "the AOT binary exited on its own with $($process.ExitCode); it is not a working application"
            }
            Write-Host 'Still running when the timeout fired, which is a pass.'
        }
        finally {
            if (-not $process.HasExited) { $process.Kill($true) }
        }
    }

    Write-Host "`nDone. Native binary: $binary"
}
finally {
    Remove-Intermediate
}
