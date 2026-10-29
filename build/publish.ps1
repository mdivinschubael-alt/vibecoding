<#
.SYNOPSIS
    Script de compilation et de publication automatisée pour PharmaGestion (Windows x64).

.DESCRIPTION
    Ce script compile la solution en mode Release, exécute les tests unitaires et d'intégration,
    puis publie les fichiers exécutables prêts à être distribués soit de manière autonome
    (Self-Contained avec runtime et SQLite intégrés), soit dépendante du framework .NET.

.PARAMETER Configuration
    Configuration de build (par défaut: "Release").

.PARAMETER Runtime
    Identifiant de runtime cible (par défaut: "win-x64").

.PARAMETER Target
    Type de publication : "Standalone", "FrameworkDependent", ou "Both" (par défaut: "Both").

.PARAMETER Clean
    Nettoie les anciens dossiers de publication avant de compiler.

.EXAMPLE
    .\build\publish.ps1
    .\build\publish.ps1 -Target Standalone -Clean
#>

[CmdletBinding()]
param (
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [ValidateSet("Standalone", "FrameworkDependent", "Both")]
    [string]$Target = "Both",
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

# Assurer que dotnet est accessible
if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    if (Test-Path "C:\Program Files\dotnet\dotnet.exe") {
        $env:PATH = "C:\Program Files\dotnet;" + $env:PATH
        Write-Host "Chemin .NET ajouté à la session : C:\Program Files\dotnet" -ForegroundColor Cyan
    } else {
        Write-Error "Le SDK .NET n'est pas trouvé sur votre machine. Veuillez l'installer depuis https://dotnet.microsoft.com/download"
        exit 1
    }
}

$RootDir = Split-Path -Parent $PSScriptRoot
$ProjectUI = Join-Path $RootDir "PharmaGestion.UI\PharmaGestion.UI.csproj"
$ProjectTests = Join-Path $RootDir "PharmaGestion.Tests\PharmaGestion.Tests.csproj"
$PublishBaseDir = Join-Path $RootDir "publish"

Write-Host "==========================================================" -ForegroundColor Green
Write-Host "  PharmaGestion - Script de Publication & Déploiement" -ForegroundColor Green
Write-Host "  Cible : $Runtime | Configuration : $Configuration" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

# 1. Nettoyage si demandé
if ($Clean) {
    Write-Host "`n[1/4] Nettoyage des répertoires de publication..." -ForegroundColor Yellow
    if (Test-Path $PublishBaseDir) {
        Remove-Item -Recurse -Force $PublishBaseDir
        Write-Host "  Dossier 'publish/' nettoyé." -ForegroundColor Gray
    }
} else {
    Write-Host "`n[1/4] Conservation des caches de publication existants..." -ForegroundColor Gray
}

# 2. Exécution des tests automatisés
Write-Host "`n[2/4] Exécution de la suite de tests automatisés (xUnit)..." -ForegroundColor Yellow
dotnet test $ProjectTests -c $Configuration --nologo
if ($LASTEXITCODE -ne 0) {
    Write-Error "Échec des tests automatisés. Publication interrompue pour préserver la qualité logicielle."
    exit $LASTEXITCODE
}
Write-Host "  Tous les tests sont validés avec succès !" -ForegroundColor Green

# 3. Publication Standalone (Autonome)
if ($Target -eq "Standalone" -or $Target -eq "Both") {
    $OutputDirStandalone = Join-Path $PublishBaseDir "win-x64-standalone"
    Write-Host "`n[3/4] Publication Autonome (Self-Contained) -> $OutputDirStandalone" -ForegroundColor Yellow
    Write-Host "  Inclusion complète du Runtime .NET et des dépendances SQLite..." -ForegroundColor Gray

    dotnet publish $ProjectUI `
        -c $Configuration `
        -r $Runtime `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -p:DebugType=None `
        -p:DebugSymbols=false `
        -o $OutputDirStandalone `
        --nologo

    # Nettoyage des éventuels PDB résiduels pour alléger la taille finale
    Get-ChildItem -Path $OutputDirStandalone -Filter "*.pdb" -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue

    $exePath = Join-Path $OutputDirStandalone "PharmaGestion.UI.exe"
    if (Test-Path $exePath) {
        $sizeMb = [Math]::Round(((Get-Item $exePath).Length / 1MB), 2)
        Write-Host "  Succès : $exePath ($sizeMb Mo)" -ForegroundColor Green
        Write-Host "  Ce binaire est 100% autonome et fonctionnera sur toute machine Windows 64-bit." -ForegroundColor Cyan
    }
}

# 4. Publication Framework-Dependent (Légère)
if ($Target -eq "FrameworkDependent" -or $Target -eq "Both") {
    $OutputDirFdd = Join-Path $PublishBaseDir "win-x64-framework-dependent"
    Write-Host "`n[4/4] Publication Dépendante du Framework -> $OutputDirFdd" -ForegroundColor Yellow
    Write-Host "  Génération d'un binaire compact nécessitant le runtime .NET installé..." -ForegroundColor Gray

    dotnet publish $ProjectUI `
        -c $Configuration `
        -r $Runtime `
        --self-contained false `
        -p:PublishSingleFile=true `
        -p:DebugType=None `
        -p:DebugSymbols=false `
        -o $OutputDirFdd `
        --nologo

    Get-ChildItem -Path $OutputDirFdd -Filter "*.pdb" -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue

    $exePathFdd = Join-Path $OutputDirFdd "PharmaGestion.UI.exe"
    if (Test-Path $exePathFdd) {
        $sizeMbFdd = [Math]::Round(((Get-Item $exePathFdd).Length / 1MB), 2)
        Write-Host "  Succès : $exePathFdd ($sizeMbFdd Mo)" -ForegroundColor Green
    }
}

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host "  Publication terminée avec succès !" -ForegroundColor Green
Write-Host "  Vous pouvez maintenant compiler l'installateur Inno Setup :" -ForegroundColor White
Write-Host "  ISCC.exe .\installer\PharmaGestion_Setup.iss" -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Green
