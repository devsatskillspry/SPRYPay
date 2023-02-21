FROM mcr.microsoft.com/dotnet/sdk:6.0.401-bullseye-slim AS builder
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /source
COPY nuget.config nuget.config
COPY Build/Common.csproj Build/Common.csproj
COPY SPRYPayServer.Abstractions/SPRYPayServer.Abstractions.csproj SPRYPayServer.Abstractions/SPRYPayServer.Abstractions.csproj
COPY SPRYPayServer/SPRYPayServer.csproj SPRYPayServer/SPRYPayServer.csproj
COPY SPRYPayServer.Common/SPRYPayServer.Common.csproj SPRYPayServer.Common/SPRYPayServer.Common.csproj
COPY SPRYPayServer.Rating/SPRYPayServer.Rating.csproj SPRYPayServer.Rating/SPRYPayServer.Rating.csproj
COPY SPRYPayServer.Data/SPRYPayServer.Data.csproj SPRYPayServer.Data/SPRYPayServer.Data.csproj
COPY SPRYPayServer.Client/SPRYPayServer.Client.csproj SPRYPayServer.Client/SPRYPayServer.Client.csproj
RUN cd SPRYPayServer && dotnet restore
COPY SPRYPayServer.Common/. SPRYPayServer.Common/.
COPY SPRYPayServer.Rating/. SPRYPayServer.Rating/.
COPY SPRYPayServer.Data/. SPRYPayServer.Data/.
COPY SPRYPayServer.Client/. SPRYPayServer.Client/.
COPY SPRYPayServer.Abstractions/. SPRYPayServer.Abstractions/.
COPY SPRYPayServer/. SPRYPayServer/.
COPY Build/Version.csproj Build/Version.csproj
ARG CONFIGURATION_NAME=Release
ARG GIT_COMMIT
RUN cd SPRYPayServer && dotnet publish -p:GitCommit=${GIT_COMMIT} --output /app/ --configuration ${CONFIGURATION_NAME}

FROM mcr.microsoft.com/dotnet/aspnet:6.0.9-bullseye-slim

RUN apt-get update && apt-get install -y --no-install-recommends iproute2 openssh-client \
    && rm -rf /var/lib/apt/lists/* 

ENV LC_ALL en_US.UTF-8
ENV LANG en_US.UTF-8

WORKDIR /datadir
WORKDIR /app
ENV SPRYPAY_DATADIR=/datadir
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
VOLUME /datadir

COPY --from=builder "/app" .
COPY docker-entrypoint.sh docker-entrypoint.sh
ENTRYPOINT ["/app/docker-entrypoint.sh"]
