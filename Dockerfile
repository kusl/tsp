# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Version configuration - change these two lines when upgrading .NET versions
ARG DOTNET_VERSION=9.0
ARG DOTNET_VERSION_EXACT=9.0

# These ARGs allow for swapping out the base used to make the final image when debugging from VS
ARG LAUNCHING_FROM_VS
# This sets the base image for final, but only if LAUNCHING_FROM_VS has been defined
ARG FINAL_BASE_IMAGE=${LAUNCHING_FROM_VS:+aotdebug}

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION} AS base
USER $APP_UID
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
# Install clang/zlib1g-dev dependencies for publishing to native
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang zlib1g-dev
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and Directory.Build.props FIRST
COPY ["TSP.sln", "./"]
COPY ["Directory.Build.props", "./"]

# Copy project files for both projects
COPY ["TravelingSalesman.ConsoleApp/TravelingSalesman.ConsoleApp.csproj", "TravelingSalesman.ConsoleApp/"]
COPY ["TravelingSalesman.Core/TravelingSalesman.Core.csproj", "TravelingSalesman.Core/"]

# Restore the console app (which will also restore its dependencies)
RUN dotnet restore "TravelingSalesman.ConsoleApp/TravelingSalesman.ConsoleApp.csproj"

# Copy all source code
COPY . .

WORKDIR "/src/TravelingSalesman.ConsoleApp"
RUN dotnet build "TravelingSalesman.ConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TravelingSalesman.ConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

# This stage is used as the base for the final stage when launching from VS to support debugging in regular mode (Default when not using the Debug configuration)
FROM base AS aotdebug
USER root
# Install GDB to support native debugging
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    gdb
USER app

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
# Re-declare the ARG in this stage so it's available
ARG DOTNET_VERSION=9.0
FROM ${FINAL_BASE_IMAGE:-mcr.microsoft.com/dotnet/runtime-deps:${DOTNET_VERSION}} AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./TravelingSalesman.ConsoleApp"]
