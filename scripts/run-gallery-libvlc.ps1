[CmdletBinding()]
param(
    [string]$VlcHome = $env:VLC_HOME,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [switch]$NoBuild,
    [switch]$CheckOnly,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$GalleryArguments
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'dotnet was not found in PATH.'
}
$sdkVersion = (& dotnet --version).Trim()
$sdkMajor = 0
if (-not [int]::TryParse(($sdkVersion -split '\.')[0], [ref]$sdkMajor) -or $sdkMajor -lt 10) {
    throw ".NET SDK 10 or newer is required; selected SDK is $sdkVersion."
}

$candidates = @(
    $VlcHome,
    $(if ($VlcHome) { Join-Path $VlcHome 'lib' }),
    $(if ($env:ProgramFiles) { Join-Path $env:ProgramFiles 'VideoLAN/VLC' }),
    $(if (${env:ProgramFiles(x86)}) { Join-Path ${env:ProgramFiles(x86)} 'VideoLAN/VLC' })
) | Where-Object { $_ -and (Test-Path $_ -PathType Container) }

$library = $null
foreach ($candidate in $candidates) {
    $library = Get-ChildItem -Path $candidate -Filter 'libvlc.dll' -File -Recurse -Depth 2 -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($library) { break }
}
if (-not $library) {
    throw @'
Native libvlc.dll was not found. Install the desktop VLC application, or pass its install root:
  .\scripts\run-gallery-libvlc.ps1 -VlcHome 'C:\Program Files\VideoLAN\VLC'
The managed LibVLCSharp package alone does not contain the native decoder.
'@
}

$libDir = $library.Directory.FullName
$pluginCandidates = @(
    $(if ($VlcHome) { Join-Path $VlcHome 'plugins' }),
    (Join-Path $libDir 'plugins')
) | Where-Object { $_ -and (Test-Path $_ -PathType Container) }
$pluginDir = $pluginCandidates | Select-Object -First 1
if (-not $pluginDir) {
    throw "libvlc.dll was found in '$libDir', but its plugins directory was not found. Pass the VLC install root with -VlcHome."
}

$env:Path = "$libDir;$env:Path"
$env:VLC_PLUGIN_PATH = $pluginDir
Write-Host "LibVLC library: $libDir"
Write-Host "LibVLC plugins: $pluginDir"
Write-Host ".NET SDK: $sdkVersion"
if ($CheckOnly) { exit 0 }

$arguments = @('run', '--project', $project, '--configuration', $Configuration, '-p:PublishAot=false')
if ($NoBuild) { $arguments += '--no-build' }
$arguments += '--'
if ($GalleryArguments) { $arguments += $GalleryArguments }

& dotnet @arguments
exit $LASTEXITCODE
