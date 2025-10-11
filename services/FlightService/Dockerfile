FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8060

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем файлы проектов с правильными путями
COPY ["services/FlightService/FlightService.csproj", "services/FlightService/"]
COPY ["services/Common/Common.csproj", "services/Common/"]

# Восстанавливаем зависимости для FlightService
RUN dotnet restore "services/FlightService/FlightService.csproj"

# Копируем весь исходный код
COPY . .

# Переходим в директорию проекта для сборки
WORKDIR "/src/services/FlightService"
RUN dotnet build "FlightService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FlightService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlightService.dll"]