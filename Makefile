.PHONY: help dev up down db-up db-down db-logs restore build test clean dev-api dev-client build-client ef-update

# --- Variables ---
SLN = Lootopia.sln
API_DIR = Lootopia.Api
CLIENT_DIR = Lootopia.Client

# --- Raccourcis d'aide ---
help: ## Affiche les commandes disponibles
	@echo "=== Commandes de developpement Lootopia ==="
	@echo "Lancement global local :"
	@echo "  make dev          : DEMARRE LES 3 EN PARALLELE (DB en fond, API Hot Reload + Client React)"
	@echo ""
	@echo "Infrastructure Docker :"
	@echo "  make up           : Lance l'infrastructure complete (API + DB) via Docker Compose"
	@echo "  make down         : Arrete et supprime les conteneurs Docker Compose"
	@echo "  make db-up        : Lance uniquement la base de donnees PostgreSQL en fond"
	@echo "  make db-down      : Arrete le conteneur de la base de donnees"
	@echo "  make db-logs      : Affiche les logs en direct de la base de donnees"
	@echo ""
	@echo "Backend (.NET) :"
	@echo "  make restore      : Restaure les paquets NuGet (.NET) et NPM (React)"
	@echo "  make build        : Compile la solution .NET"
	@echo "  make test         : Execute les tests unitaires"
	@echo "  make dev-api      : Lance l'API .NET avec rechargement automatique (watch)"
	@echo "  make ef-update    : Applique les migrations Entity Framework en base de donnees"
	@echo ""
	@echo "Frontend (React/Vite) :"
	@echo "  make dev-client   : Lance le serveur de developpement React/Vite"
	@echo "  make build-client : Compile les assets statiques de production du frontend"
	@echo "  make clean        : Nettoie les dossiers de build (bin, obj, dist)"


# --- Lancement Local Parallèle ---
dev: db-up
	@echo "=== Lancement de l'API (.NET) et du Client (Vite) en parallele ==="
	$(MAKE) -j2 dev-api dev-client

# --- Docker & Base de données ---
up:
	docker compose up -d

down:
	docker compose down

db-up:
	docker compose up -d db

db-down:
	docker compose stop db

db-logs:
	docker compose logs -f db

# --- Global ---
restore:
	dotnet restore $(SLN)
	cd $(CLIENT_DIR) && npm install

build:
	dotnet build $(SLN)

test:
	dotnet test $(SLN)

clean:
	dotnet clean $(SLN)

# --- Backend (.NET) ---
dev-api:
	dotnet watch run --project $(API_DIR)/Lootopia.Api.csproj

ef-update:
	dotnet ef database update --project $(API_DIR)/Lootopia.Api.csproj

# --- Frontend (React / Vite) ---
dev-client:
	cd $(CLIENT_DIR) && npm run dev

build-client:
	cd $(CLIENT_DIR) && npm run build
