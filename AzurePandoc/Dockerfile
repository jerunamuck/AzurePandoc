FROM mcr.microsoft.com/dotnet/aspnet:9.0-jammy AS base
WORKDIR /app

# Install pandoc 3.3.0
RUN apt-get update \
    && apt-get install -y wget ca-certificates gnupg \
    && wget https://github.com/jgm/pandoc/releases/download/3.3.0/pandoc-3.3.0-1-amd64.deb -O /tmp/pandoc.deb \
    && apt-get install -y /tmp/pandoc.deb \
    && rm -f /tmp/pandoc.deb \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0-jammy AS build
WORKDIR /src
COPY ["AzurePandoc.csproj", "./"]
RUN dotnet restore "AzurePandoc.csproj"
COPY . .
RUN dotnet publish "AzurePandoc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "AzurePandoc.dll"]
