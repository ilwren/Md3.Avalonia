# Release workflow

The repository publishes releases through `.github/workflows/publish-release.yml`. It is intentionally manual: it runs the headless test suite, packs all seventeen NuGet packages, builds Gallery for Linux, Windows, macOS, and Android, creates a GitHub Release with the Gallery downloads, NuGet packages, symbol packages, and checksums, then pushes the packages to NuGet.org.

The ordinary **Build NuGet** workflow only uploads packages as GitHub Actions Artifacts for that run. It does not create or modify a GitHub Release.

## Release lines and immutable assets

Select `main` to publish the main release line or `avalonia-11` to publish the Avalonia 11 line. Their public identities are deliberately different:

| Item | `main` | `avalonia-11` |
| --- | --- | --- |
| Tag for `0.5.0-preview.2` | `v0.5.0-preview.2` | `v0.5.0-preview.2-avalonia11` |
| Release title | `Md3.Avalonia 0.5.0-preview.2` | `Md3.Avalonia11 0.5.0-preview.2` |
| GitHub Source code target | the selected `main` commit | `main` |
| Linux Gallery archive | `gallery-linux-x64.zip` | `gallery-avalonia11-linux-x64.zip` |
| Desktop executable | `Md3.Avalonia.Gallery.Desktop` | `Md3.Avalonia11.Gallery.Desktop` |
| Android ApplicationId / APK base name | `io.arena.md3avalonia.gallery` | `io.arena.md3avalonia11.gallery` |
| Checksums | `SHA256SUMS.txt` | `SHA256SUMS-avalonia11.txt` |

For an Avalonia 11 release, the packages and Gallery binaries are built from the `avalonia-11` commit, while GitHub's automatically generated **Source code** archives intentionally target `main`. The Release notes state this distinction explicitly. NuGet file names already use the separate `Md3.Avalonia11*` IDs; Windows and macOS Gallery archives receive the same `-avalonia11` asset suffix as Linux.

An existing tag stops the run during validation. An existing GitHub Release also stops the Release job: the workflow never reuses a Release, uploads more assets into it, or replaces same-named assets. This makes a re-run of that job fail intentionally rather than overwrite published files. Use a new version for new binaries; after a later NuGet push failure, re-run only the failed `push-nuget` job.

Publishing uses **NuGet Trusted Publishing** rather than a stored API key. The publish job asks GitHub for an OpenID Connect token, exchanges it at `https://www.nuget.org/api/v2/token` for an API key that nuget.org expires after about twenty minutes, pushes every package with that key, and then lets it lapse. There is no `NUGET_API_KEY` secret in this repository, and nothing long-lived is stored anywhere.

## One-time setup

### 1. Repository variable

Add a repository Actions **variable** (not a secret) named **`NUGET_USERNAME`**, set to the nuget.org account or organization that owns the `Md3.Avalonia11*` packages. The account name is not sensitive — the Trusted Publishing policy is what actually authorises the push — and the token endpoint requires it in the request body.

### 2. Trusted Publishing policy on nuget.org

Open <https://www.nuget.org/account/trustedpublishing> while signed in as the package owner, and create one policy. The account has to be a confirmed account, and it (or an organization it belongs to) has to own at least one package before the form is offered at all.

| Field | Value for this repository |
| --- | --- |
| Policy Name | `md3-avalonia-release` (any name; it only has to be recognisable) |
| Package Owner | the account named by `NUGET_USERNAME` |
| CI/CD Provider | GitHub Actions |
| Repository Owner | `ilwren` |
| Repository | `Md3.Avalonia` |
| Workflow File | `publish-release.yml` |
| Environment | leave empty — the publish job does not run in a named GitHub environment |

Notes on the fields that cause the most failed first pushes:

- **Workflow File** is the file name only. NuGet strips a leading `.github/workflows/`, but the name still has to match the file that requests the token. Renaming `publish-release.yml` silently breaks publishing until the policy is updated.
- **Environment** is matched against the `environment` claim. Because the publish job declares no `environment:`, the claim is absent, so the field must stay empty. Setting it here would make every run fail.
- If this workflow is ever moved into a reusable workflow, the claim that nuget.org sees is `job_workflow_ref` — the *called* file — not this one. The policy would have to name the reusable file instead.

### 3. Pushing all seventeen packages with one policy

This is the part that matters for a repository this size. A policy's scope takes **glob patterns**, so seventeen packages do not need seventeen policies — one pattern covers all of them, because every package ID in `scripts/build-nuget.sh` starts with `Md3.Avalonia11`:

```
Md3.Avalonia11*
```

In **Select Scopes**, tick **Push** and choose which of the two push scopes the policy grants:

