# Spécification Technique - API Plateforme de Jeu de Piste

## Vue d'ensemble du projet

Créer une API REST en .NET 8 pour gérer une plateforme de jeu de piste appelée "Loutaupia V2" avec système d'économie virtuelle, suivant une architecture en tranches verticales (Vertical Slice Architecture) avec séparation des use cases (Screaming Architecture).

## Architecture du projet

### Structure de dossiers

```
src/
├── Api/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Dockerfile
│   └── Extensions/
│       ├── ServiceCollectionExtensions.cs
│       └── WebApplicationExtensions.cs
├── Core/
│   ├── Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Exceptions/
│   └── Contracts/
│       ├── Repositories/
│       └── Services/
├── Features/
│   ├── Players/
│   │   ├── CreatePlayer/
│   │   │   ├── CreatePlayerEndpoint.cs
│   │   │   ├── CreatePlayerUseCase.cs
│   │   │   ├── CreatePlayerRequest.cs
│   │   │   └── CreatePlayerResponse.cs
│   │   ├── GetPlayerProfile/
│   │   ├── UpdatePlayerProfile/
│   │   └── DeletePlayer/
│   ├── Inventory/
│   │   ├── AddItem/
│   │   ├── RemoveItem/
│   │   ├── GetInventory/
│   │   └── TransferItem/
│   ├── AuctionHouse/
│   │   ├── CreateListing/
│   │   ├── PlaceBid/
│   │   ├── BuyNow/
│   │   ├── GetListings/
│   │   └── CancelListing/
│   ├── Currency/
│   │   ├── GetBalance/
│   │   ├── AddCurrency/
│   │   ├── DeductCurrency/
│   │   └── TransferCurrency/
└── Infrastructure/
    ├── Persistence/
    │   ├── ApplicationDbContext.cs
    │   ├── Configurations/
    │   └── Repositories/
    ├── Authentication/
    │   ├── JwtService.cs
    │   └── AuthenticationHandler.cs
    └── Migrations/
```

## Entités du domaine

### 1. Player (Joueur)

**Propriétés:**
- `PlayerId` (Guid) - Identifiant unique
- `Username` (string) - Nom d'utilisateur (3-20 caractères, unique)
- `Email` (string) - Email valide
- `PasswordHash` (string) - Hash sécurisé du mot de passe
- `CreatedAt` (DateTime)

**Validations défensives:**
- Username non vide, longueur valide, caractères alphanumériques uniquement
- Email format valide

### 2. Inventory (Inventaire)

**Propriétés:**
- `InventoryId` (Guid)
- `PlayerId` (Guid)
- `Items` (IReadOnlyCollection<Artefact>)

**Validations:**
- MaxSlots >= 10 et <= 500
- Vérifier que le nombre d'items ne dépasse pas MaxSlots

### 3. Artefact

**Propriétés:**
- `ArtefactId` (Guid)
- `ArtefactDefinition` (Guid) - Référence vers ItemDefinition
- `Quantity` (int)

**Validations:**
- Quantity > 0

### 4. ArtefactDefinition (Définition d'artefact)

**Propriétés:**
- `ArtefactDefinitionId` (Guid)
- `Name` (string)
- `Description` (string)
- `Rarity` (enum: Common, Uncommon, Rare, Epic, Legendary)
- `Category` (enum: Weapon, Armor, Consumable, QuestItem, Material)

**Validations:**
- Name non vide

### 5. CurrencyWallet (Portefeuille de monnaie)

**Propriétés:**
- `WalletId` (Guid)
- `PlayerId` (Guid)
- `GoldCoins` (long) - Monnaie principale
- `LastUpdated` (DateTime)

**Validations:**
- GoldCoins >= 0 et <= long.MaxValue
- PremiumGems >= 0
- Implémenter des transactions atomiques pour éviter les race conditions

### 6. AuctionListing (Annonce de vente)

**Propriétés:**
- `ListingId` (Guid)
- `SellerId` (Guid)
- `ItemId` (Guid)
- `Quantity` (int)
- `StartingPrice` (decimal)
- `BuyoutPrice` (decimal?)
- `CurrentBid` (decimal?)
- `CurrentBidderId` (Guid?)
- `ExpiresAt` (DateTime)
- `Status` (enum: Active, Sold, Expired, Cancelled)
- `CreatedAt` (DateTime)

**Validations:**
- StartingPrice > 0
- BuyoutPrice > StartingPrice (si défini)
- ExpiresAt > DateTime.UtcNow
- Quantity > 0


## Use Cases par feature

### Players

1. **CreatePlayerUseCase**: Créer un nouveau joueur avec validation email unique
2. **AuthenticatePlayerUseCase**: Authentifier et générer JWT
3. **GetPlayerProfileUseCase**: Récupérer le profil complet
4. **UpdatePlayerProfileUseCase**: Modifier informations non sensibles
5. **ChangePasswordUseCase**: Changer le mot de passe avec validation

### Inventory

1. **GetPlayerInventoryUseCase**: Liste paginée de l'inventaire
2. **AddItemToInventoryUseCase**: Ajouter un item avec vérification d'espace
3. **RemoveItemFromInventoryUseCase**: Retirer un item
4. **TransferItemUseCase**: Transférer vers un autre joueur (vérifier IsBound)
5. **UseConsumableItemUseCase**: Utiliser un item consommable

