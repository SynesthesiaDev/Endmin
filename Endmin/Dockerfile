FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Endmin/Endmin.csproj", "Endmin/"]
RUN dotnet restore "Endmin/Endmin.csproj"

COPY . .
WORKDIR "/src/Endmin"

RUN dotnet publish "./Endmin.csproj" \
    -c Release \
    -r linux-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final
WORKDIR /app

RUN apk add --no-cache icu-libs libstdc++ gcompat

COPY --from=build /app/publish .

RUN chmod +x ./Endmin

ENTRYPOINT ["./Endmin"]