# Lootopia Docker Setup

This project uses Docker Compose to run the complete application stack.

## Services

### Database (PostgreSQL with PostGIS)
- **Container**: `lootopia-db`
- **Port**: `5432`
- **Image**: `postgis/postgis:17-3.5`
- **Credentials**: 
  - Username: `lootopia`
  - Password: `lootopia_dev_2026`
  - Database: `lootopia`

### API (.NET 9)
- **Container**: `lootopia-api`
- **Port**: `8080`
- **Health Check**: `http://localhost:8080/health`
- **Swagger**: `http://localhost:8080/swagger` (Development mode)
- **Base URL**: `http://localhost:8080/api`

### Frontend (React + Vite + Nginx)
- **Container**: `lootopia-client`
- **Port**: `3000`
- **URL**: `http://localhost:3000`
- **API Proxy**: All `/api/*` requests are proxied to the backend API

## Running the Application

### Start all services
```bash
docker-compose up -d --build
```

### Stop all services
```bash
docker-compose down
```

### View logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f client
docker-compose logs -f db
```

### Check service status
```bash
docker-compose ps
```

## Access Points

- **Frontend**: http://localhost:3000
- **API**: http://localhost:8080/api
- **API Health**: http://localhost:8080/health
- **Database**: localhost:5432

## Development

### Frontend Development (without Docker)
If you want to run the frontend locally for development:

```bash
cd Lootopia.Client
npm install
npm run dev
```

The Vite dev server will proxy `/api` requests to `http://localhost:8080` by default.

### API Development (without Docker)
If you want to run the API locally:

```bash
cd Lootopia.Api
dotnet run
```

Make sure you have PostgreSQL running locally or update the connection string.

## Web Deployment

The frontend is built for web deployment:
- Static files are served by Nginx
- API requests are proxied through Nginx to avoid CORS issues
- Service Worker and PWA features are enabled
- Static assets are cached for optimal performance

## Architecture

```
┌─────────────────┐
│   Frontend      │  Port 3000
│   (Nginx)       │  Serves static files + proxies /api
└────────┬────────┘
         │
         │ /api/* requests
         ▼
┌─────────────────┐
│   API           │  Port 8080
│   (.NET 9)      │  REST API endpoints
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Database      │  Port 5432
│   (PostgreSQL)  │  Data storage
└─────────────────┘
```
