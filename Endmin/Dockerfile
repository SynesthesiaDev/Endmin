FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
USER $APP_UID
WORKDIR /app
ENV DOTNET_GCLatencyMode=0
ENV DOTNET_GCHeapHardLimitPercent=70
ENV DOTNET_GCConserveMemory=9

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Endmin/Endmin.csproj", "Endmin/"]
RUN dotnet restore "Endmin/Endmin.csproj" -r linux-musl-arm64
COPY . .
WORKDIR "/src/Endmin"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./Endmin.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    --self-contained false\
    p:UseAppHost=false \
    p:PublishTrimmed=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Endmin.dll"]