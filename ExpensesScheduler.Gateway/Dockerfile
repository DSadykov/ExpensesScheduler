# 
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8082
ENV ASPNETCORE_URLS=http://+:8082

# Переключаемся на root для установки curl
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
# Возвращаемся к безопасному пользователю
USER $APP_UID

# Stage build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ExpensesScheduler.Gateway/ExpensesScheduler.Gateway.csproj", "ExpensesScheduler.Gateway/"]
RUN dotnet restore "./ExpensesScheduler.Gateway/ExpensesScheduler.Gateway.csproj"
COPY . .
WORKDIR "/src/ExpensesScheduler.Gateway"
RUN dotnet build "./ExpensesScheduler.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ExpensesScheduler.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# 
ENTRYPOINT ["dotnet", "ExpensesScheduler.Gateway.dll"]