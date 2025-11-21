# Script pour créer l'architecture complète du projet

# Créer la structure de dossiers
$dirs = @(
    "src/Core/Domain/Entities",
    "src/Core/Domain/ValueObjects", 
    "src/Core/Domain/Exceptions",
    "src/Core/Contracts/Repositories",
    "src/Core/Contracts/Services",
    "src/Infrastructure/Persistence/Configurations",
    "src/Infrastructure/Persistence/Repositories",
    "src/Infrastructure/Authentication",
    "src/Features/Players/CreatePlayer",
    "src/Features/Players/AuthenticatePlayer",
    "src/Features/Players/GetPlayerProfile",
    "src/Api/Extensions"
)

foreach ($dir in $dirs) {
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
}

Write-Host "Structure de dossiers créée avec succès!"

