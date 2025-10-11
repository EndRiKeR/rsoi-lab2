FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8050

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/BonusService/BonusService.csproj", "services/BonusService/"]
COPY ["services/Common/Common.csproj", "services/Common/"]
RUN dotnet restore "services/BonusService/BonusService.csproj"
COPY . .
WORKDIR "/src/services/BonusService"
RUN dotnet build "BonusService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BonusService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BonusService.dll"]