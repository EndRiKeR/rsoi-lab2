FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8070

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["services/TicketsService/TicketsService.csproj", "TicketsService/"]
COPY ["services/Common/Common.csproj", "Common/"]
RUN dotnet restore "TicketsService/TicketsService.csproj"

COPY services/ ./

WORKDIR "/src/TicketsService"
RUN dotnet build "TicketsService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TicketsService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TicketsService.dll"]