### AuctionHouse

1. **CreateAuctionListingUseCase**: Créer une annonce (retirer l'item de l'inventaire)
2. **PlaceBidUseCase**: Enchérir (vérifier fonds, rembourser ancien enchérisseur)
3. **BuyoutAuctionUseCase**: Achat immédiat
4. **GetAuctionListingsUseCase**: Liste filtrée/triée des annonces actives
5. **CancelAuctionListingUseCase**: Annuler une annonce (rendre l'item)
6. **ProcessExpiredAuctionsUseCase**: Job de fond pour finaliser les ventes

### Currency

1. **GetCurrencyBalanceUseCase**: Consulter les soldes
2. **AddCurrencyUseCase**: Ajouter de la monnaie (admin/rewards)
3. **DeductCurrencyUseCase**: Retirer de la monnaie (achats)
4. **TransferCurrencyUseCase**: Transférer entre joueurs (avec frais?)
5. **GetTransactionHistoryUseCase**: Historique des transactions

## Sécurité

### Authentication & Authorization

- **JWT Bearer Token**: Expiration 24h, refresh token 7 jours
- **Password hashing**: Utiliser BCrypt avec salt factor >= 12
- **Rate limiting**: Limiter les requêtes par IP/utilisateur
- **CORS**: Configuration stricte pour le frontend
- **HTTPS only**: Forcer HTTPS en production

### Validations

- **Input validation**: FluentValidation pour tous les requests
- **SQL Injection**: Utiliser EF Core avec requêtes paramétrées
- **XSS**: Encoder les sorties
- **CSRF**: Tokens CSRF pour les actions sensibles

### Audit & Logging

- Logger toutes les transactions de monnaie
- Logger les actions sensibles (changement mot de passe, transferts)
- Implémenter un système d'audit trail pour les entités critiques

## Base de données PostgreSQL

### Configuration

```yaml
# docker-compose.yml snippet
postgres:
  image: postgres:16-alpine
  environment:
    POSTGRES_DB: treasure_hunt_db
    POSTGRES_USER: treasure_admin
    POSTGRES_PASSWORD: ${DB_PASSWORD}
  ports:
    - "5432:5432"
  volumes:
    - postgres_data:/var/lib/postgresql/data
```

### Indexes recommandés

- `Player.Username` (unique)
- `Player.Email` (unique)
- `InventoryItem.PlayerId`
- `AuctionListing.Status`, `AuctionListing.ExpiresAt`
- `CurrencyTransaction.PlayerId`, `CurrencyTransaction.CreatedAt`

### Migrations

Utiliser EF Core Migrations pour la gestion du schéma

## Docker Compose complet

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: treasure_hunt_db
      POSTGRES_USER: treasure_admin
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U treasure_admin"]
      interval: 10s
      timeout: 5s
      retries: 5

  backend:
    build:
      context: ./src/Api
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Host=postgres;Database=treasure_hunt_db;Username=treasure_admin;Password=${DB_PASSWORD}"
      Jwt__Secret: ${JWT_SECRET}
      Jwt__Issuer: "TreasureHuntAPI"
      Jwt__Audience: "TreasureHuntClient"
    depends_on:
      postgres:
        condition: service_healthy
    restart: unless-stopped

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    ports:
      - "3000:3000"
    environment:
      REACT_APP_API_URL: "http://localhost:5000"
    depends_on:
      - backend
    restart: unless-stopped

volumes:
  postgres_data:
```

## Bonnes pratiques de code

### Code défensif dans les entités

```csharp
// Exemple pour Player
public class Player
{
    private string _username;
    
    public string Username
    {
        get => _username;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Username cannot be empty");
            
            if (value.Length < 3 || value.Length > 20)
                throw new DomainException("Username must be between 3 and 20 characters");
            
            if (!Regex.IsMatch(value, "^[a-zA-Z0-9_]+$"))
                throw new DomainException("Username can only contain alphanumeric characters and underscores");
            
            _username = value;
        }
    }
}
```

### Pattern Repository

- Un repository par agrégat racine
- Méthodes asynchrones uniquement
- Retourner des `Result<T>` plutôt que lancer des exceptions

### CQRS léger

- Séparer les commandes (write) des queries (read)
- Utiliser MediatR pour dispatcher les use cases
- Optimiser les queries avec projections EF Core

### Tests

- Tests unitaires pour les use cases
- Tests d'intégration pour les repositories
- Tests end-to-end pour les endpoints critiques

## Frontend React (TypeScript)

### Structure suggérée

```
frontend/
├── src/
│   ├── components/
│   │   ├── player/
│   │   ├── inventory/
│   │   ├── auction-house/
│   │   └── quests/
│   ├── services/
│   │   └── api/
│   ├── hooks/
│   ├── contexts/
│   │   └── AuthContext.tsx
│   ├── types/
│   └── App.tsx
├── Dockerfile
└── package.json
```

### Features frontend prioritaires

1. Authentification (login/register)
2. Dashboard joueur avec statistiques
3. Gestion d'inventaire avec drag & drop
4. Hôtel des ventes avec filtres/recherche
5. Liste et suivi des quêtes actives
6. Affichage du solde de monnaies