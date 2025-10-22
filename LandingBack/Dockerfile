# ================================
# Build Stage
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto y restaurar dependencias
COPY LandingBack.csproj .
RUN dotnet restore

# Copiar el resto del código y compilar
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ================================
# Runtime Stage
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Instalar dependencias necesarias para PostgreSQL
RUN apt-get update && apt-get install -y \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Copiar los archivos publicados desde la etapa de build
COPY --from=build /app/publish .

# Crear directorio para logs (opcional)
RUN mkdir -p /app/logs && chmod 777 /app/logs

# Exponer el puerto (Render usa la variable de entorno PORT)
EXPOSE 8080

# Variables de entorno por defecto
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Ejecutar la aplicación
ENTRYPOINT ["dotnet", "LandingBack.dll"]
