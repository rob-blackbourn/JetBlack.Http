# Development

## Deployment

```bash
dotnet build
dotnet pack JetBlack.Http/JetBlack.Http.csproj
VERSION=1.0.1
dotnet nuget push JetBlack.Http/bin/Debug/JetBlack.Http.${VERSION}.nupkg --api-key ${NUGET_API_KEY} --source https://api.nuget.org/v3/index.json
```