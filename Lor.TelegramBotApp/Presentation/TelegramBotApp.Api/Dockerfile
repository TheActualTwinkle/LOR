FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Lor.TelegramBotApp/Presentation/TelegramBotApp.Api/TelegramBotApp.Api.csproj", "Lor.TelegramBotApp/Presentation/TelegramBotApp.Api/"]
COPY ["Lor.TelegramBotApp/Core/TelegramBotApp.Application/TelegramBotApp.Application.csproj", "Lor.TelegramBotApp/Core/TelegramBotApp.Application/"]
COPY ["Lor.TelegramBotApp/Infrastructure/TelegramBotApp.AppCommunication/TelegramBotApp.AppCommunication.csproj", "Lor.TelegramBotApp/Infrastructure/TelegramBotApp.AppCommunication/"]
COPY ["Lor.TelegramBotApp/Infrastructure/TelegramBotApp.Identity/TelegramBotApp.Identity.csproj", "Lor.TelegramBotApp/Infrastructure/TelegramBotApp.Identity/"]
RUN dotnet restore "Lor.TelegramBotApp/Presentation/TelegramBotApp.Api/TelegramBotApp.Api.csproj"
COPY . .
WORKDIR "/src/Lor.TelegramBotApp/Presentation/TelegramBotApp.Api"
RUN dotnet build "TelegramBotApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TelegramBotApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramBotApp.Api.dll"]
