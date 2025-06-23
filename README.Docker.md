# Love Letter Game - Full Stack Docker Setup

This repository contains a complete Docker setup for the Love Letter card game, including the backend API (.NET), frontend (SvelteKit), and PostgreSQL database.

## 🚀 Quick Start

### Prerequisites
- Docker and Docker Compose installed
- Ports 3000, 5433, and 8080 available on your host

### Run the Full Stack
```bash
# Clone or navigate to the project root
cd /home/cardoso42/dev/love-letter

# Start all services
docker-compose up --build

# Or run in detached mode
docker-compose up --build -d
```

### Access the Application
- **🎮 Game Frontend**: http://localhost:3000
- **🔧 API Endpoints**: http://localhost:8080/api
- **❤️ Health Check**: http://localhost:8080/health
- **🗄️ Database**: localhost:5433 (PostgreSQL)

## 📋 Services Overview

### 🗄️ PostgreSQL Database (`postgres`)
- **Image**: postgres:15-alpine
- **Port**: 5433 (external) → 5432 (internal)
- **Database**: love_letter
- **User**: princess
- **Password**: 9

### 🔧 Backend API (`api`)
- **Framework**: .NET 8.0 with ASP.NET Core
- **Port**: 8080
- **Features**: 
  - JWT Authentication
  - SignalR for real-time communication
  - Entity Framework with automatic migrations
  - Swagger documentation (in development)
  - Health checks

### 🎮 Frontend (`frontend`)
- **Framework**: SvelteKit with TypeScript
- **Port**: 3000
- **Server**: Nginx with SPA routing
- **Features**:
  - API proxy to backend
  - SignalR WebSocket proxy
  - Static asset optimization
  - Gzip compression

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   Backend API   │    │   PostgreSQL    │
│   (SvelteKit)   │◄──►│   (.NET Core)   │◄──►│   Database      │
│   Port: 3000    │    │   Port: 8080    │    │   Port: 5433    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🔧 Development Workflow

### Full Stack Development
```bash
# Start all services
docker-compose up --build

# View logs for all services
docker-compose logs -f

# View logs for specific service
docker-compose logs -f api
docker-compose logs -f frontend
docker-compose logs -f postgres
```

### Individual Service Development
```bash
# Start only database and API
docker-compose up postgres api

# Start only database
docker-compose up postgres
```

### Rebuild After Changes
```bash
# Rebuild specific service
docker-compose up --build api
docker-compose up --build frontend

# Rebuild everything
docker-compose down
docker-compose up --build
```

## 🛠️ Configuration

### Environment Variables
The setup uses the following default configuration:

#### Backend API
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=Host=postgres;Database=love_letter;Username=princess;Password=9`

#### Database
- `POSTGRES_DB=love_letter`
- `POSTGRES_USER=princess`
- `POSTGRES_PASSWORD=9`

### Customization
To customize the setup:

1. **Change ports**: Edit the `ports` section in `docker-compose.yml`
2. **Environment variables**: Add to the `environment` section
3. **Database credentials**: Update both postgres and api services
4. **Volumes**: Modify the `volumes` section for persistent data

## 📊 Health Checks

All services include health checks:

### Check Service Health
```bash
# Check all services
docker-compose ps

# Manual health checks
curl http://localhost:8080/health  # API
curl http://localhost:3000         # Frontend
docker-compose exec postgres pg_isready -U princess -d love_letter  # Database
```

## 🗂️ Data Persistence

### Database Data
- PostgreSQL data is persisted in the `postgres_data` Docker volume
- Data survives container restarts and rebuilds

### Application Logs
- Backend logs are written to `./src/CartasDeAmorBack/logs/`
- Logs are accessible from the host system

### Reset Database
```bash
# Stop services and remove volumes
docker-compose down -v

# Restart (this will recreate the database)
docker-compose up --build
```

## 🔍 Troubleshooting

### Port Conflicts
```bash
# Check what's using ports
sudo netstat -tulpn | grep :3000
sudo netstat -tulpn | grep :8080
sudo netstat -tulpn | grep :5433

# Stop conflicting services or change ports in docker-compose.yml
```

### Service Dependencies
Services start in order:
1. 🗄️ PostgreSQL (postgres)
2. 🔧 Backend API (api) - waits for database health check
3. 🎮 Frontend (frontend) - waits for API health check

### Common Issues

#### Database Connection
```bash
# Check if database is accessible
docker-compose exec postgres psql -U princess -d love_letter -c "SELECT version();"
```

#### API Issues
```bash
# Check API logs for Entity Framework migrations
docker-compose logs api | grep -i migration

# Check if API is responding
curl http://localhost:8080/health
```

#### Frontend Issues
```bash
# Check nginx configuration
docker-compose exec frontend nginx -t

# Check if static files are served
curl -I http://localhost:3000
```

### Reset Everything
```bash
# Nuclear option: remove everything and start fresh
docker-compose down -v
docker system prune -f
docker-compose up --build
```

## 🚦 Testing the Setup

### 1. Health Checks
```bash
curl http://localhost:8080/health
curl http://localhost:3000
```

### 2. Create Account
```bash
curl -X POST http://localhost:8080/api/account/create \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "testpassword123"
  }'
```

### 3. Login
```bash
curl -X POST http://localhost:8080/api/account/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "testpassword123"
  }'
```

### 4. Test Frontend
Open http://localhost:3000 in your browser and test the game interface.

## 📝 Development Notes

### Hot Reload
- Backend: Code changes require container rebuild
- Frontend: Code changes require container rebuild (not hot reload in production build)

### Database Migrations
- Migrations are automatically applied on API startup
- Database schema is created automatically

### CORS
- Configured to allow frontend domain
- SignalR configured for WebSocket connections

## 🔒 Security Notes

- Default database password is weak (change for production)
- JWT secret should be randomized for production
- Containers run as non-root users where possible
- Security headers included in nginx configuration

## 📚 Additional Resources

- Backend API Documentation: `./src/CartasDeAmorBack/README.Docker.md`
- Frontend Documentation: `./src/CartasDeAmorFront/README.Docker.md`
- API Documentation: `./docs/api-documentation.md`
- Game Rules: `./docs/cards.md`

## 🏷️ Version Information

- .NET: 8.0
- PostgreSQL: 15
- Node.js: 20
- Svelte: 5.0
- SvelteKit: 2.16
