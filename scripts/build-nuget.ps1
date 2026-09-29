[CmdletBinding()]
param(
    [Parameter()]
    [string]$Font,

    [Parameter()]
    [string]$Output,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]$Configuration = "Release",

    [Parameter()]
    [switch]$NoRestore,

    [Parameter()]
    [switch]$Help
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

$scriptRoot = $PSScriptRoot
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $scriptRoot ".."))

if ([string]::IsNullOrWhiteSpace($Font)) {
    $Font = Join-Path $repositoryRoot "src\Md3.Avalonia.Icons\Assets\Fonts\MaterialSymbolsRounded.ttf"
}
elseif (-not [System.IO.Path]::IsPathRooted($Font)) {
    $Font = Join-Path $repositoryRoot $Font
}
$Font = [System.IO.Path]::GetFullPath($Font)

if ([string]::IsNullOrWhiteSpace($Output)) {
    $Output = Join-Path $repositoryRoot "artifacts\nuget"
}
elseif (-not [System.IO.Path]::IsPathRooted($Output)) {
    $Output = Join-Path $repositoryRoot $Output
}
$Output = [System.IO.Path]::GetFullPath($Output)

$projects = @(
    (Join-Path $repositoryRoot "src\Md3.Avalonia\Md3.Avalonia.csproj"),
    (Join-Path $repositoryRoot "src\Md3.Avalonia.Icons\Md3.Avalonia.Icons.csproj"),
    (Join-Path $repositoryRoot "src\Md3.Avalonia.Ecosystem\Md3.Avalonia.Ecosystem.csproj")
)

function Write-Usage {
    Write-Host @"
Build the three release NuGet packages for net8.0 and net10.0.

Examples:
  powershell -ExecutionPolicy Bypass -File .\scripts\build-nuget.ps1
  powershell -ExecutionPolicy Bypass -File .\scripts\build-nuget.ps1 `
    -Font D:\OfflineAssets\MaterialSymbolsRounded.ttf
  powershell -ExecutionPolicy Bypass -File .\scripts\build-nuget.ps1 `
    -Font D:\OfflineAssets\MaterialSymbolsRounded.ttf `
    -Output D:\Packages -NoRestore

The official font is intentionally not included in this repository. If -Font is
omitted, the script expects:
  src\Md3.Avalonia.Icons\Assets\Fonts\MaterialSymbolsRounded.ttf
"@
}

function Invoke-DotNet {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Remove-IntermediateOutputs {
    foreach ($project in $projects) {
        $projectDirectory = Split-Path -Parent $project
        foreach ($directoryName in @("bin", "obj")) {
            $directory = Join-Path $projectDirectory $directoryName
            if (Test-Path -LiteralPath $directory) {
                Remove-Item -LiteralPath $directory -Recurse -Force
            }
        }
    }
}

function Test-FontFile {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw @"
Material Symbols Rounded font is missing.
Expected: $Path
Copy the official font into the reserved slot or pass:
  -Font D:\OfflineAssets\MaterialSymbolsRounded.ttf
"@
    }

    $extension = [System.IO.Path]::GetExtension($Path).ToLowerInvariant()
    if ($extension -ne ".ttf" -and $extension -ne ".otf") {
        throw "The font must be a .ttf or .otf file: $Path"
    }

    $fontInfo = Get-Item -LiteralPath $Path
    if ($fontInfo.Length -lt 100000) {
        throw "'$Path' is too small to be the official Material Symbols font ($($fontInfo.Length) bytes)."
    }

    $stream = [System.IO.File]::OpenRead($Path)
    try {
        $header = New-Object byte[] 4
        if ($stream.Read($header, 0, 4) -ne 4) {
            throw "Unable to read the OpenType/TrueType header from '$Path'."
        }
    }
    finally {
        $stream.Dispose()
    }

    $magic = ([System.BitConverter]::ToString($header)).Replace("-", "")
    $supportedHeaders = @("00010000", "4F54544F", "74727565", "74797031")
    if ($supportedHeaders -notcontains $magic) {
        throw "'$Path' does not have a supported OpenType/TrueType header."
    }
}

$exitCode = 0
try {
    if ($PSBoundParameters.ContainsKey("Help")) {
        Write-Usage
        exit 0
    }

    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnetCommand) {
        throw "dotnet was not found in PATH. Install the .NET 10 SDK first."
    }

    $sdkVersion = (& dotnet --version).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to query the selected .NET SDK."
    }
    $sdkMajorText = $sdkVersion.Split('.')[0]
    $sdkMajor = 0
    if (-not [int]::TryParse($sdkMajorText, [ref]$sdkMajor) -or $sdkMajor -lt 10) {
        throw ".NET SDK 10 or newer is required; selected SDK is $sdkVersion."
    }

    Test-FontFile -Path $Font
    $fontProperty = "-p:MaterialSymbolsRoundedFontFile=$Font"

    Remove-IntermediateOutputs
    if (Test-Path -LiteralPath $Output) {
        Remove-Item -LiteralPath $Output -Recurse -Force
    }
    New-Item -ItemType Directory -Path $Output -Force | Out-Null

    if (-not $NoRestore) {
        Invoke-DotNet -Arguments @("restore", $projects[0], "--nologo")
        Invoke-DotNet -Arguments @("restore", $projects[1], "--nologo", $fontProperty)
        Invoke-DotNet -Arguments @("restore", $projects[2], "--nologo")
    }

    Invoke-DotNet -Arguments @("build", $projects[0], "-c", $Configuration, "--no-restore", "--nologo")
    Invoke-DotNet -Arguments @("build", $projects[1], "-c", $Configuration, "--no-restore", "--nologo", $fontProperty)
    Invoke-DotNet -Arguments @("build", $projects[2], "-c", $Configuration, "--no-restore", "--nologo")

    Invoke-DotNet -Arguments @("pack", $projects[0], "-c", $Configuration, "--no-build", "--no-restore", "--nologo", "-o", $Output)
    Invoke-DotNet -Arguments @("pack", $projects[1], "-c", $Configuration, "--no-build", "--no-restore", "--nologo", "-o", $Output, $fontProperty)
    Invoke-DotNet -Arguments @("pack", $projects[2], "-c", $Configuration, "--no-build", "--no-restore", "--nologo", "-o", $Output)

    $packages = @(Get-ChildItem -LiteralPath $Output -File | Where-Object {
        $_.Name.EndsWith(".nupkg", [System.StringComparison]::OrdinalIgnoreCase) -or
        $_.Name.EndsWith(".snupkg", [System.StringComparison]::OrdinalIgnoreCase)
    })
    if ($packages.Count -ne 6) {
        throw "Expected 6 package files, found $($packages.Count) in '$Output'."
    }

    Write-Host ""
    Write-Host "Built packages ($sdkVersion):"
    foreach ($package in ($packages | Sort-Object Name)) {
        Write-Host "  $($package.Name)"
    }
    Write-Host ""
    Write-Host "Output: $Output"
    Write-Host "Intermediate bin/obj directories are removed automatically."
}
catch {
    [Console]::Error.WriteLine("error: {0}", $_.Exception.Message)
    $exitCode = 1
}
finally {
    Remove-IntermediateOutputs
}

exit $exitCode
