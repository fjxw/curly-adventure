FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["src/Shared/HRManagement.Shared.Common/HRManagement.Shared.Common.csproj", "src/Shared/HRManagement.Shared.Common/"]
COPY ["src/Shared/HRManagement.Shared.Contracts/HRManagement.Shared.Contracts.csproj", "src/Shared/HRManagement.Shared.Contracts/"]
COPY ["src/Shared/HRManagement.Shared.MessageBus/HRManagement.Shared.MessageBus.csproj", "src/Shared/HRManagement.Shared.MessageBus/"]

# Copy service project
COPY ["src/Services/Employees/HRManagement.Employees.Api/HRManagement.Employees.Api.csproj", "src/Services/Employees/HRManagement.Employees.Api/"]

# Restore
RUN dotnet restore "src/Services/Employees/HRManagement.Employees.Api/HRManagement.Employees.Api.csproj"

# Copy everything
COPY . .

# Build
WORKDIR "/src/src/Services/Employees/HRManagement.Employees.Api"
RUN dotnet build "HRManagement.Employees.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HRManagement.Employees.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HRManagement.Employees.Api.dll"]
