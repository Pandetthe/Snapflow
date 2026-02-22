ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:3001
EXPOSE 3001

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src


COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

COPY ["src/Presentation/Presentation.csproj", "src/Presentation/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Common/Common.csproj", "src/Common/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/Presentation/Presentation.csproj"

COPY . .
WORKDIR "/src/src/Presentation"
ARG BUILD_CONFIGURATION=Release
RUN dotnet build "Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:3001/alive || exit 1

ENTRYPOINT ["dotnet", "Presentation.dll"]
