# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG DOTNET_VERSION=8.0
ARG BUILD_CONFIGURATION=Release

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:$DOTNET_VERSION AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_VERSION AS build
WORKDIR /src
COPY ["_Apps/TradeX.API/TradeX.API.csproj", "_Apps/TradeX.API/"]
COPY ["Application/TradeX.Application/TradeX.Application.csproj", "Application/TradeX.Application/"]
COPY ["Repository/TradeX.Repository/TradeX.Repository.csproj", "Repository/TradeX.Repository/"]
COPY ["Domain/TradeX.Domain/TradeX.Domain.csproj", "Domain/TradeX.Domain/"]
COPY ["Domain/TradeX.Domain.Abstractions/TradeX.Domain.Abstractions.csproj", "Domain/TradeX.Domain.Abstractions/"]
COPY ["Repository/TradeX.Repository.Abstractions/TradeX.Repository.Abstractions.csproj", "Repository/TradeX.Repository.Abstractions/"]
COPY ["Application/TradeX.Application.Abstractions/TradeX.Application.Abstractions.csproj", "Application/TradeX.Application.Abstractions/"]
COPY ["Common/API.Abstractions/API.Abstractions.csproj", "Common/API.Abstractions/"]
COPY ["Infrastructure/TradeX.Infrastrucure.JwtAuthorization/TradeX.Infrastrucure.JwtAuthorization.csproj", "Infrastructure/TradeX.Infrastrucure.JwtAuthorization/"]
RUN dotnet restore "./_Apps/TradeX.API/TradeX.API.csproj"
COPY . .
WORKDIR "/src/_Apps/TradeX.API"
RUN dotnet build "./TradeX.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
RUN dotnet publish "./TradeX.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TradeX.API.dll"]