# Cartas de Amor - Docker Setup

This directory contains the Docker configuration for the Cartas de Amor (Love Letter) game backend.

## Prerequisites

- Docker and Docker Compose installed on your system
- Port 5432 (PostgreSQL) and 8080 (API) available on your host machine

## Quick Start

### 1. Build and Run with Docker Compose (Recommended)

```bash
# Navigate to the backend directory
cd /home/cardoso42/dev/love-letter/src/CartasDeAmorBack

# Build and start all services
docker-compose up --build

# Or run in detached mode
docker-compose up --build -d
```

This will:
- Start a PostgreSQL database container
- Build and start the .NET API container
- Set up networking between the containers
- Expose the API on `http://localhost:8080`

### 2. Build and Run with Docker only

```bash
# Build the Docker image
docker build -t cartas-de-amor-api .

# Run PostgreSQL database
docker run -d \
  --name love_letter_db \
  -e POSTGRES_DB=love_letter \
  -e POSTGRES_USER=princess \
  -e POSTGRES_PASSWORD=9 \
  -p 5432:5432 \
  postgres:15-alpine

# Run the API (make sure to replace localhost with the container IP)
docker run -d \
  --name love_letter_api \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Database=love_letter;Username=princess;Password=9" \
  cartas-de-amor-api
```

## Available Endpoints

Once running, the API will be available at:

- **Health Check**: `http://localhost:8080/health`
- **Swagger UI**: `http://localhost:8080/swagger` (in Development environment)
- **API Base**: `http://localhost:8080/api`

## Environment Configuration

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Set to `Production` by default in Docker
- `ASPNETCORE_URLS`: Set to `http://+:8080`
- `ConnectionStrings__DefaultConnection`: Database connection string

### Configuration Files

- `appsettings.json`: Base configuration
- `appsettings.Production.json`: Production-specific overrides (used in Docker)

## Database

The setup uses PostgreSQL with the following default credentials:
- **Host**: postgres (in Docker network) / localhost (external)
- **Database**: love_letter
- **Username**: princess
- **Password**: 9
- **Port**: 5432

## Logs

Application logs are written to:
- Console output (visible with `docker-compose logs api`)
- File logs in `./logs` directory (mounted as volume)

## Development vs Production

### Development
- Uses `appsettings.Development.json`
- Swagger UI enabled
- Detailed logging
- CORS configured for localhost frontend

### Production (Docker)
- Uses `appsettings.Production.json`
- Console and file logging
- Swagger UI disabled
- Security-focused configuration

## Troubleshooting

### Port Conflicts
If you get port conflicts:
```bash
# Check what's using the ports
sudo netstat -tulpn | grep :8080
sudo netstat -tulpn | grep :5432

# Stop existing containers
docker-compose down
```

### Database Connection Issues
```bash
# Check if PostgreSQL container is running
docker-compose ps

# Check PostgreSQL logs
docker-compose logs postgres

# Connect to PostgreSQL directly
docker-compose exec postgres psql -U princess -d love_letter
```

### API Connection Issues
```bash
# Check API logs
docker-compose logs api

# Check health endpoint
curl http://localhost:8080/health
```

### Rebuild After Code Changes
```bash
# Rebuild only the API container
docker-compose up --build api

# Or rebuild everything
docker-compose down
docker-compose up --build
```

## Useful Commands

```bash
# View logs
docker-compose logs -f api
docker-compose logs -f postgres

# Stop services
docker-compose stop

# Remove containers and volumes
docker-compose down -v

# Execute commands in running containers
docker-compose exec api bash
docker-compose exec postgres psql -U princess -d love_letter

# Scale services (if needed)
docker-compose up --scale api=2
```

## Security Notes

- The default database password is weak and should be changed for production use
- JWT secret key should be generated randomly for production
- Consider using Docker secrets for sensitive information in production
- The application runs as a non-root user inside the container for security

## Frontend Integration

To connect a frontend application:

1. **Local Frontend**: Use `http://localhost:8080/api` as the API base URL
2. **Dockerized Frontend**: Use `http://api:8080/api` if running in the same Docker network
3. **SignalR Hub**: Connect to `http://localhost:8080/gameHub`

Make sure CORS is properly configured for your frontend domain.
