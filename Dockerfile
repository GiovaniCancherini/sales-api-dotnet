
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SalesApi.WebApi/SalesApi.WebApi.csproj", "src/SalesApi.WebApi/"]
COPY ["src/SalesApi.Application/SalesApi.Application.csproj", "src/SalesApi.Application/"]
COPY ["src/SalesApi.Common/SalesApi.Common.csproj", "src/SalesApi.Common/"]
COPY ["src/SalesApi.Domain/SalesApi.Domain.csproj", "src/SalesApi.Domain/"]
COPY ["src/SalesApi.IoC/SalesApi.IoC.csproj", "src/SalesApi.IoC/"]
COPY ["src/SalesApi.ORM/SalesApi.ORM.csproj", "src/SalesApi.ORM/"]
RUN dotnet restore "./src/SalesApi.WebApi/SalesApi.WebApi.csproj"
COPY . .
WORKDIR "/src/src/SalesApi.WebApi"
RUN dotnet build "./SalesApi.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SalesApi.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SalesApi.WebApi.dll"]
