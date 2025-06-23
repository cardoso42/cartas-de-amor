# Multi-stage Dockerfile for CartasDeAmor application

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy global.json to ensure SDK version compatibility
COPY global.json ./

# Copy solution file
COPY CartasDeAmor.sln ./

# Copy project files for restore (improves Docker layer caching)
COPY CartasDeAmor.Application/CartasDeAmor.Application.csproj ./CartasDeAmor.Application/
COPY CartasDeAmor.Domain/CartasDeAmor.Domain.csproj ./CartasDeAmor.Domain/
COPY CartasDeAmor.Infrastructure/CartasDeAmor.Infrastructure.csproj ./CartasDeAmor.Infrastructure/
COPY CartasDeAmor.Presentation/CartasDeAmor.Presentation.csproj ./CartasDeAmor.Presentation/

# Restore NuGet packages
RUN dotnet restore CartasDeAmor.Presentation/CartasDeAmor.Presentation.csproj

# Copy all source files
COPY . ./

# Build the application
RUN dotnet build CartasDeAmor.Presentation/CartasDeAmor.Presentation.csproj -c Release --no-restore

# Publish the application
RUN dotnet publish CartasDeAmor.Presentation/CartasDeAmor.Presentation.csproj -c Release --no-build -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy the published application
COPY --from=build /app/publish .

# Create logs directory and set permissions
RUN mkdir -p /app/Logs && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose the port the app runs on
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set the entry point
ENTRYPOINT ["dotnet", "CartasDeAmor.Presentation.dll"]
