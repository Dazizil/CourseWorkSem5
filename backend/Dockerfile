# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Сначала только csproj — слой с restore кэшируется, пока не меняются зависимости
COPY SocialNetwork.slnx ./
COPY src/SocialNetwork.Domain/SocialNetwork.Domain.csproj src/SocialNetwork.Domain/
COPY src/SocialNetwork.Application/SocialNetwork.Application.csproj src/SocialNetwork.Application/
COPY src/SocialNetwork.Infrastructure/SocialNetwork.Infrastructure.csproj src/SocialNetwork.Infrastructure/
COPY src/SocialNetwork.Api/SocialNetwork.Api.csproj src/SocialNetwork.Api/
RUN dotnet restore src/SocialNetwork.Api/SocialNetwork.Api.csproj

COPY src/ src/
RUN dotnet publish src/SocialNetwork.Api/SocialNetwork.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "SocialNetwork.Api.dll"]
