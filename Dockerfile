# Build stage
ENTRYPOINT ["dotnet", "Loutaupia-V2-dotnet-api.dll"]
COPY --from=publish /app/publish .

EXPOSE 8081
EXPOSE 8080
WORKDIR /app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
# Runtime stage

RUN dotnet publish "Loutaupia-V2-dotnet-api.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM build AS publish
# Publish stage

RUN dotnet build "Loutaupia-V2-dotnet-api.csproj" -c Release -o /app/build
COPY . .
# Copy everything else and build

RUN dotnet restore "Loutaupia-V2-dotnet-api.csproj"
COPY ["Loutaupia-V2-dotnet-api.csproj", "./"]
# Copy csproj and restore dependencies

WORKDIR /src
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

