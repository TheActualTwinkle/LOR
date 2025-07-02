# Use the ASP.NET Core runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Use the .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Lor.GroupScheduleApp/Presentation/GroupScheduleApp.Api/GroupScheduleApp.Api.csproj", "Lor.GroupScheduleApp/Presentation/GroupScheduleApp.Api/"]
COPY ["Lor.GroupScheduleApp/Core/GroupScheduleApp.ScheduleUpdating/GroupScheduleApp.ScheduleUpdating.csproj", "Lor.GroupScheduleApp/Core/GroupScheduleApp.ScheduleUpdating/"]
COPY ["Lor.GroupScheduleApp/Core/GroupScheduleApp.Shared/GroupScheduleApp.Shared.csproj", "Lor.GroupScheduleApp/Core/GroupScheduleApp.Shared/"]
COPY ["Lor.GroupScheduleApp/Infrastructure/GroupScheduleApp.ScheduleProviding/GroupScheduleApp.ScheduleProviding.csproj", "Lor.GroupScheduleApp/Infrastructure/GroupScheduleApp.ScheduleProviding/"]
COPY ["Lor.GroupScheduleApp/Infrastructure/GroupScheduleApp.AppCommunication/GroupScheduleApp.AppCommunication.csproj", "Lor.GroupScheduleApp/Infrastructure/GroupScheduleApp.AppCommunication/"]
RUN dotnet restore "Lor.GroupScheduleApp/Presentation/GroupScheduleApp.Api/GroupScheduleApp.Api.csproj"
COPY . .
WORKDIR "/src/Lor.GroupScheduleApp/Presentation/GroupScheduleApp.Api"
RUN dotnet build "GroupScheduleApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GroupScheduleApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Use the ASP.NET Core runtime as the final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GroupScheduleApp.Api.dll"]
