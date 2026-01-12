# Сценарий запуска
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Порт для этого сервиса
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID

# Сценарий сборки
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Копируем csproj основного сервиса и всех зависимых библиотек
COPY ["ExpensesScheduler.SmtpNotificationService/ExpensesScheduler.SmtpNotificationService.csproj", "ExpensesScheduler.NotificationService/"]
COPY ["ExpensesScheduler.Authorization/ExpensesScheduler.Authorization.csproj", "ExpensesScheduler.Authorization/"]
COPY ["ExpensesScheduler.Messaging/ExpensesScheduler.Messaging.csproj", "ExpensesScheduler.Messaging/"]

# 2. Восстанавливаем зависимости
RUN dotnet restore "ExpensesScheduler.SmtpNotificationService/ExpensesScheduler.SmtpNotificationService.csproj"

# 3. Копируем весь исходный код из корня решения
COPY . .

# 4. Сборка проекта
WORKDIR "/src/ExpensesScheduler.SmtpNotificationService"
RUN dotnet build "ExpensesScheduler.SmtpNotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 5. Публикация
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ExpensesScheduler.SmtpNotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 6. Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExpensesScheduler.SmtpNotificationService.dll"]