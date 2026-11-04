ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG TARGETARCH
ARG VERSION=2.0.0
RUN apt-get update \
    && apt-get install --yes --no-install-recommends clang zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /source
COPY global.json .editorconfig Directory.Build.props Directory.Packages.props ./
COPY src/TravelingSalesman.Core/TravelingSalesman.Core.csproj src/TravelingSalesman.Core/
COPY src/TravelingSalesman.Cli/TravelingSalesman.Cli.csproj src/TravelingSalesman.Cli/
RUN case "${TARGETARCH:-amd64}" in arm64) rid=linux-arm64 ;; *) rid=linux-x64 ;; esac \
    && echo "$rid" > /tmp/rid \
    && dotnet restore src/TravelingSalesman.Cli/TravelingSalesman.Cli.csproj --runtime "$rid"
COPY src/ src/
RUN dotnet publish src/TravelingSalesman.Cli/TravelingSalesman.Cli.csproj \
        --configuration Release \
        --runtime "$(cat /tmp/rid)" \
        --no-restore \
        --output /app \
        -p:Version="${VERSION}"

FROM mcr.microsoft.com/dotnet/runtime-deps:${DOTNET_VERSION}-noble-chiseled AS final
LABEL org.opencontainers.image.title="tsp" \
      org.opencontainers.image.description="Traveling Salesman Problem solver" \
      org.opencontainers.image.source="https://github.com/kusl/tsp"
WORKDIR /app
COPY --from=build /app/tsp ./tsp
USER $APP_UID
ENTRYPOINT ["/app/tsp"]
