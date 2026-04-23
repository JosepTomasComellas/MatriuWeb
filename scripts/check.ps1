# check.ps1 — Validació local abans de commit/release
$ErrorActionPreference = "Stop"

Write-Host "=== Check MatriuWeb ===" -ForegroundColor Cyan

$errors = @()

# Fitxers obligatoris
$required = @(
    "compose.yaml", ".env.example", ".gitignore", "README.md",
    "CLAUDE.md", "ARCHITECTURE.md", "RELEASE.md",
    "nginx/conf.d/matriuweb.conf",
    "monitoring/prometheus/prometheus.yml",
    "src/MatriuWeb/MatriuWeb.csproj",
    "src/MatriuWeb/Dockerfile",
    "src/MatriuWeb/Program.cs"
)

foreach ($f in $required) {
    if (-not (Test-Path $f)) {
        $errors += "Falta: $f"
    }
}

# .env no ha de ser al repositori
if (Test-Path ".env") {
    $errors += "WARN: .env present — no hauria d'estar al repositori"
}

# Cert fora del repo (avís, no error)
if ((Get-Content ".gitignore") -notmatch "nginx/certs") {
    $errors += "WARN: nginx/certs no ignorat al .gitignore"
}

# Compilació Blazor
Write-Host "Compilant..." -ForegroundColor Yellow
Push-Location "src/MatriuWeb"
dotnet build -c Release --nologo -v quiet 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) { $errors += "ERROR: dotnet build ha fallat" }
Pop-Location

if ($errors.Count -gt 0) {
    Write-Host ""
    Write-Host "Problemes detectats:" -ForegroundColor Red
    $errors | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    exit 1
}

Write-Host "Tot OK" -ForegroundColor Green
exit 0