| Scope | Meaning | When to pick it |
| --- | --- | --- |
| `package:push` | Create **new** packages and push versions of them | The first publication of a package, or a repository that still adds packages. `Md3.Avalonia11.Notifications` and `Md3.Avalonia11.InkCanvas` are recent enough that this is the safe choice. |
| `package:pushversion` | Push new versions of packages that **already exist** only | Once every ID is registered and the set is frozen. Narrower, so prefer it if no new package is expected. |

`package:unlist` is separate; tick it only if the workflow should be able to unlist a bad version.

With the glob set to `Md3.Avalonia11*`, the single short-lived key the workflow obtains is valid for all seventeen IDs, and the publish step loops over `release-artifacts/release-nuget/*.nupkg` pushing each one with that key. Symbol packages need no separate handling: each `.nupkg` carries the `.snupkg` sitting beside it.

Using `*` instead would also work, but it authorises the workflow to publish any package the account owns, so the narrower prefix is the better default.

If a policy is created **before** a package exists on nuget.org, nuget.org treats it as a temporary policy and resets its validate-by date on each update. Publishing the package promotes it; check the policy list afterwards to confirm it is no longer pending.

## Publish a version

1. Open **Actions → Publish Release → Run workflow** and select `main` or `avalonia-11`, matching the release line you intend to publish.
2. Choose `preview` or `stable`.
3. Enter the base `X.Y.Z` version. Do not include a prerelease suffix.
4. For a preview, enter a positive preview sequence. The resulting version is `X.Y.Z-preview.N`; for a stable release, the sequence is ignored and the version is `X.Y.Z`.
5. Run the workflow and wait for every build and test job to pass.

Examples:

| Channel | Base version | Preview sequence | Package / Gallery version | `main` tag | `avalonia-11` tag |
| --- | --- | ---: | --- | --- | --- |
| Preview | `0.5.0` | `1` | `0.5.0-preview.1` | `v0.5.0-preview.1` | `v0.5.0-preview.1-avalonia11` |
| Preview | `0.5.0` | `2` | `0.5.0-preview.2` | `v0.5.0-preview.2` | `v0.5.0-preview.2-avalonia11` |
| Stable | `0.5.0` | ignored | `0.5.0` | `v0.5.0` | `v0.5.0-avalonia11` |

The workflow rejects runs from branches other than `main` and `avalonia-11`, malformed versions, and tags that already exist. A preview is marked as a GitHub prerelease. Android's display version is set to the same version as NuGet and desktop Gallery; its integer Android version code is increased per workflow run. The Avalonia 11 Android application can be installed beside the main application because their ApplicationIds differ.

Use a new version for every publication. NuGet.org does not allow replacing a published package version; `--skip-duplicate` lets a retry continue after a partially successful upload, but it does not make it safe to rebuild different contents under an already published version.

## NuGet source repository metadata

All package projects inherit `RepositoryUrl` and `RepositoryType` from the root `Directory.Build.props`. This emits a `<repository type="git" url="https://github.com/ilwren/Md3.Avalonia" />` element in each `.nuspec`, which NuGet.org displays as the source repository link. The metadata applies to newly packed and published versions; an existing NuGet version cannot be rewritten.

## If the push fails

The publish step reports which half of the exchange broke:

| Message | Cause |
| --- | --- |
| `The Actions OIDC request returned no token.` | The publish job lost `id-token: write`, or the run is not on GitHub-hosted runners. |
| `nuget.org did not return an API key.` | The policy does not match. Compare the repository owner, repository, workflow file and environment against the table above; the response body from nuget.org is printed beneath the message. |
| `Repository variable NUGET_USERNAME must name…` | The variable was never added, or was added as a secret rather than a variable. |
| `Expected 17 .nupkg files, found N.` | The pack step and this check disagree. The expected count is derived from `PACKAGE_IDS` in `scripts/build-nuget.sh`, so this means the pack job did not produce every package. |

A key that expired mid-run — possible only if a push is stalled for more than about twenty minutes — shows up as an authentication error from `dotnet nuget push` partway through the loop. Re-running only the failed `push-nuget` job is safe: `--skip-duplicate` skips the versions that already landed. Re-running the Release job for the same version deliberately fails instead of changing its published assets.

## Version files

The workflow supplies an MSBuild version override to the package and Gallery builds and does not edit or commit project files. The checked-in `.csproj` versions therefore remain unchanged by a release. After publishing, bump the project versions and Android `ApplicationDisplayVersion` in a normal reviewed change so local builds and subsequent branch artifacts reflect the new development baseline.

`build-nuget.sh` accepts `--version 0.5.0-preview.2`; `build-nuget.ps1` accepts `-Version 0.5.0-preview.2`. Without either option, both scripts continue to use the version declared in the project files. The ordinary Gallery and NuGet build workflows remain available for branches and pull requests but no longer run on tags or releases; this workflow owns release creation and upload to avoid duplicate publication.
