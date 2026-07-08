# Etapa 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos solo los archivos de proyecto primero (mejora el cache de Docker)
COPY ["Chronos/Chronos.Web/Chronos.Web.csproj", "Chronos/Chronos.Web/"]
COPY ["Chronos/Chronos.Infrastructure/Chronos.Infrastructure.csproj", "Chronos/Chronos.Infrastructure/"]
COPY ["Chronos/Chronos.Domain/Chronos.Domain.csproj", "Chronos/Chronos.Domain/"]

RUN dotnet restore "Chronos/Chronos.Web/Chronos.Web.csproj"

# Copiamos el resto del código fuente
COPY . .

WORKDIR "/src/Chronos/Chronos.Web"
RUN dotnet publish "Chronos.Web.csproj" -c Release -o /app/publish

# Etapa 2: Imagen final de ejecución (más liviana)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway asigna el puerto dinámicamente vía variable de entorno PORT
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Chronos.Web.dll"]
