FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Attendance/HRManagement.Attendance.Api/HRManagement.Attendance.Api.csproj", "src/Services/Attendance/HRManagement.Attendance.Api/"]
COPY ["src/Shared/HRManagement.Shared.Common/HRManagement.Shared.Common/HRManagement.Shared.Common.csproj", "src/Shared/HRManagement.Shared.Common/HRManagement.Shared.Common/"]
COPY ["src/Shared/HRManagement.Shared.Contracts/HRManagement.Shared.Contracts/HRManagement.Shared.Contracts.csproj", "src/Shared/HRManagement.Shared.Contracts/HRManagement.Shared.Contracts/"]
COPY ["src/Shared/HRManagement.Shared.MessageBus/HRManagement.Shared.MessageBus/HRManagement.Shared.MessageBus.csproj", "src/Shared/HRManagement.Shared.MessageBus/HRManagement.Shared.MessageBus/"]
RUN dotnet restore "src/Services/Attendance/HRManagement.Attendance.Api/HRManagement.Attendance.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Attendance/HRManagement.Attendance.Api"
RUN dotnet build "HRManagement.Attendance.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HRManagement.Attendance.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HRManagement.Attendance.Api.dll"]
