FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Gateway/HRManagement.Gateway/HRManagement.Gateway/HRManagement.Gateway.csproj", "src/Services/Gateway/HRManagement.Gateway/HRManagement.Gateway/"]
RUN dotnet restore "src/Services/Gateway/HRManagement.Gateway/HRManagement.Gateway/HRManagement.Gateway.csproj"
COPY . .
WORKDIR "/src/src/Services/Gateway/HRManagement.Gateway/HRManagement.Gateway"
RUN dotnet build "HRManagement.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HRManagement.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HRManagement.Gateway.dll"]
