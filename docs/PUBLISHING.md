# Manual release workflow

The repository publishes releases through `.github/workflows/publish-release.yml`. It is intentionally manual: it runs the headless test suite, packs all seven NuGet packages, builds Gallery for Linux, Windows, macOS, and Android, pushes the packages to NuGet.org, then creates a GitHub Release with the Gallery downloads, NuGet packages, symbol packages, and `SHA256SUMS.txt`.

## One-time setup

Add a repository Actions secret named **`NUGET_API_KEY`** with a NuGet.org API key that can publish the seven `Md3.Avalonia*` package IDs. The key is only explicitly passed to the final publish job; test and build steps do not receive it as an environment variable, and it is never stored in the repository.

## Publish a version

1. Open **Actions → Publish Release → Run workflow** and select the `main` branch.
2. Choose `preview` or `stable`.
3. Enter the base `X.Y.Z` version. Do not include a prerelease suffix.
4. For a preview, enter a positive preview sequence. The resulting version is `X.Y.Z-preview.N`; for a stable release, the sequence is ignored and the version is `X.Y.Z`.
5. Run the workflow and wait for every build and test job to pass.

Examples:

| Channel | Base version | Preview sequence | Package / Gallery version | Git tag |
| --- | --- | ---: | --- | --- |
| Preview | `0.4.6` | `1` | `0.4.6-preview.1` | `v0.4.6-preview.1` |
| Stable | `0.4.6` | ignored | `0.4.6` | `v0.4.6` |

The workflow rejects runs from branches other than `main`, malformed versions, and tags that already exist. A preview is marked as a GitHub prerelease. Android's display version is set to the same version as NuGet and desktop Gallery; its integer Android version code is increased per workflow run.

Use a new version for every publication. NuGet.org does not allow replacing a published package version; `--skip-duplicate` lets a retry continue after a partially successful upload, but it does not make it safe to rebuild different contents under an already published version.

## Version files

The workflow supplies an MSBuild version override to the package and Gallery builds and does not edit or commit project files. The checked-in `.csproj` versions therefore remain unchanged by a release. After publishing, bump the project versions and Android `ApplicationDisplayVersion` in a normal reviewed change so local builds and subsequent branch artifacts reflect the new development baseline.

`build-nuget.sh` accepts `--version 0.4.6-preview.1`; `build-nuget.ps1` accepts `-Version 0.4.6-preview.1`. Without either option, both scripts continue to use the version declared in the project files. The ordinary Gallery and NuGet build workflows remain available for branches and pull requests but no longer run on tags or releases; this workflow owns release creation and upload to avoid duplicate publication.
