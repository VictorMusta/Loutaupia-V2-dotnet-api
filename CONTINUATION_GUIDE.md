# Guide de Continuation - Loutaupia V2 API

## État Actuel ✅

### Complété
1. **Configuration du projet**
   - ✅ Packages NuGet installés (EF Core, PostgreSQL, JWT, BCrypt, FluentValidation, MediatR, Serilog)
   - ✅ `appsettings.json` configuré
   - ✅ `docker-compose.yml` créé
   - ✅ Structure de dossiers créée

2. **Fichiers de base créés**
   - ✅ `DomainException.cs`
   - ✅ `Rarity.cs`, `ArtefactCategory.cs`, `AuctionStatus.cs`
   - ✅ `Result.cs` (pattern Result)
   - ✅ `IJwtService.cs`, `IPasswordHasher.cs`

## Prochaines Étapes 🚀

### 1. Créer les Entités du Domaine
Créer dans `src/Core/Domain/Entities/`:
- `Player.cs` - avec validations défensives (username 3-20 chars, email format, etc.)
- `Inventory.cs` - avec MaxSlots (10-500)
- `Artefact.cs` - avec quantité > 0
- `ArtefactDefinition.cs`
- `CurrencyWallet.cs` - avec GoldCoins >= 0
- `AuctionListing.cs` - avec validations des prix

### 2. Créer les Interfaces de Repository
Créer dans `src/Core/Contracts/Repositories/`:
- `IPlayerRepository.cs`
- `IInventoryRepository.cs`
- `IArtefactRepository.cs`
- `IArtefactDefinitionRepository.cs`
- `ICurrencyWalletRepository.cs`
- `IAuctionListingRepository.cs`

### 3. Créer l'Infrastructure
- `src/Infrastructure/Authentication/JwtService.cs`
- `src/Infrastructure/Authentication/PasswordHasher.cs`
- `src/Infrastructure/Persistence/ApplicationDbContext.cs`
- `src/Infrastructure/Persistence/Configurations/*.cs` (pour chaque entité)
- `src/Infrastructure/Persistence/Repositories/*.cs` (implémentations)

### 4. Créer les Features (Vertical Slices)
Pour chaque feature, créer 4 fichiers:

