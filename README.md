# Lootopia Backend API

Geolocated AR Treasure Hunt Platform - Backend API built with .NET 9 and Vertical Slice Architecture.

## Stack

- **.NET 9** WebAPI (Minimal APIs)
- **PostgreSQL 17** + PostGIS 3.5
- **Entity Framework Core 9** (Code First + NetTopologySuite)
- **MediatR** (CQRS pipeline)
- **FluentValidation** (auto-wired pipeline behavior)
- **JWT Authentication** + Refresh Tokens + Magic Links
- **Swagger/OpenAPI** documentation

## Architecture

```
Lootopia.Api/
├── Domain/           # Entities, Enums, Value Objects
├── Features/         # Vertical Slices (Screaming Architecture)
│   ├── Auth/         # Login, Register, Guest, MagicLink, JWT
│   ├── Hunts/        # Hunt CRUD, Start, ValidateStep, Complete
│   ├── Wallet/       # Balance, Credit, Debit, Transactions
│   ├── Inventory/    # Player items management
│   ├── Campaigns/    # Partner campaigns B2B
│   ├── Partners/     # Activity reports
│   ├── Admin/        # Fraud alerts, Freeze, Credit
│   ├── Marketplace/  # Sales listings, purchases
│   ├── Trading/      # P2P item trading (escrow)
│   ├── Auctions/     # Real-time bidding system
│   ├── Commissions/  # Platform fees and payouts
│   ├── Leaderboards/ # Rankings with multi-criteria
│   ├── Achievements/ # Badges and rule engine
│   └── Notifications/# In-app notifications (GDPR)
├── Infrastructure/   # EF Core, Services, Middleware
└── SharedKernel/     # Result<T>, IGeoValidator, Behaviors
```

## Quick Start

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

## Seed Users

| Role    | Email               | Password     |
|---------|---------------------|--------------|
| Admin   | admin@lootopia.io   | Admin123!    |
| Partner | partner@lootopia.io | Partner123!  |
| Player  | player@lootopia.io  | Player123!   |

## API Endpoints

### Auth
- `POST /api/auth/register` - Create account
- `POST /api/auth/login` - Login (email + password)
- `POST /api/auth/guest` - Guest login (deviceId)
- `POST /api/auth/upgrade` - Upgrade guest to player
- `POST /api/auth/refresh` - Refresh JWT token
- `POST /api/auth/magic-link/generate` - Generate magic link (admin)
- `POST /api/auth/magic-link/validate` - Consume magic link

### Hunts
- `GET /api/hunts?lat=&lng=&radius=` - List nearby hunts
- `GET /api/hunts/{id}` - Get hunt details
- `POST /api/hunts` - Create hunt (admin)
- `POST /api/hunts/{id}/activate` - Activate hunt (admin)
- `POST /api/hunts/{id}/start` - Start a hunt
- `POST /api/hunts/{id}/steps/{order}/validate` - Validate step (GPS)

### Wallet
- `GET /api/wallet` - Get balance
- `GET /api/wallet/transactions` - Transaction history
- `POST /api/wallet/credit` - Credit wallet (admin)

### Inventory
- `GET /api/inventory` - Get player items

### Marketplace
- `GET /api/marketplace/listings` - Browse catalog
- `POST /api/marketplace/listings` - Create listing
- `POST /api/marketplace/listings/{id}/purchase` - Buy item
- `POST /api/marketplace/listings/{id}/cancel` - Cancel listing

### Trading
- `POST /api/trading/offers` - Create trade offer
- `POST /api/trading/offers/{id}/respond` - Accept/Refuse
- `GET /api/trading/offers` - My trades

### Auctions
- `GET /api/auctions` - List auctions
- `POST /api/auctions` - Create auction
- `POST /api/auctions/{id}/bid` - Place bid
- `POST /api/auctions/{id}/close` - Close auction

### Leaderboards
- `GET /api/leaderboards` - Get rankings
- `GET /api/leaderboards/me` - My rank

### Achievements
- `GET /api/achievements` - My achievements

### Notifications
- `GET /api/notifications` - My notifications
- `POST /api/notifications/{id}/read` - Mark as read
- `GET /api/notifications/preferences` - Get preferences
- `PUT /api/notifications/preferences` - Update preferences

### Admin
- `GET /api/admin/fraud-alerts` - View fraud alerts
- `POST /api/admin/users/{id}/freeze` - Freeze user
- `POST /api/admin/users/{id}/unfreeze` - Unfreeze user
- `POST /api/admin/campaigns/{id}/freeze` - Freeze campaign
- `POST /api/admin/partners/{id}/credit` - Credit partner budget
