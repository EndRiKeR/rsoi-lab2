FROM  mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY . ./TicketsService
WORKDIR /app/TicketsService
RUN dotnet publish --configuration Release --runtime linux-x64 --self-contained true --output /app
WORKDIR /app
ENTRYPOINT ["dotnet", "TicketsService.dll"]