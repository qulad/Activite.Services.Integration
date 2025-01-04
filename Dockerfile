FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

ENV ASPNETCORE_URLS=http://+:5001

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/Activite.Services.Integration/Activite.Services.Integration.csproj", "src/Activite.Services.Integration/"]
RUN dotnet restore "src/Activite.Services.Integration/Activite.Services.Integration.csproj"
COPY . .
WORKDIR "/src/src/Activite.Services.Integration"
RUN dotnet build "Activite.Services.Integration.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Activite.Services.Integration.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Activite.Services.Integration.dll"]
