FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DocWriter/DocWriter/DocWriter.csproj", "DocWriter/DocWriter/"]
COPY ["DocWriter/DocWriter.Client/DocWriter.Client.csproj", "DocWriter/DocWriter.Client/"]
RUN dotnet restore "DocWriter/DocWriter/DocWriter.csproj"
COPY . .
WORKDIR "/src/DocWriter/DocWriter"
RUN dotnet build "DocWriter.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DocWriter.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocWriter.dll"]
