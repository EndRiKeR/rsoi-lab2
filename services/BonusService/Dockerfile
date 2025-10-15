FROM  mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY . ./BonusService
WORKDIR /app/BonusService
RUN dotnet publish --configuration Release --runtime linux-x64 --self-contained true --output /app
WORKDIR /app
ENTRYPOINT ["dotnet", "BonusService.dll"]