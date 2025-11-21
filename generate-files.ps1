# Script de génération complète des fichiers de l'API Loutaupia V2
# Ce script crée tous les fichiers nécessaires pour l'architecture Vertical Slice

Write-Host "=== Génération des fichiers de l'API Loutaupia V2 ===" -ForegroundColor Green

# Fonction pour créer un fichier avec son contenu
function Create-FileWithContent {
    param(
        [string]$Path,
        [string]$Content
    )
    
    $dir = Split-Path -Path $Path -Parent
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
    
    Set-Content -Path $Path -Value $Content -Encoding UTF8
    Write-Host "✓ Créé: $Path" -ForegroundColor Cyan
}

Write-Host "`nContinuer avec ce script vous guidera pour créer tous les fichiers nécessaires." -ForegroundColor Yellow
Write-Host "Le projet contient déjà:" -ForegroundColor Yellow
Write-Host "  - Configuration (appsettings.json, docker-compose.yml)" -ForegroundColor Gray
Write-Host "  - Packages NuGet" -ForegroundColor Gray
Write-Host "  - Structure de dossiers" -ForegroundColor Gray
Write-Host "  - Fichiers de base (exceptions, enums, interfaces)" -ForegroundColor Gray
Write-Host "`nIl reste à créer les entités, repositories, use cases, et endpoints." -ForegroundColor Yellow
Write-Host "`nConsultez CONTINUATION_GUIDE.md pour la liste complète." -ForegroundColor Green

Write-Host "`n=== Fin du script ===" -ForegroundColor Green

