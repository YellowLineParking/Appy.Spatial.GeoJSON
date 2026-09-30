# .NET 10 Migration

- **Status:** Planned
- **Branch:** `feat/net10`

## Summary

Move the three packages to `net10.0;net9.0;net8.0` on the .NET 10 SDK, drop `net6.0` and
`netstandard2.0`, and bring the Cake build, GitHub Actions and test stack in line with
[Appy.Configuration](https://github.com/YellowLineParking/Appy.Configuration). Released as
**2.0.0**. No public API change.

## Decisions

| Decision | Context | Alternatives Considered |
|----------|---------|------------------------|
| Libraries `net10.0;net9.0;net8.0`, tests `net10.0` | `net6.0` is out of support; `net8.0` and `net9.0` keep current users building | Keep `netstandard2.0`: keeps the nullable shim and the System.Text.Json package for no current need |
| Version **2.0.0** | Dropping `netstandard2.0` ends .NET Framework and pre-net8 support: breaking under SemVer. Those users stay on 1.4.x | Minor bump (Appy.Configuration used 1.2.0 for the same drop): hides the break |
| Remove the `System.Text.Json` package | In-box on net8+; the package only served `netstandard2.0`/`net6.0` | Keep it: NU1510 pruning warning, fails the warnings-as-errors build |
| `Newtonsoft.Json` floor stays **13.0.3** | Raising it to 13.0.4 breaks users who pin 13.0.3 directly (NU1605). 13.0.3 has no advisory | 13.0.4 (latest): no functional gain |
| Tests: `xunit.v3` 3.2.2, `xunit.runner.visualstudio` 3.1.5, FluentAssertions 7.2.2, `Microsoft.NET.Test.Sdk` 18.10.1, `GitHubActionsTestLogger` 3.0.5, `XunitXml.TestLogger` 8.0.0 | xUnit v2 is in maintenance. FluentAssertions 8 needs a commercial licence | xUnit 2.9.3 (as in Appy.Configuration); `xunit.v3` 4.x |
| Build tooling as Appy.Configuration: Cake 6.0.0, Cake.MinVer 4.0.0, Cake.Yaml 6.0.0, YamlDotNet 16.2.0, MinVer 2.3.0, traversal `src/build.csproj`, `.sln` kept | Same pipeline across repos; MinVer 2.3.0 keeps versioning unchanged | MinVer 8: new pre-release options, out of scope |
| Package validation against 1.4.0 | Catches accidental API breaks; only the dropped TFMs are suppressed (PKV006) | None; Appy.Configuration has no validation |
| `MinVerMinimumMajorMinor` 2.0 (MSBuild and `build.cake`) | The master push that follows the merge publishes a preview. Without this it is `1.4.1-preview`, carrying breaking changes. Cake computes the package path, so both must agree | Skip it and accept one misleading preview |

## Tasks

- [ ] 1. `feat(net10): target net10.0, net9.0 and net8.0`. `global.json` SDK `10.0.103`,
  `rollForward: latestFeature`. The three library csproj files go to `net10.0;net9.0;net8.0`, tests
  to `net10.0`. Remove the `netstandard2.0` shims (`Nullable` package, `PackageDownload`
  `Microsoft.NETCore.App.Ref`, `AnnotatedReferenceAssemblyVersion`, per-project `LangVersion`) and
  the `System.Text.Json` package. `PackageTags` become `NET10;NET9;NET8`.
  Test: the 6 `SerialisationTests` pass on `net10.0` (`dotnet test src/Appy.Spatial.Geojson.sln`).
- [ ] 2. `chore(deps): move tests to xunit v3`. Update `src/Directory.Packages.props` to the test
  stack above. Remove unused entries (`Moq`, `MartinCostello.Logging.XUnit`,
  `TunnelVisionLabs.ReferenceAssemblyAnnotator`, `coverlet.collector`) and the
  `Version="$(...)"` attributes in the test csproj.
  Test: same 6 tests pass. The run reports a non-zero count.
- [ ] 3. `chore(build): upgrade cake to 6.0.0 with traversal build`. Update `dotnet-tools.json`,
  `build.cake` (addins, traversal branch), `global.json` `msbuild-sdks` (Traversal 4.1.82), and add
  `src/build.csproj`. `config.yml` still drives packing and publishing.
  Test: `dotnet tool restore && dotnet cake` is green. `.artifacts/` holds 3 nupkgs with
  `lib/net8.0`, `lib/net9.0`, `lib/net10.0` only.
- [ ] 4. `ci: build and publish with .NET 10 SDK`. In `ci.yaml` and `publish.yaml`, move to
  `actions/checkout@v6`, `actions/cache@v5` and `actions/setup-dotnet@v5`, with SDKs `8.0.x`,
  `9.0.x` and `10.0.x`. Triggers stay as they are.
  Test: PR checks green on Windows, macOS and Linux.
- [ ] 5. `build: validate packages against 1.4.0`. Add `EnablePackageValidation` and
  `PackageValidationBaselineVersion` 1.4.0 to packable projects. Pack fails first on PKV006 (the
  dropped TFMs). Then add a `CompatibilitySuppressions.xml` with PKV006 only.
  Test: `dotnet cake` is green. No CP0xxx (API) diagnostics.
- [ ] 6. `build: start versions at 2.0`. Set `MinVerMinimumMajorMinor` 2.0 in
  `src/Directory.Build.targets` and `.WithMinimumMajorMinor("2.0")` in `build.cake`.
  Test: `dotnet cake` logs `2.0.0-preview.0.N`. The nupkg names match the version Cake logs.
- [ ] 7. `docs: update readme and contributing for net10`. The README gets a supported-frameworks
  line and says that `netstandard2.0`/`net6.0` users should stay on 1.4.x. In `CONTRIBUTING.md`,
  replace `build.ps1` with `dotnet tool restore && dotnet cake`.
  Test: the links resolve.
- [ ] 8. Release (maintainers). Rebase-merge the PR. The master push publishes
  `2.0.0-preview.0.N`. Tag `2.0.0` on master to publish the release. Then commit
  `docs(readme): change packages version to 2.0.0` (badges).

## Verification

- [ ] `dotnet cake` (Default target) green locally on SDK 10, warnings as errors, 6/6 tests.
- [ ] Nuspecs: TextJson has no `System.Text.Json` dependency; Newtonsoft depends on
  `Newtonsoft.Json` >= 13.0.3; groups only for net8.0, net9.0 and net10.0.
- [ ] Package validation passes with only PKV006 suppressed.
- [ ] PR CI green on all three OS jobs.
- [ ] Done when merged and nuget.org lists 2.0.0 for `Appy.Spatial.GeoJSON`,
  `Appy.Spatial.GeoJSON.Newtonsoft` and `Appy.Spatial.GeoJSON.TextJson`.

## Key Files

```
global.json                          # SDK 10 + Traversal SDK
dotnet-tools.json                    # Cake 6
build.cake                           # Cake pipeline, MinVer settings
config.yml                           # projects to build, test and pack
src/build.csproj                     # traversal project (new)
src/Directory.Build.props            # package metadata, package validation
src/Directory.Build.targets          # MinVer settings
src/Directory.Packages.props         # central package versions
src/*/*.csproj                       # target frameworks
.github/workflows/ci.yaml            # PR build
.github/workflows/publish.yaml       # publish on master push or tag
```
