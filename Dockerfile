# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["SupabaseNET.csproj", "./"]
RUN dotnet restore "SupabaseNET.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src"
RUN dotnet build "SupabaseNET.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "SupabaseNET.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Crear usuario no root para seguridad
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copiar archivos publicados
COPY --from=publish /app/publish .

# Cambiar propiedad de los archivos
RUN chown -R appuser:appuser /app

# Cambiar a usuario no root
USER appuser

# Exponer el puerto (puede ser sobrescrito por el PaaS)
EXPOSE 8080

# Variables de entorno por defecto
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Punto de entrada
ENTRYPOINT ["dotnet", "SupabaseNET.dll"]
