# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["NxB.BookingApi.csproj", "./"]
RUN dotnet restore "NxB.BookingApi.csproj"

# Copy source code and build
COPY . .
RUN dotnet build "NxB.BookingApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "NxB.BookingApi.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install necessary dependencies for SQL Server connection
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "NxB.BookingApi.dll"]