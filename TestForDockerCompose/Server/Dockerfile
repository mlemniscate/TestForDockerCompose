#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TestForDockerCompose/Server/TestForDockerCompose.Server.csproj", "TestForDockerCompose/Server/"]
COPY ["TestForDockerCompose/Client/TestForDockerCompose.Client.csproj", "TestForDockerCompose/Client/"]
COPY ["TestForDockerCompose/Shared/TestForDockerCompose.Shared.csproj", "TestForDockerCompose/Shared/"]
RUN dotnet restore "TestForDockerCompose/Server/TestForDockerCompose.Server.csproj"
COPY . .
WORKDIR "/src/TestForDockerCompose/Server"
RUN dotnet build "TestForDockerCompose.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestForDockerCompose.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestForDockerCompose.Server.dll"]