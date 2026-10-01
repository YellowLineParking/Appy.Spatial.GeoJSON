# .NET 10 Migration

- **Status:** In Progress
- **Branch:** `feat/net10`

## Summary

Move the three packages to `net10.0;net8.0` on the .NET 10 SDK, drop `net6.0` and `netstandard2.0`,
and bring the Cake build, GitHub Actions and test stack in line with
[Appy.Configuration](https://github.com/YellowLineParking/Appy.Configuration). Released as
**2.0.0**. No public API change.

## Decisions

| Decision | Context | Alternatives Considered |
|----------|---------|------------------------|
| Libraries `net10.0;net8.0`, tests `net10.0;net8.0` | The two LTS releases. `net9.0` apps resolve the `net8.0` asset; `net6.0` is out of support. Tests run on both because `net8.0` uses the in-box System.Text.Json 8 | Also `net9.0`: no API gain. Keep `netstandard2.0`: keeps the nullable shim and the System.Text.Json package for no current need |
| Version **2.0.0** | Dropping `netstandard2.0` ends .NET Framework and pre-net8 support: breaking under SemVer. Those users stay on 1.4.x | Minor bump (Appy.Configuration used 1.2.0 for the same drop): hides the break |
| Remove the `System.Text.Json` package | In-box on net8+; the package only served `netstandard2.0`/`net6.0` | Keep it: NU1510 pruning warning, fails the warnings-as-errors build |
| `Newtonsoft.Json` **13.0.4** | Latest stable. Apps that pin a lower version directly must bump it with 2.0.0 (NU1605) | Keep 13.0.3: no advisory against it, but stays behind latest |
| Tests: `xunit.v3` 3.2.2, `xunit.runner.visualstudio` 3.1.5, FluentAssertions 7.2.2, `Microsoft.NET.Test.Sdk` 18.10.1, `GitHubActionsTestLogger` 2.4.1 | xUnit v2 is in maintenance. FluentAssertions 8 needs a commercial licence. Logger 3.x and `XunitXml.TestLogger` 8.x need Microsoft.Testing.Platform 2 (CS1705 against xunit.v3 3.2.2's 1.9.1); the XML logger was unused | xUnit 2.9.3 (as in Appy.Configuration); `xunit.v3` 4.x |
| Build tooling as Appy.Configuration: Cake 6.3.0, Cake.MinVer 4.0.0, Cake.Yaml 6.0.0, YamlDotNet 16.2.0, MinVer 2.3.0; Source Link from the SDK (no `Microsoft.SourceLink.GitHub` package); `build.cake` builds the `config.yml` projects one by one | Same pipeline across repos; MinVer 2.3.0 keeps versioning unchanged | Traversal `src/build.csproj`: not adopted yet. MinVer 8: new pre-release options, out of scope |
| Solution as `src/Appy.Spatial.GeoJSON.slnx` | SDK 10 format; the name now matches the package casing | Keep the classic solution format |
| CI installs the SDK in `global.json` plus 8.0.x | SDK 10 builds both targets; 8.0.x provides the .NET 8 runtime for the `net8.0` test run; the Cake, MinVer and gpr tools roll forward | SDK 10 only: leaves the `net8.0` assets untested |
| `nuget.config` with nuget.org only | CPM raises NU1507 when a machine has several package sources; one repo-level source keeps restores reproducible | Suppress NU1507 (as Appy.Configuration does): hides where packages come from |
| Package validation against 1.4.0 | Catches accidental API breaks; only the dropped TFMs are suppressed (PKV006) | None; Appy.Configuration has no validation |
| `MinVerMinimumMajorMinor` 2.0 (MSBuild and `build.cake`) | The master push that follows the merge publishes a preview. Without this it is `1.4.1-preview`, carrying breaking changes. Cake computes the package path, so both must agree | Skip it and accept one misleading preview |

## Tasks

- [x] 1. `feat(net10): target net10.0 and net8.0`. `global.json` SDK `10.0.103`,
  `rollForward: latestFeature`. The three library csproj files go to `net10.0;net8.0`, tests
  to `net10.0` (later `net10.0;net8.0`, from review). Remove the `netstandard2.0` shims (`Nullable` package, `PackageDownload`
  `Microsoft.NETCore.App.Ref`, `AnnotatedReferenceAssemblyVersion`, per-project `LangVersion`) and
  the `System.Text.Json` package. `PackageTags` become `NET10;NET8`.
  Test: the 6 `SerialisationTests` pass on `net10.0` and `net8.0`
  (`dotnet test src/Appy.Spatial.GeoJSON.slnx`).
- [x] 2. `chore(deps): move tests to xunit v3`. Update `src/Directory.Packages.props` to the test
  stack above. Remove unused entries (`Moq`, `MartinCostello.Logging.XUnit`,
  `TunnelVisionLabs.ReferenceAssemblyAnnotator`, `coverlet.collector`, `XunitXml.TestLogger`) and the
  `Version="$(...)"` attributes in the test csproj.
  Test: same 6 tests pass. The run reports a non-zero count.
- [x] 3. `chore(build): upgrade cake to 6.0.0`, then 6.3.0. Update `dotnet-tools.json` and the `build.cake`
  addins. `config.yml` still drives build, test, pack and publish (no traversal build).
  Test: `dotnet tool restore && dotnet cake` is green. `.artifacts/` holds 3 nupkgs with
  `lib/net8.0` and `lib/net10.0` only.
- [x] 4. `ci: build and publish with .NET 10 SDK`. In `ci.yaml` and `publish.yaml`, move to
  `actions/checkout@v6`, `actions/cache@v5` and `actions/setup-dotnet@v5`, SDK from `global.json`
  plus 8.0.x for the `net8.0` test run (from review). Triggers stay as they are.
  Test: PR checks green on Windows, macOS and Linux.
- [x] 5. `build: validate packages against 1.4.0`. Add `EnablePackageValidation` and
  `PackageValidationBaselineVersion` 1.4.0 to packable projects. Pack fails first on PKV006 (the
  dropped TFMs). Then add a `CompatibilitySuppressions.xml` with PKV006 only.
  Test: `dotnet cake` is green. No CP0xxx (API) diagnostics.
- [x] 6. `build: start versions at 2.0`. Set `MinVerMinimumMajorMinor` 2.0 in
  `src/Directory.Build.targets` and `.WithMinimumMajorMinor("2.0")` in `build.cake`.
  Test: `dotnet cake` logs `2.0.0-preview.0.N`. The nupkg names match the version Cake logs.
- [x] 7. `docs: update readme and contributing for net10`. The README gets a supported-frameworks
  line and says that `netstandard2.0`/`net6.0` users should stay on 1.4.x. In `CONTRIBUTING.md`,
  replace `build.ps1` with `dotnet tool restore && dotnet cake`.
  Test: the links resolve.

## Verification

- [x] `dotnet cake` (Default target) green locally on SDK 10, warnings as errors, 6/6 tests on net10.0 and net8.0.
- [x] Nuspecs: TextJson has no `System.Text.Json` dependency; Newtonsoft depends on
  `Newtonsoft.Json` >= 13.0.4; groups only for net8.0 and net10.0.
- [x] Package validation passes with only PKV006 suppressed.
- [x] PR CI green on all three OS jobs.
- [ ] Done when merged and nuget.org lists 2.0.0 for `Appy.Spatial.GeoJSON`,
  `Appy.Spatial.GeoJSON.Newtonsoft` and `Appy.Spatial.GeoJSON.TextJson`.

## Release

Maintainers rebase-merge the PR; the master push publishes `2.0.0-preview.0.N`. Tagging `2.0.0` on
master publishes the release. Then `docs(readme): change packages version to 2.0.0` (badges), and
move `PackageValidationBaselineVersion` to 2.0.0 and delete the `CompatibilitySuppressions.xml` files.

## Key Files

```
global.json                          # SDK 10
dotnet-tools.json                    # Cake 6.3.0
nuget.config                         # nuget.org as the only package source (new)
build.cake                           # Cake pipeline, MinVer settings
config.yml                           # projects to build, test and pack
src/Directory.Build.props            # package metadata
src/Directory.Build.targets          # package validation, MinVer settings
src/*/CompatibilitySuppressions.xml  # PKV006 for the dropped TFMs (new)
src/Directory.Packages.props         # central package versions
src/Appy.Spatial.GeoJSON.slnx        # solution (new, replaces the classic one)
src/*/*.csproj                       # target frameworks
.github/workflows/ci.yaml            # PR build
.github/workflows/publish.yaml       # publish on master push or tag
```
