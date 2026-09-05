[CmdletBinding()]
param(
    [ValidateSet("all", "cli", "service", "firmware")]
    [string] $Target = "all",

    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$dist = Join-Path $root "dist"
$firmwareProject = Join-Path $root "rvl.device.fw"
$firmwareOutput = Join-Path $firmwareProject ".pio\build\pico\firmware.uf2"
$cliOutput = Join-Path $dist "cli"
$serviceOutput = Join-Path $dist "service"
$firmwareDist = Join-Path $dist "firmware"

function Invoke-CheckedCommand {
    param(
        [string] $Command,
        [string[]] $Arguments
    )

    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Command failed with exit code $LASTEXITCODE"
    }
}

if ($Target -eq "all") {
    Write-Host "Cleaning $dist"
    if (Test-Path $dist) {
        Remove-Item $dist -Recurse -Force
    }
}
else {
    $targetOutput = Join-Path $dist $Target
    Write-Host "Cleaning $targetOutput"
    if (Test-Path $targetOutput) {
        Remove-Item $targetOutput -Recurse -Force
    }
}

if ($Target -in @("all", "cli")) {
    Write-Host "Publishing CLI ($Configuration)"
    Invoke-CheckedCommand "dotnet" @(
        "publish",
        (Join-Path $root "rvl.host\Cli\Rvl.Display.Cli.csproj"),
        "--configuration", $Configuration,
        "--output", $cliOutput
    )
}

if ($Target -in @("all", "service")) {
    Write-Host "Publishing service ($Configuration)"
    Invoke-CheckedCommand "dotnet" @(
        "publish",
        (Join-Path $root "rvl.host\Service\Rvl.Display.Service.csproj"),
        "--configuration", $Configuration,
        "--output", $serviceOutput
    )
}

if ($Target -in @("all", "firmware")) {
    Write-Host "Building firmware"
    Invoke-CheckedCommand "pio" @(
        "run",
        "--project-dir", $firmwareProject
    )

    if (-not (Test-Path $firmwareOutput)) {
        throw "PlatformIO completed but firmware artifact was not found: $firmwareOutput"
    }

    New-Item $firmwareDist -ItemType Directory -Force | Out-Null
    Copy-Item $firmwareOutput (Join-Path $firmwareDist "rvl-display.uf2")
}

Write-Host "Build complete: $dist"