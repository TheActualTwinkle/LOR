﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 31400
EXPOSE 31401

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Lor.DatabaseApp/Presentation/DatabaseApp.WebApi/DatabaseApp.WebApi.csproj", "Lor.DatabaseApp/Presentation/DatabaseApp.WebApi/"]
COPY ["Lor.DatabaseApp/Infrastructure/DatabaseApp.Caching/DatabaseApp.Caching.csproj", "Lor.DatabaseApp/Infrastructure/DatabaseApp.Caching/"]
COPY ["Lor.DatabaseApp/Core/DatabaseApp.Application/DatabaseApp.Application.csproj", "Lor.DatabaseApp/Core/DatabaseApp.Application/"]
COPY ["Lor.DatabaseApp/Core/DatabaseApp.Domain/DatabaseApp.Domain.csproj", "Lor.DatabaseApp/Core/DatabaseApp.Domain/"]
COPY ["Lor.DatabaseApp/Infrastructure/DatabaseApp.Persistence/DatabaseApp.Persistence.csproj", "Lor.DatabaseApp/Infrastructure/DatabaseApp.Persistence/"]
COPY ["Lor.DatabaseApp/Infrastructure/DatabaseApp.Messaging/DatabaseApp.Messaging.csproj", "Lor.DatabaseApp/Infrastructure/DatabaseApp.Messaging/"]
RUN dotnet restore "Lor.DatabaseApp/Presentation/DatabaseApp.WebApi/DatabaseApp.WebApi.csproj"
COPY . .
WORKDIR "/src/Lor.DatabaseApp/Presentation/DatabaseApp.WebApi"
RUN dotnet build "./DatabaseApp.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DatabaseApp.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DatabaseApp.WebApi.dll"]