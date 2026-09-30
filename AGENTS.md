# AGENTS.md

Guidance for AI coding agents and contributors working in this repository.

## Overview

GeoJSON model types plus JSON converters for Newtonsoft.Json and System.Text.Json, published to
nuget.org as three packages:

| Package | Contents |
|---------|----------|
| `Appy.Spatial.GeoJSON` | Model: `Feature`, `FeatureCollection`, geometries, `Crs`, `BoundingBox` |
| `Appy.Spatial.GeoJSON.Newtonsoft` | `FeatureConverter`, `GeometryConverter`, `JsonSerializerSettings.UseGeoJsonConverters()` |
| `Appy.Spatial.GeoJSON.TextJson` | `FeatureConverter`, `GeometryConverter`, `JsonSerializerOptions.UseGeoJsonConverters()` |

Architecture and CI: [docs/Architecture.md](docs/Architecture.md).

## Build Commands

```bash
dotnet tool restore                          # Cake, MinVer CLI, gpr
dotnet cake                                  # Default target: Clean, Build, Test, Package (.artifacts/)
dotnet test src/Appy.Spatial.Geojson.sln
dotnet test src/Appy.Spatial.Geojson.sln --filter "FullyQualifiedName~SerialisationTests.ShouldRoundTripPoint"
```

- `build.cake` builds the projects listed in `config.yml` (`Type: Package` or `Type: Test`); add new
  projects there.
- Cake builds treat all warnings as errors.
- `Publish` and `Publish-Package-*` targets push to nuget.org and GitHub Packages. They run only in
  GitHub Actions; never run them locally.

## Conventions

- **Central package management**: versions live in `src/Directory.Packages.props`; `PackageReference`
  items carry no `Version`. Framework-specific versions use a `Condition` on `$(TargetFramework)`.
- **Shared metadata**: package info, SourceLink and MinVer are in `src/Directory.Build.props`.
- **Versioning**: MinVer derives the version from git tags (`1.4.0`). No version is stored in files.
- **Package validation**: `dotnet pack` checks each package against the last release
  (`PackageValidationBaselineVersion` in `src/Directory.Build.targets`). API breaks fail the build.
- **Tests**: xUnit + FluentAssertions. Every geometry round-trips through both serializers, as the
  base type and the concrete type, bare and inside a `Feature`. Keep both converter packages in step.
- **Target frameworks** are set per `.csproj`; the SDK is pinned in `global.json`.
- **Style**: `.editorconfig` (4 spaces, 2 for XML/JSON/YAML, CRLF). File-scoped namespaces.
- **Public repo**: no secrets, internal hostnames or private feed URLs in code, docs or commits.

## Git Conventions

- Branch: `feat/<desc>`, `fix/<desc>`, `docs/<desc>`.
- Commits: conventional commits, one line, e.g. `feat(net8): add net8 and drop support for net7`.
  See [CONTRIBUTING.md](CONTRIBUTING.md).

## Documentation

- [docs/Architecture.md](docs/Architecture.md): overview, CI and release flow, plans
- [docs/plans/](docs/plans/): implementation plans
