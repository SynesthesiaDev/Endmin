FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
RUN apk add --no-cache clang build-base zlib-dev
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
    /p:StripSymbols=true

FROM alpine:latest AS final
WORKDIR /app

RUN apk add --no-cache libstdc++ gcompat

COPY --from=build /app/publish/Endmin .

RUN chmod +x ./Endmin

ENTRYPOINT ["./Endmin"]