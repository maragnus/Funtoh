FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
EXPOSE 8080
ARG APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Funtoh.Data/Funtoh.Data.csproj", "Funtoh.Data/"]
COPY ["Funtoh.Web/Funtoh.Web.csproj", "Funtoh.Web/"]
RUN dotnet restore "Funtoh.Web/Funtoh.Web.csproj"
COPY . .
WORKDIR "/src/Funtoh.Web"
RUN dotnet build "Funtoh.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Funtoh.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Application Configuration
ENV ASPNETCORE_URLS http://+:8080

# Garbage Collection: Server mode, Concurrent, 256 MB limit, 0-9 Conservation Strategy
ENV DOTNET_gcServer 1
ENV DOTNET_gcConcurrent 1
ENV DOTNET_GCHeapHardLimit 0x10000000
ENV DOTNET_GCConserveMemory 5

USER $APP_UID
ENTRYPOINT ["dotnet", "Funtoh.Web.dll"]
