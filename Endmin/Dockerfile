# 1. Build Stage - Use the standard Debian-based SDK (faster/more stable for AOT)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Install clang and zlib (Debian names)
RUN apt-get update && apt-get install -y clang zlib1g-dev
WORKDIR /src

COPY ["Endmin/Endmin.csproj", "Endmin/"]
RUN dotnet restore "Endmin/Endmin.csproj"

COPY . .
WORKDIR "/src/Endmin"

RUN dotnet publish "./Endmin.csproj" \
    -c Release \
    -r linux-arm64 \
    -o /app/publish \
    /p:PublishAot=true \
    /p:StripSymbols=true \
    --self-contained
    
FROM alpine:latest AS final
WORKDIR /app

RUN apk add --no-cache libstdc++ gcompat icu-libs

COPY --from=build /app/publish/Endmin .
RUN chmod +x ./Endmin

ENTRYPOINT ["./Endmin"]