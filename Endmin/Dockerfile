FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
ENV DOTNET_GCLatencyMode=0
ENV DOTNET_GCHeapHardLimitPercent=70
ENV DOTNET_GCConserveMemory=9

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src
COPY ["Endmin/Endmin.csproj", "Endmin/"]
RUN case "$TARGETARCH" in \
      amd64) echo x64 > /tmp/arch ;; \
      arm64) echo arm64 > /tmp/arch ;; \
      *) echo "unsupported: $TARGETARCH" && exit 1 ;; \
    esac \
 && dotnet restore "Endmin/Endmin.csproj" -a "$(cat /tmp/arch)"
COPY . .
WORKDIR "/src/Endmin"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Endmin.csproj" \
    -c $BUILD_CONFIGURATION \
    -a "$(cat /tmp/arch)" \
    -o /app/publish \
    --no-restore \
    --self-contained false \
    /p:UseAppHost=false \
    /p:PublishTrimmed=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Endmin.dll"]