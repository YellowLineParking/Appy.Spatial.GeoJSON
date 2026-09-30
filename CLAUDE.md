# CLAUDE.md

@./AGENTS.md

## Build & Test

```bash
dotnet tool restore
dotnet cake                                  # Default: clean, build, test, pack into .artifacts/
dotnet test src/Appy.Spatial.Geojson.sln     # tests only
```

Never run the `Publish` cake target or `dotnet nuget push`: publishing happens only in GitHub Actions.
