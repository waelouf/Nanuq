# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 5000

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Nanuq.WebApi/Nanuq.WebApi.csproj", "Nanuq.WebApi/"]
COPY ["Nanuq.Common/Nanuq.Common.csproj", "Nanuq.Common/"]
COPY ["Nanuq.EF/Nanuq.EF.csproj", "Nanuq.EF/"]
COPY ["Nanuq.Kafka/Nanuq.Kafka.csproj", "Nanuq.Kafka/"]
COPY ["Nanuq.RabbitMQ/Nanuq.RabbitMQ.csproj", "Nanuq.RabbitMQ/"]
COPY ["Nanuq.Redis/Nanuq.Redis.csproj", "Nanuq.Redis/"]
COPY ["Nanuq.ServiceDefaults/Nanuq.ServiceDefaults.csproj", "Nanuq.ServiceDefaults/"]
COPY ["Nanuq.Sqlite/Nanuq.Sqlite.csproj", "Nanuq.Sqlite/"]
RUN dotnet restore "./Nanuq.WebApi/Nanuq.WebApi.csproj"
COPY . .
WORKDIR "/src/Nanuq.WebApi"
RUN dotnet build "./Nanuq.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Nanuq.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_HTTP_PORTS=5000
USER root
RUN chmod 777 /app/Database
ENTRYPOINT ["dotnet", "Nanuq.WebApi.dll"]