# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["UrlShortener.App.csproj", "./"]
RUN dotnet restore "UrlShortener.App.csproj"

COPY . ./
RUN dotnet publish "UrlShortener.App.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "UrlShortener.App.dll"]
