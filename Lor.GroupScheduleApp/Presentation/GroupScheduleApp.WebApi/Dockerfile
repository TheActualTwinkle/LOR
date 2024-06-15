# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 31401

# Build image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["Lor.GroupScheduleApp/Presentation/GroupScheduleApp.WebApi/GroupScheduleApp.WebApi.csproj", "Lor.GroupScheduleApp/Presentation/GroupScheduleApp.WebApi/"]
COPY ["Lor.GroupScheduleApp/Core/GroupScheduleApp.Grpc/GroupScheduleApp.Grpc.csproj", "Lor.GroupScheduleApp/Core/GroupScheduleApp.Grpc/"]
RUN dotnet restore "Lor.GroupScheduleApp/Presentation/GroupScheduleApp.WebApi/GroupScheduleApp.WebApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Lor.GroupScheduleApp/Presentation/GroupScheduleApp.WebApi"
RUN dotnet build "GroupScheduleApp.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish application
FROM build AS publish
RUN dotnet publish "GroupScheduleApp.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GroupScheduleApp.WebApi.dll"]
