# �������� �������
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# ���� ��� ����� �������
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID

# �������� ������
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. �������� csproj ��������� ������� � ���� ��������� ���������
COPY ["ExpensesScheduler.SmtpNotificationService/ExpensesScheduler.SmtpNotificationService.csproj", "ExpensesScheduler.SmtpNotificationService/"]
COPY ["ExpensesScheduler.Authorization/ExpensesScheduler.Authorization.csproj", "ExpensesScheduler.Authorization/"]
COPY ["ExpensesScheduler.Messaging/ExpensesScheduler.Messaging.csproj", "ExpensesScheduler.Messaging/"]

# 2. ��������������� �����������
RUN dotnet restore "ExpensesScheduler.SmtpNotificationService/ExpensesScheduler.SmtpNotificationService.csproj"

# 3. �������� ���� �������� ��� �� ����� �������
COPY . .

# 4. ������ �������
WORKDIR "/src/ExpensesScheduler.SmtpNotificationService"
RUN dotnet build "ExpensesScheduler.SmtpNotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 5. ����������
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ExpensesScheduler.SmtpNotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 6. ��������� �����
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExpensesScheduler.SmtpNotificationService.dll"]