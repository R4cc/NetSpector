FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NetworkDeviceMonitor.UI/NetworkDeviceMonitor.UI.csproj", "NetworkDeviceMonitor.UI/"]
RUN dotnet restore "NetworkDeviceMonitor.UI/NetworkDeviceMonitor.UI.csproj"
COPY . .
WORKDIR "/src/NetworkDeviceMonitor.UI"
RUN dotnet build "NetworkDeviceMonitor.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetworkDeviceMonitor.UI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetworkDeviceMonitor.UI.dll"]
