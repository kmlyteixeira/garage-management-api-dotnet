FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["common.props", "./"]
COPY ["NuGet.Config", "./"]
COPY ["src/GarageManagement.Application/GarageManagement.Application.csproj", "src/GarageManagement.Application/"]
COPY ["src/GarageManagement.Application.Contracts/GarageManagement.Application.Contracts.csproj", "src/GarageManagement.Application.Contracts/"]
COPY ["src/GarageManagement.Domain/GarageManagement.Domain.csproj", "src/GarageManagement.Domain/"]
COPY ["src/GarageManagement.Domain.Shared/GarageManagement.Domain.Shared.csproj", "src/GarageManagement.Domain.Shared/"]
COPY ["src/GarageManagement.EntityFrameworkCore/GarageManagement.EntityFrameworkCore.csproj", "src/GarageManagement.EntityFrameworkCore/"]
COPY ["src/GarageManagement.DbMigrator/GarageManagement.DbMigrator.csproj", "src/GarageManagement.DbMigrator/"]

RUN dotnet restore "src/GarageManagement.DbMigrator/GarageManagement.DbMigrator.csproj"

COPY . .
WORKDIR /src/src/GarageManagement.DbMigrator
RUN dotnet publish "GarageManagement.DbMigrator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GarageManagement.DbMigrator.dll"]
