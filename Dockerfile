FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["babbly-api-gateway/babbly-api-gateway.csproj", "babbly-api-gateway/"]
RUN dotnet restore "babbly-api-gateway/babbly-api-gateway.csproj"
COPY . .
WORKDIR "/src/babbly-api-gateway"
RUN dotnet build "babbly-api-gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "babbly-api-gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "babbly-api-gateway.dll"] 