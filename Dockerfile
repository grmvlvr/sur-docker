# Клиент
FROM node:20-alpine AS web
WORKDIR /src/web
COPY web/package.json web/package-lock.json ./
RUN npm ci
COPY web/ ./
RUN npx ng build --configuration production

# API + статика
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY server/Sur.Api/Sur.Api.csproj server/Sur.Api/
RUN dotnet restore server/Sur.Api/Sur.Api.csproj
COPY server/Sur.Api/ server/Sur.Api/
COPY --from=web /src/web/dist/web/browser/ server/Sur.Api/wwwroot/
RUN dotnet publish server/Sur.Api/Sur.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/publish .
VOLUME ["/app/App_Data"]
ENTRYPOINT ["dotnet", "Sur.Api.dll"]
