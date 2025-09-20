# AzurePandoc

This project exposes the Pandoc 3.3.0 CLI via a .NET 9 Web API suitable for Azure App Service container deployments.

Running locally (requires dotnet 9 and pandoc on PATH):

```pwsh
cd AzurePandoc
dotnet run --project AzurePandoc.csproj
```

Docker build and run:

```pwsh
cd AzurePandoc
docker build -t azurepandoc:local .
docker run -p 8080:80 azurepandoc:local
```

Endpoints:
- GET /api/pandoc/version - returns pandoc --version
- GET /api/pandoc/formats - returns --list-output-formats
- POST /api/pandoc/convert - JSON body { input, args } returns converted output
- POST /api/pandoc/run - JSON body { args, input } runs arbitrary pandoc args

Notes:
- The container installs Pandoc 3.3.0 from upstream .deb; verify licensing and security for production.
- The /run endpoint executes arbitrary args against the pandoc binary — consider restricting allowed flags in production.
