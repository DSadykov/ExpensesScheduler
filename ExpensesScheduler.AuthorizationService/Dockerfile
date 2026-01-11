# Базовый образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Порт для этого сервиса
EXPOSE 7069
ENV ASPNETCORE_URLS=http://+:7069

USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID

# Образ для сборки
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Копируем файлы проектов (.csproj) всех зависимостей. 
# Это необходимо для корректного выполнения dotnet restore.
COPY ["ExpensesScheduler.AuthorizationService/ExpensesScheduler.AuthorizationService.csproj", "ExpensesScheduler.AuthorizationService/"]
COPY ["ExpensesScheduler.Authorization/ExpensesScheduler.Authorization.csproj", "ExpensesScheduler.Authorization/"]
COPY ["ExpensesScheduler.Messaging/ExpensesScheduler.Messaging.csproj", "ExpensesScheduler.Messaging/"]

# 2. Восстанавливаем зависимости для основного проекта
RUN dotnet restore "ExpensesScheduler.AuthorizationService/ExpensesScheduler.AuthorizationService.csproj"

# 3. Копируем всё содержимое папок (исходный код)
COPY . .

# 4. Переходим в папку сервиса и собираем его
WORKDIR "/src/ExpensesScheduler.AuthorizationService"
RUN dotnet build "ExpensesScheduler.AuthorizationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 5. Публикация
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ExpensesScheduler.AuthorizationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 6. Финальный этап
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExpensesScheduler.AuthorizationService.dll"]