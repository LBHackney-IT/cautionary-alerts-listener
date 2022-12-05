FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'
ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

ARG SONAR_TOKEN
ENV SONAR_TOKEN=$SONAR_TOKEN

WORKDIR /app

RUN apt-get update && apt-get install -y openjdk-11-jdk
RUN dotnet tool install --global dotnet-sonarscanner --version 5.6.0
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet sonarscanner begin /k:"LBHackney-IT_cautionary-alerts-listener" /o:"lbhackney-it" /d:sonar.host.url=https://sonarcloud.io /d:sonar.login="${SONAR_TOKEN}"

# Copy csproj and restore as distinct layers
COPY ./CautionaryAlertsListener.sln ./
COPY ./CautionaryAlertsListener/CautionaryAlertsListener.csproj ./CautionaryAlertsListener/
COPY ./CautionaryAlertsListener.Tests/CautionaryAlertsListener.Tests.csproj ./CautionaryAlertsListener.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./CautionaryAlertsListener/CautionaryAlertsListener.csproj
RUN dotnet restore ./CautionaryAlertsListener.Tests/CautionaryAlertsListener.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c Release -o out CautionaryAlertsListener/CautionaryAlertsListener.csproj
RUN dotnet build -c debug -o out CautionaryAlertsListener.Tests/CautionaryAlertsListener.Tests.csproj

CMD dotnet test
RUN dotnet sonarscanner end /d:sonar.login="${SONAR_TOKEN}"
