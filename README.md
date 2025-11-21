# Loutaupia V2 - API de Jeu de Piste 🎮

API REST en .NET 9 pour une plateforme de jeu de piste avec système d'économie virtuelle, suivant une **Vertical Slice Architecture** avec **Screaming Architecture**.

## 📋 Table des Matières

- [Vue d'ensemble](#vue-densemble)
- [Technologies](#technologies)
- [Architecture](#architecture)
- [État du Projet](#état-du-projet)
- [Démarrage Rapide](#démarrage-rapide)
- [Documentation](#documentation)

## 🎯 Vue d'ensemble

Loutaupia V2 est une plateforme de jeu de piste qui inclut:
- 👤 Gestion des joueurs (inscription, authentification JWT)
- 🎒 Système d'inventaire
- 💰 Économie virtuelle (monnaie, transactions)
- 🏛️ Hôtel des ventes (enchères entre joueurs)
- 🎯 Système de quêtes (à venir)

## 🛠️ Technologies

- **.NET 9.0** - Framework principal
- **PostgreSQL 16** - Base de données
- **Entity Framework Core 9.0** - ORM
- **JWT** - Authentification
- **BCrypt** - Hashing des mots de passe
- **FluentValidation** - Validation des entrées
- **MediatR** - Pattern CQRS (prévu)
- **Serilog** - Logging
- **Swagger/OpenAPI** - Documentation API
- **Docker** - Conteneurisation

## 🏗️ Architecture

### Vertical Slice Architecture
Chaque fonctionnalité est auto-contenue dans son propre dossier avec:
- `Endpoint.cs` - Point d'entrée API
- `UseCase.cs` - Logique métier
- `Request.cs` - DTO d'entrée
- `Response.cs` - DTO de sortie
- `Validator.cs` - Règles de validation (optionnel)

### Structure du Projet
```
src/
├── Api/Extensions/          # Configuration des services
├── Core/                    # Cœur du domaine
│   ├── Domain/             # Entités, Value Objects, Exceptions
│   └── Contracts/          # Interfaces (Repositories, Services)
├── Features/               # Tranches verticales par fonctionnalité
│   ├── Players/
│   ├── Inventory/
│   ├── Currency/
│   └── AuctionHouse/
└── Infrastructure/         # Implémentations techniques
    ├── Authentication/     # JWT, Hashing
    └── Persistence/        # EF Core, Repositories
```

## 📊 État du Projet

### ✅ Complété
- [x] Configuration du projet (.NET 9, packages NuGet)
- [x] Structure de dossiers (Vertical Slice Architecture)
- [x] Configuration Docker (docker-compose.yml, Dockerfile)
- [x] Configuration de base (appsettings.json)
- [x] Exceptions et Value Objects du domaine
- [x] Interfaces des services (JWT, PasswordHasher)
- [x] Documentation (Specs.md, guides)

### 🚧 En cours
- [ ] Entités du domaine (Player, Inventory, Artefact, etc.)
- [ ] Repositories (interfaces et implémentations)
- [ ] Infrastructure (DbContext, configurations EF Core)
- [ ] Authentication (implémentation JWT, BCrypt)
- [ ] Features Players (CreatePlayer, Login, GetProfile)
- [ ] Migrations EF Core

### 📝 Prévu
- [ ] Features Inventory
- [ ] Features Currency
- [ ] Features AuctionHouse
- [ ] Features Quests
- [ ] Tests unitaires et d'intégration
- [ ] Frontend React TypeScript

## 🚀 Démarrage Rapide

### Prérequis
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optionnel)
- [PostgreSQL 16](https://www.postgresql.org/download/) (si pas Docker)

### Installation

```bash
# Cloner le repository
git clone https://github.com/votre-username/Loutaupia-V2-dotnet-api.git
cd Loutaupia-V2-dotnet-api

# Restaurer les packages
dotnet restore

# Configurer la base de données (éditer appsettings.json si nécessaire)
# Puis créer et appliquer les migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# Lancer l'API
dotnet run
```

L'API sera disponible sur `http://localhost:5000`  
Swagger UI: `http://localhost:5000/swagger`

### Avec Docker

```bash
# Créer un fichier .env à partir de .env.example
cp .env.example .env

# Lancer tous les services
docker-compose up --build

# L'API sera disponible sur http://localhost:5000
```

## 📚 Documentation

- **[Specs.md](./Specs.md)** - Spécifications techniques complètes
- **[CONTINUATION_GUIDE.md](./CONTINUATION_GUIDE.md)** - Guide pour continuer le développement
- **[IMPLEMENTATION_STATUS.md](./IMPLEMENTATION_STATUS.md)** - État d'avancement détaillé

## 🔐 Configuration

### Base de Données
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=loutaupia_db;Username=loutaupia_admin;Password=VotreMotDePasse"
  }
}
```

### JWT
```json
{
  "Jwt": {
    "Secret": "VotreCléSecrèteDe32CaractèresMinimum",
    "Issuer": "LoutaupiaV2API",
    "Audience": "LoutaupiaV2Client"
  }
}
```

## 🤝 Contribution

1. Fork le projet
2. Créer une branche (`git checkout -b feature/AmazingFeature`)
3. Commit vos changements (`git commit -m 'Add some AmazingFeature'`)
4. Push vers la branche (`git push origin feature/AmazingFeature`)
5. Ouvrir une Pull Request

## 📝 License

Ce projet est sous licence MIT. Voir le fichier `LICENSE` pour plus de détails.

## 👤 Auteur

Victor - [GitHub Profile](https://github.com/votre-username)

## 🙏 Remerciements

- Inspiration: Clean Architecture, Vertical Slice Architecture
- Technologies: .NET, PostgreSQL, Docker
Projet Backend en dotnet qui gèrera une partir de l'application Loutaupia V2
