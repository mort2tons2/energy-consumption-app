# Build context is the StromForbrok/ solution directory (see <DockerfileContext> in the csproj).
# The frontend is a separate Aspire resource and is not built here.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["StromForbrok-webapp/StromForbrok.Api/StromForbrok.Api.csproj", "StromForbrok-webapp/StromForbrok.Api/"]
COPY ["StromForbrok.ServiceDefaults/StromForbrok.ServiceDefaults.csproj", "StromForbrok.ServiceDefaults/"]
RUN dotnet restore "StromForbrok-webapp/StromForbrok.Api/StromForbrok.Api.csproj"
COPY . .
WORKDIR "/src/StromForbrok-webapp/StromForbrok.Api"
RUN dotnet publish "StromForbrok.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StromForbrok.Api.dll"]
