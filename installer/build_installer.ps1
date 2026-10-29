<#
.SYNOPSIS
    Script pour compiler l'installateur Inno Setup (.exe).
#>

[CmdletBinding()]
param ()

$RootDir = Split-Path -Parent $PSScriptRoot
$IssPath = Join-Path $RootDir "installer\PharmaGestion_Setup.iss"
$ExePath = Join-Path $RootDir "publish\win-x64-standalone\PharmaGestion.UI.exe"

Write-Host "==========================================================" -ForegroundColor Green
Write-Host "  Compilation de l'Installateur Inno Setup" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

# 1. Vérifier si l'exécutable a été publié
if (-not (Test-Path $ExePath)) {
    Write-Host "L'exécutable autonome n'a pas encore été généré." -ForegroundColor Yellow
    Write-Host "Lancement automatique de la publication..." -ForegroundColor Cyan
    & "$RootDir\build\publish.ps1" -Target Standalone
}

# 2. Chercher le compilateur Inno Setup (ISCC.exe)
$isccCandidates = @(
    (Get-Command "ISCC.exe" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source),
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
    "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe"
)

$isccPath = $isccCandidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1

if (-not $isccPath) {
    Write-Host "`n[INFORMATION] Inno Setup 6 n'est pas encore installé sur cette machine." -ForegroundColor Yellow
    Write-Host "Pour générer le fichier .exe d'installation :" -ForegroundColor White
    Write-Host "1. Téléchargez Inno Setup 6 gratuitement sur : https://jrsoftware.org/isdl.php" -ForegroundColor Cyan
    Write-Host "2. Installez-le avec les paramètres par défaut." -ForegroundColor Cyan
    Write-Host "3. Relancez ce script ou ouvrez '$IssPath' dans Inno Setup Compiler et cliquez sur 'Compile'." -ForegroundColor Cyan
    exit 0
}

Write-Host "Compilateur Inno Setup détecté : $isccPath" -ForegroundColor Green
Write-Host "Compilation de '$IssPath'..." -ForegroundColor Cyan

& "$isccPath" "$IssPath"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n==========================================================" -ForegroundColor Green
    Write-Host "  Installateur généré avec succès dans installer\output\ !" -ForegroundColor Green
    Write-Host "==========================================================" -ForegroundColor Green
} else {
    Write-Error "Erreur lors de la compilation Inno Setup (Code $LASTEXITCODE)."
}
