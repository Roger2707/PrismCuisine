FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY PrismCuisine.sln ./
COPY src/ ./src/

RUN dotnet restore src/Api/PrismCuisine.Api/PrismCuisine.Api.csproj
RUN dotnet publish src/Api/PrismCuisine.Api/PrismCuisine.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PrismCuisine.Api.dll"]
