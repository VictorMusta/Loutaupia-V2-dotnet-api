# Development Guide: Loutaupia-V2-dotnet-api

## Prerequisites
Pour travailler sur ce projet, vous devez avoir installé :
- **.NET 8.0 SDK** ([dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Docker Desktop** (pour les tests de containerisation)
- **Visual Studio 2022** ou **JetBrains Rider** (recommandé)

## Setup and Installation
1. Clonez le repository.
2. Restaurez les packages NuGet :
   ```bash
   dotnet restore
   ```

## Development Commands
### Run the Application (Local)
Démarrez l'API en mode développement avec hot-reload :
```bash
dotnet watch run --project LootopiAPI/API/API.csproj
```

### Build the Project
Compilez la solution pour vérifier les erreurs :
```bash
dotnet build
```

### Docker
Générez l'image Docker locale :
```bash
docker build -t lootopia-api ./LootopiAPI/API
```

## API Documentation (Swagger)
En mode développement, l'interface Swagger est accessible à l'adresse suivante (par défaut) :
- `https://localhost:7165/swagger/index.html` (Vérifiez le port dans `launchSettings.json`)

## Code Style & Standards
- Utilisez le **PascalCase** pour les noms de classes et de méthodes.
- Respectez la structure **Vertical Slice** : placez le nouveau code dans `Features/[NomDeLaFeature]`.
- Implémentez l'injection de dépendances via les constructeurs.
