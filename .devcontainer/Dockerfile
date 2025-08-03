FROM mcr.microsoft.com/dotnet/sdk:9.0

RUN apt-get update && apt-get install -y curl git procps
RUN dotnet tool install --global dotnet-ef

WORKDIR /workspace