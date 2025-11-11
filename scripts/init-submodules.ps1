<#
.SYNOPSIS
Initializes and clones submodules defined in .gitmodules.

USAGE
From the repository root run:
  pwsh.exe ./scripts/init-submodules.ps1

This script will first try `git submodule update --init --recursive`.
If that fails or some paths are still missing it will fall back to cloning
each url listed in .gitmodules into the configured path.
#>

Set-StrictMode -Version Latest

try {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $repoRoot = Resolve-Path (Join-Path $scriptDir '..')
    $gitmodulesPath = Join-Path $repoRoot '.gitmodules'

    if (-not (Test-Path $gitmodulesPath)) {
        Write-Host "No .gitmodules file found at $gitmodulesPath. Nothing to do." -ForegroundColor Yellow
        exit 0
    }

    Write-Host "Attempting: git submodule update --init --recursive"
    & git -C $repoRoot submodule update --init --recursive
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Submodules initialized successfully." -ForegroundColor Green
        exit 0
    }

    Write-Host "Falling back to cloning submodules listed in .gitmodules"
    $pathKeys = & git config --file $gitmodulesPath --name-only --get-regexp path
    if (-not $pathKeys) {
        Write-Host "No submodule paths listed in .gitmodules" -ForegroundColor Yellow
        exit 1
    }

    foreach ($key in $pathKeys) {
        $relativePath = (& git config --file $gitmodulesPath --get $key).Trim()
        if (-not $relativePath) { continue }
        $urlKey = $key -replace '\.path$','.url'
        $url = (& git config --file $gitmodulesPath --get $urlKey).Trim()

        $targetPath = Join-Path $repoRoot $relativePath
        if (Test-Path $targetPath) {
            Write-Host "Path '$relativePath' already exists — skipping." -ForegroundColor Cyan
            continue
        }

        if (-not $url) {
            Write-Host "No URL configured for submodule path '$relativePath' — skipping." -ForegroundColor Yellow
            continue
        }

        Write-Host "Cloning '$url' into '$relativePath'..."
        & git -C $repoRoot clone --depth 1 $url $relativePath
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to clone $url into $relativePath" -ForegroundColor Red
        } else {
            Write-Host "Cloned $relativePath" -ForegroundColor Green
        }
    }

    Write-Host "Done. If any submodules still missing, try running: git submodule update --init --recursive" -ForegroundColor Green
    exit 0
}
catch {
    Write-Error "An error occurred: $_"
    exit 2
}
