FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

RUN apt-get update \
	&& apt-get install -y --no-install-recommends nodejs npm \
	&& rm -rf /var/lib/apt/lists/*

RUN dotnet tool install -g Volo.Abp.Cli --version 8.3.0
ENV PATH="$PATH:/root/.dotnet/tools"

COPY ["common.props", "./"]
COPY ["NuGet.Config", "./"]
COPY ["src/GarageManagement.Application/GarageManagement.Application.csproj", "src/GarageManagement.Application/"]
COPY ["src/GarageManagement.Application.Contracts/GarageManagement.Application.Contracts.csproj", "src/GarageManagement.Application.Contracts/"]
COPY ["src/GarageManagement.Domain/GarageManagement.Domain.csproj", "src/GarageManagement.Domain/"]
COPY ["src/GarageManagement.Domain.Shared/GarageManagement.Domain.Shared.csproj", "src/GarageManagement.Domain.Shared/"]
COPY ["src/GarageManagement.EntityFrameworkCore/GarageManagement.EntityFrameworkCore.csproj", "src/GarageManagement.EntityFrameworkCore/"]
COPY ["src/GarageManagement.HttpApi/GarageManagement.HttpApi.csproj", "src/GarageManagement.HttpApi/"]
COPY ["src/GarageManagement.HttpApi.Host/GarageManagement.HttpApi.Host.csproj", "src/GarageManagement.HttpApi.Host/"]

RUN dotnet restore "src/GarageManagement.HttpApi.Host/GarageManagement.HttpApi.Host.csproj"

COPY . .
WORKDIR /src/src/GarageManagement.HttpApi.Host
RUN abp install-libs
RUN dotnet publish "GarageManagement.HttpApi.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GarageManagement.HttpApi.Host.dll"]
