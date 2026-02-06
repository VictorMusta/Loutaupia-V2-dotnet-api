# Source Tree Analysis: Loutaupia-V2-dotnet-api

## Project Structure Overview
Le projet suit une structure moderne de type **Micro-Frontend/Service** appliquée au backend, organisée par fonctionnalités (**Features**). Cette approche permet une meilleure scalabilité et une isolation des domaines métiers.

## Annotated Directory Tree
```text
LootopiAPI/
├── API/                    # Dossier principal de l'API
│   ├── Features/           # Logique métier organisée par domaine
│   │   ├── Authentication/ # Gestion des accès et JWT (Hypothèse)
│   │   ├── Users/          # Gestion des profils utilisateurs (Hypothèse)
│   │   └── ...            # Autres fonctionnalités
│   ├── Infrastructure/     # Configuration technique globale
│   │   ├── Data/           # Accès aux données (Entity Framework, etc.)
│   │   ├── Security/       # Middleware et gardes de sécurité
│   │   └── ...
│   ├── Model/              # Entités partagées et DTOs
│   ├── Properties/         # Configuration de lancement (launchSettings.json)
│   ├── Program.cs          # Point d'entrée de l'application (Bootstrapper)
│   ├── appsettings.json    # Configuration de l'environnement
│   └── Dockerfile          # Configuration de containerisation
├── LootopiAPI.slnx         # Fichier de solution (Solution Explorer)
└── .dockerignore           # Exclusions pour Docker
```

## Critical Directories
| Directory | Purpose |
| :--- | :--- |
| `API/Features/` | Contient le code métier. Chaque sous-dossier représente une fonctionnalité complète (Vertical Slice). |
| `API/Infrastructure/` | Contient la plomberie technique (Persistence, Logging, Auth filters). |
| `API/Model/` | Définit les objets de données circulant dans le système. |

## Entry Points
- **Program.cs**: Configure l'injection de dépendances, le pipeline de middleware HTTP et démarre l'écouteur d'API.
