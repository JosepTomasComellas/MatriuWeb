param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$Summary
)

$ErrorActionPreference = "Stop"

Write-Host "=== Release MatriuWeb ===" -ForegroundColor Cyan

# Validar versió
if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    Write-Error "Versió incorrecta (usa X.Y.Z)"
    exit 1
}

# Validació prèvia
.\scripts\check.ps1

if ($LASTEXITCODE -ne 0) {
    Write-Error "Check fallit"
    exit 1
}

# Actualitzar README
$readme = Get-Content "README.md" -Raw

$readme = $readme -replace 'v\d+\.\d+\.\d+', "v$Version"

$today = Get-Date -Format "yyyy-MM-dd"

$entry = "`n### v$Version`n- $Summary`n- $today`n"

$readme = $readme -replace '(## 📜 Changelog)', "`$1`n$entry"

Set-Content "README.md" $readme

# Git
git add .
git commit -m "release: v$Version - $Summary"
git push

Write-Host "✔ Release completada" -ForegroundColor Green
Write-Host "👉 Ara executa deploy al servidor"
