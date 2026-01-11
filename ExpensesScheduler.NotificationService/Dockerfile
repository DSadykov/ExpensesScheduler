# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Порт для этого сервиса
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8081

USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ExpensesScheduler.NotificationService/ExpensesScheduler.NotificationService.csproj", "ExpensesScheduler.NotificationService/"]
RUN dotnet restore "./ExpensesScheduler.NotificationService/ExpensesScheduler.NotificationService.csproj"
COPY . .
WORKDIR "/src/ExpensesScheduler.NotificationService"
RUN dotnet build "./ExpensesScheduler.NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ExpensesScheduler.NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExpensesScheduler.NotificationService.dll"]