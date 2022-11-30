FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'
ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./CautionaryAlertsListener.sln ./
COPY ./CautionaryAlertsListener/CautionaryAlertsListener.csproj ./CautionaryAlertsListener/
COPY ./CautionaryAlertsListener.Tests/CautionaryAlertsListener.Tests.csproj ./CautionaryAlertsListener.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./CautionaryAlertsListener/CautionaryAlertsListener.csproj
RUN dotnet restore ./CautionaryAlertsListener.Tests/CautionaryAlertsListener.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out CautionaryAlertsListener.Tests/CautionaryAlertsListener.Tests.csproj

CMD dotnet test
