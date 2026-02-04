# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj + restore first (cache friendly)
COPY ECG.Api/ECG.Api.csproj ECG.Api/
RUN dotnet restore ECG.Api/ECG.Api.csproj

# copy everything and publish
COPY . .
RUN dotnet publish ECG.Api/ECG.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

# folder for local uploads later
RUN mkdir -p /app/uploads

ENTRYPOINT ["dotnet", "ECG.Api.dll"]
