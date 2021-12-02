# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Cloud/Cloud.csproj ./Cloud/
COPY ./Common/Common.csproj ./Common/
RUN dotnet restore ./Cloud/Cloud.csproj 

# Copy everything else and build
COPY ./Directory.Build.props ./
COPY ./Cloud ./Cloud
COPY ./Common ./Common
RUN dotnet publish ./Cloud/Cloud.csproj  -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "IntelliHome.Cloud.dll"]