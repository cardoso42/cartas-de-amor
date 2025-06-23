# Cartas de Amor Frontend - Docker Setup

This directory contains the Docker configuration for the Cartas de Amor (Love Letter) game frontend built with SvelteKit.

## Prerequisites

- Docker and Docker Compose installed on your system
- Port 3000 available on your host machine (or modify the port mapping)

## Quick Start

### Option 1: Run Frontend Only

```bash
# Navigate to the frontend directory
cd /home/cardoso42/dev/love-letter/src/CartasDeAmorFront

# Build and run the frontend
docker-compose up --build

# Or run in detached mode
docker-compose up --build -d
```

This will start only the frontend on `http://localhost:3000`

### Option 2: Run Full Stack (Recommended)

```bash
# Navigate to the backend directory
cd /home/cardoso42/dev/love-letter/src/CartasDeAmorBack

# Build and run all services (database, API, and frontend)
docker-compose up --build

# Or run in detached mode
docker-compose up --build -d
```

This will start:
- PostgreSQL database (port 5433)
- .NET API (port 8080)
- Svelte frontend (port 3000)

## Available URLs

Once running, the application will be available at:

- **Frontend**: `http://localhost:3000`
- **API (via proxy)**: `http://localhost:3000/api` (proxied to backend)
- **SignalR Hub (via proxy)**: `http://localhost:3000/gameHub` (proxied to backend)
- **Direct API**: `http://localhost:8080/api` (if running full stack)

## Docker Configuration

### Build Process

The Dockerfile uses a multi-stage build:

1. **Build Stage**: 
   - Uses Node.js 20 Alpine
   - Installs pnpm and dependencies
   - Builds the SvelteKit application using static adapter
   
2. **Production Stage**:
   - Uses Nginx Alpine for serving
   - Configures SPA routing with fallback to index.html
   - Includes proxy configuration for API and SignalR

### SvelteKit Configuration

The frontend is configured to use:
- `@sveltejs/adapter-static` for containerized deployment
- Static file generation with SPA fallback
- Output to `build/` directory

### Nginx Configuration

The production container includes:
- SPA routing support (all routes fallback to index.html)
- API proxy to backend (`/api/*` → `http://backend:8080/api/*`)
- SignalR WebSocket proxy (`/gameHub` → `http://backend:8080/gameHub`)
- Static asset caching (1 year for immutable assets)
- Gzip compression
- Security headers

## Development vs Production

### Development
- Use `pnpm run dev` for local development
- Hot reload and development features enabled
- Direct API calls to backend

### Production (Docker)
- Optimized static build
- Nginx serving with compression
- Proxy configuration for API calls
- No development dependencies

## Environment Configuration

### Default Configuration
- Frontend runs on port 80 inside container (mapped to 3000 externally)
- API proxy configured for `host.docker.internal:8080`
- Health checks enabled

### Customization
You can modify the configuration by:
1. Updating `docker-compose.yml` for port mappings
2. Editing the Dockerfile for nginx configuration
3. Modifying `svelte.config.js` for build settings

## Troubleshooting

### Build Issues
```bash
# Check if pnpm install completed successfully
docker-compose logs frontend

# Rebuild without cache
docker-compose build --no-cache frontend
```

### Runtime Issues
```bash
# Check nginx logs
docker-compose logs frontend

# Check if frontend is responding
curl http://localhost:3000
```

### API Connectivity Issues
```bash
# Test direct API connection
curl http://localhost:8080/health

# Test proxied API connection
curl http://localhost:3000/api/health
```

### Network Issues
```bash
# Check if containers are on the same network
docker network ls
docker network inspect love_letter_network
```

## Useful Commands

```bash
# View frontend logs
docker-compose logs -f frontend

# Rebuild only frontend
docker-compose up --build frontend

# Stop services
docker-compose stop

# Remove containers and volumes
docker-compose down -v

# Execute commands in frontend container
docker-compose exec frontend sh

# Check nginx configuration
docker-compose exec frontend nginx -t
```

## Security Notes

- Static files are served with appropriate caching headers
- Security headers are included (X-Frame-Options, X-Content-Type-Options, etc.)
- Gzip compression is enabled for better performance
- The container runs nginx as non-root user

## Integration with Backend

When running the full stack:
1. Frontend automatically proxies API calls to the backend
2. SignalR connections are proxied with proper WebSocket support
3. CORS is handled by the backend configuration
4. Authentication tokens are passed through the proxy

## Performance Optimization

The Docker setup includes:
- Multi-stage build to minimize image size
- Static asset caching with long expiration
- Gzip compression for text assets
- Optimized nginx configuration for SPA serving

## Custom Configuration

To customize the setup:

1. **Change ports**: Edit `docker-compose.yml`
2. **Modify proxy targets**: Edit the nginx configuration in `Dockerfile`
3. **Add environment variables**: Add to the frontend service in `docker-compose.yml`
4. **Custom build settings**: Modify `svelte.config.js`
