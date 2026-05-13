# 🗺️ Lootopia — Plateforme de Chasse aux Trésors Géo-localisée en Réalité Augmentée

**Lootopia** est une application web immersive combinant exploration physique, mécanismes de jeu old-school et économie virtuelle. Conçue autour d'une direction artistique **rétro-organique et lumineuse**, elle invite les joueurs à explorer leur environnement réel pour dénicher des indices, valider des étapes GPS et remporter des tokens échangeables sur un marché intégré.

---

## ✨ Aperçu de l'Application

Voici un aperçu de l'interface de jeu en mode clair rétro-organique (sans glassmorphisme, avec un design solide et tactile) :

### 📍 Carte d'Exploration en Direct
![Aperçu de la Carte Lootopia](file:///C:/Users/v.grabowski/.gemini/antigravity/brain/862620f2-935e-4b19-a789-e1c19dcd3659/lootopia_app_preview_1778660194941.png)

### 🛒 Marché & Économie Virtuelle
![Aperçu du Marché Lootopia](file:///C:/Users/v.grabowski/.gemini/antigravity/brain/862620f2-935e-4b19-a789-e1c19dcd3659/lootopia_marketplace_preview_1778660219235.png)

---

## 🛠️ Stack Technique

Le projet repose sur une architecture moderne séparant clairement le client riche et l'API robuste orientée tranches fonctionnelles (Vertical Slice Architecture) :

- **Frontend** : React 19 + TypeScript + Vite + Tailwind CSS (Thème rétro-organique texturé, sans flou/transparence)
- **Cartographie** : Leaflet + React-Leaflet + Suivi de géolocalisation haute fidélité
- **Backend API** : .NET 9 WebAPI (Minimal APIs)
- **Base de Données** : PostgreSQL 17 + extension spatiale **PostGIS 3.5**
- **ORM & Données Spatiales** : Entity Framework Core 9 (Code First) + NetTopologySuite
- **Communication & Flux** : MediatR (Pattern CQRS) + FluentValidation
- **Sécurité** : JWT (JSON Web Tokens) + Magic Links B2B

---

## ⚙️ Architecture du Projet

```text
Lootopia/
├── Lootopia.Api/         # Backend API .NET 9
│   ├── Domain/           # Entités du domaine, Énums, Objets de valeur
│   ├── Features/         # Slices verticaux (Screaming Architecture)
│   │   ├── Auth/         # Authentification, Inscription, Invités, Magic Links
│   │   ├── Hunts/        # Gestion des chasses, validation par coordonnées GPS, Quotas
│   │   ├── Marketplace/  # Annonces, achats directs sécurisés
│   │   └── ...           # Portefeuille, Inventaire, Partenaires, Admin, Classements
│   └── Infrastructure/   # Contexte EF Core, Services externes, Intercepteurs
│
├── Lootopia.Client/      # Frontend Web React / Vite
│   ├── src/app/          # Pages de l'application (Play, Auth, Admin)
│   ├── src/shared/       # Composants UI réutilisables, API clients, Providers GPS
│   └── index.css         # Design système global rétro-organique
│
└── docker-compose.yml    # Déclaration des services d'infrastructure (Postgres + PostGIS)
```

---

<div className="bg-card border-2 border-border p-6 rounded-xl my-8 shadow-md">

## 🤝 Guide de Contribution & Utilisation du Makefile

Pour simplifier au maximum le développement local et la collaboration, le projet utilise un **Makefile** à la racine.

> **💡 Qu'est-ce qu'un Makefile ?**  
> Un **Makefile** est un fichier lu par l'utilitaire `make` qui rassemble et automatise des lignes de commande courantes sous forme de **cibles** (ex: `make dev`). Cela vous évite de devoir retenir ou saisir manuellement des commandes complexes pour lancer Docker, compiler l'API .NET ou démarrer le client Node.js.

### 🚀 Lancement Rapide en 1 Commande

Pour démarrer l'ensemble de l'environnement de développement en parallèle (Base de données Docker en fond, API .NET en mode Hot Reload, et Client React) :

### Using Docker (Recommended)
Run the complete application stack with one command:

```bash
# Start all services (Database + API + Frontend)
docker-compose up -d --build

# Access the application:
# Frontend: http://localhost:3000
# API: http://localhost:8080
# API Health: http://localhost:8080/health

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

See [DOCKER.md](DOCKER.md) for detailed Docker documentation.

### Manual Setup

```bash
# Start PostgreSQL + PostGIS only
docker compose up db -d

# Run the API
cd Lootopia.Api
dotnet run

# Run the Frontend (in another terminal)
cd Lootopia.Client
npm install
npm run dev

# Open Swagger UI
# http://localhost:5000/swagger
```

### 📋 Principales Commandes Disponibles

#### 🐳 Infrastructure (Docker)
- `make up` : Lance l'infrastructure complète (API + DB) via les conteneurs Docker Compose.
- `make down` : Arrête et supprime les conteneurs de l'infrastructure.
- `make db-up` : Lance uniquement le moteur de base de données PostgreSQL en arrière-plan.
- `make db-logs` : Affiche les journaux (logs) en direct de la base de données.

#### 🖥️ Backend (.NET)
- `make restore` : Restaure les dépendances NuGet (.NET) et les paquets NPM (React).
- `make build` : Compile la solution .NET globale pour valider le code.
- `make dev-api` : Démarre l'API en mode surveillance (`dotnet watch`) avec rechargement automatique à chaque modification.
- `make ef-update` : Applique les dernières migrations Entity Framework sur la base de données.

#### 🎨 Frontend (React)
- `make dev-client` : Démarre le serveur de développement local Vite pour le client web.
- `make build-client` : Compile et optimise les ressources statiques pour la production.

### 📝 Mini-Tutoriel pour Contribuer

1. **Cloner le dépôt** et vous positionner sur la branche principale :
   ```bash
   git clone <url-du-depot>
   cd Lootopia
   ```
2. **Lancer l'environnement de développement** :
   ```bash
   make dev
   ```
3. **Créer une branche de fonctionnalité** :
   ```bash
   git checkout -b feature/ma-nouvelle-fonctionnalite
   ```
4. **Développer et tester** :
   - Les modifications sur les fichiers C# rechargent automatiquement l'API.
   - Les modifications sur les composants React/TSX sont appliquées instantanément dans le navigateur grâce au HMR de Vite.
5. **Soumettre une Pull Request** :
   Assurez-vous que le projet compile correctement (`make build`) et que le code respecte l'esthétique lumineuse et rétro du design système.

</div>

---

## 🔐 Utilisateurs de Démonstration (Seeding)

La base de données est initialisée avec des comptes de test prêts à l'emploi :

| Rôle | Adresse E-mail | Mot de passe | Description |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@lootopia.io` | `Admin123!` | Accès au back-office de gestion, alertes fraudes et création de chasses |
| **Partner** | `partner@lootopia.io` | `Partner123!` | Commanditaires B2B finançant des campagnes publicitaires géolocalisées |
| **Player** | `player@lootopia.io` | `Player123!` | Joueur standard accédant à la carte, à l'inventaire et aux transactions du marché |

---

## 🎯 Règles Métier Avancées

- **Validation GPS Chirurgicale** : La validation des étapes de chasse utilise la formule de Haversine et des requêtes PostGIS natives pour s'assurer que le joueur se trouve physiquement dans le rayon de l'indice.
- **Quotas de Récompenses** : Chaque chasse possède un quota défini de gagnants. Une fois ce nombre de joueurs récompensés atteint, la chasse reste jouable **pour le fun et la gloire**, attribuant un message de victoire personnalisé sans générer d'inflation de tokens.
- **Sécurité des Transactions** : Le module MediatR gère les achats sur le marché via des transactions de base de données garantissant la cohérence absolue entre le débit de l'acheteur, le crédit du vendeur et le transfert d'inventaire.