**Players/CreatePlayer/**
- `CreatePlayerRequest.cs` - record avec Username, Email, Password
- `CreatePlayerResponse.cs` - record avec PlayerId, Username, Email, Token
- `CreatePlayerValidator.cs` - FluentValidation
- `CreatePlayerUseCase.cs` - logique métier
- `CreatePlayerEndpoint.cs` - endpoint API

**Players/AuthenticatePlayer/**
- `AuthenticatePlayerRequest.cs`
- `AuthenticatePlayerResponse.cs`
- `AuthenticatePlayerUseCase.cs`
- `AuthenticatePlayerEndpoint.cs`

**Players/GetPlayerProfile/**
- `GetPlayerProfileResponse.cs`
- `GetPlayerProfileUseCase.cs`
- `GetPlayerProfileEndpoint.cs`

### 5. Créer les Extensions
- `src/Api/Extensions/ServiceCollectionExtensions.cs` - enregistrement des services
- `src/Api/Extensions/WebApplicationExtensions.cs` - middlewares et endpoints

### 6. Créer et Appliquer les Migrations
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 7. Tester l'API
```bash
dotnet run
# Ouvrir http://localhost:5000/swagger
```

## Commandes Utiles 📝

### Développement
```bash
# Restaurer les packages
dotnet restore

# Compiler
dotnet build

# Nettoyer
dotnet clean

# Lancer l'API
dotnet run

# Watch mode (recompile automatiquement)
dotnet watch run
```

### Entity Framework
```bash
# Créer une migration
dotnet ef migrations add [NomDeLaMigration]

# Appliquer les migrations
dotnet ef database update

# Supprimer la dernière migration
dotnet ef migrations remove

# Lister les migrations
dotnet ef migrations list
```

### Docker
```bash
# Construire et lancer
docker-compose up --build

# Lancer en arrière-plan
docker-compose up -d

# Arrêter
docker-compose down

# Voir les logs
docker-compose logs -f backend
```

## Structure Complète Attendue 📁

```
Loutaupia-V2-dotnet-api/
├── src/
│   ├── Api/
│   │   └── Extensions/
│   │       ├── ServiceCollectionExtensions.cs
│   │       └── WebApplicationExtensions.cs
│   ├── Core/
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── Player.cs
│   │   │   │   ├── Inventory.cs
│   │   │   │   ├── Artefact.cs
│   │   │   │   ├── ArtefactDefinition.cs
│   │   │   │   ├── CurrencyWallet.cs
│   │   │   │   └── AuctionListing.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── Result.cs
│   │   │   │   ├── Rarity.cs
│   │   │   │   ├── ArtefactCategory.cs
│   │   │   │   └── AuctionStatus.cs
│   │   │   └── Exceptions/
│   │   │       └── DomainException.cs
│   │   └── Contracts/
│   │       ├── Repositories/
│   │       │   ├── IPlayerRepository.cs
│   │       │   ├── IInventoryRepository.cs
│   │       │   ├── IArtefactRepository.cs
│   │       │   ├── IArtefactDefinitionRepository.cs
│   │       │   ├── ICurrencyWalletRepository.cs
│   │       │   └── IAuctionListingRepository.cs
│   │       └── Services/
│   │           ├── IJwtService.cs
│   │           └── IPasswordHasher.cs
│   ├── Features/
│   │   └── Players/
│   │       ├── CreatePlayer/
│   │       │   ├── CreatePlayerRequest.cs
│   │       │   ├── CreatePlayerResponse.cs
│   │       │   ├── CreatePlayerValidator.cs
│   │       │   ├── CreatePlayerUseCase.cs
│   │       │   └── CreatePlayerEndpoint.cs
│   │       ├── AuthenticatePlayer/
│   │       │   ├── AuthenticatePlayerRequest.cs
│   │       │   ├── AuthenticatePlayerResponse.cs
│   │       │   ├── AuthenticatePlayerUseCase.cs
│   │       │   └── AuthenticatePlayerEndpoint.cs
│   │       └── GetPlayerProfile/
│   │           ├── GetPlayerProfileResponse.cs
│   │           ├── GetPlayerProfileUseCase.cs
│   │           └── GetPlayerProfileEndpoint.cs
│   └── Infrastructure/
│       ├── Authentication/
│       │   ├── JwtService.cs
│       │   └── PasswordHasher.cs
│       └── Persistence/
│           ├── ApplicationDbContext.cs
│           ├── Configurations/
│           │   ├── PlayerConfiguration.cs
│           │   ├── InventoryConfiguration.cs
│           │   ├── ArtefactConfiguration.cs
│           │   ├── ArtefactDefinitionConfiguration.cs
│           │   ├── CurrencyWalletConfiguration.cs
│           │   └── AuctionListingConfiguration.cs
│           └── Repositories/
│               ├── PlayerRepository.cs
│               ├── InventoryRepository.cs
│               ├── ArtefactRepository.cs
│               ├── ArtefactDefinitionRepository.cs
│               ├── CurrencyWalletRepository.cs
│               └── AuctionListingRepository.cs
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── docker-compose.yml
├── Dockerfile
├── .dockerignore
├── .env.example
├── Specs.md
├── IMPLEMENTATION_STATUS.md
└── CONTINUATION_GUIDE.md (ce fichier)
```

## Points Importants ⚠️

1. **Namespaces**: Utiliser des file-scoped namespaces (avec `;`) pour compatibilité avec .NET 9
2. **Records**: Utiliser `record` pour les DTOs (Request/Response)
3. **Async/Await**: Toutes les méthodes de repository doivent être async
4. **Result Pattern**: Ne pas lancer d'exceptions dans les repositories, retourner Result<T>
5. **Defensive Code**: Valider dans les setters des entités
6. **FluentValidation**: Valider les requests avant les use cases

## Configuration BDD 🔧

**Connection String (appsettings.json):**
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=loutaupia_db;Username=loutaupia_admin;Password=YourSecurePassword123!"
}
```

**JWT Configuration:**
```json
"Jwt": {
  "Secret": "YourSuperSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong",
  "Issuer": "LoutaupiaV2API",
  "Audience": "LoutaupiaV2Client"
}
```

## Endpoints de l'API 🌐

Une fois complété, l'API exposera:

### Players
- `POST /api/players/register` - Créer un compte
- `POST /api/players/login` - Se connecter
- `GET /api/players/profile` - Obtenir son profil (authentifié)

### Inventory (à implémenter)
- `GET /api/inventory` - Lister son inventaire
- `POST /api/inventory/items` - Ajouter un item
- `DELETE /api/inventory/items/{id}` - Retirer un item

### Currency (à implémenter)
- `GET /api/currency/balance` - Consulter son solde
- `POST /api/currency/transfer` - Transférer de l'argent

### AuctionHouse (à implémenter)
- `GET /api/auctions` - Lister les enchères actives
- `POST /api/auctions` - Créer une enchère
- `POST /api/auctions/{id}/bid` - Enchérir
- `POST /api/auctions/{id}/buyout` - Achat immédiat

## Aide Supplémentaire 💡

Pour reprendre le développement:

1. Ouvrir le projet dans JetBrains Rider ou Visual Studio
2. Restaurer les packages: `dotnet restore`
3. Créer les entités manquantes en suivant `Specs.md`
4. Créer les repositories
5. Créer l'infrastructure (DbContext, configurations)
6. Créer les features une par une
7. Tester avec Swagger

Bon courage! 🚀

