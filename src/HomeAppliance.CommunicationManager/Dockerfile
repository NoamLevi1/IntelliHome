# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./HomeAppliance.CommunicationManager/HomeAppliance.CommunicationManager.csproj ./HomeAppliance.CommunicationManager/
COPY ./Common/Common.csproj ./Common/
RUN dotnet restore ./HomeAppliance.CommunicationManager/HomeAppliance.CommunicationManager.csproj 

# Copy everything else and build
COPY ./Directory.Build.props ./
COPY ./HomeAppliance.CommunicationManager ./HomeAppliance.CommunicationManager
COPY ./Common ./Common
RUN dotnet publish ./HomeAppliance.CommunicationManager/HomeAppliance.CommunicationManager.csproj  -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "IntelliHome.HomeAppliance.CommunicationManager.dll"]