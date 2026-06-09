FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY PrismERP.sln ./
COPY src/ ./src/

RUN dotnet restore src/Api/PrismERP.Api/PrismERP.Api.csproj
RUN dotnet publish src/Api/PrismERP.Api/PrismERP.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PrismERP.Api.dll"]
