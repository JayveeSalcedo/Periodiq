# CI: Android build (GitHub Actions)

Workflow: `.github/workflows/build-android.yml` (uses [GameCI unity-builder v4](https://game.ci/docs/github/builder)).

## When it runs
- Every push and PR to `main` → builds an APK, downloadable from the run's **Artifacts** (kept 14 days).
- Tag `v*` (e.g. `git tag v0.1.0 && git push --tags`) → also attaches the APK to a GitHub Release.
- Manually: Actions tab → Build Android → **Run workflow**.
- Until `ProjectSettings/ProjectVersion.txt` is committed, the job skips the build with a notice.

## One-time setup: secrets
Repo → Settings → Secrets and variables → Actions → **New repository secret**:

| Secret | Value |
|---|---|
| `UNITY_EMAIL` | Your Unity account email |
| `UNITY_PASSWORD` | Your Unity account password |
| `UNITY_LICENSE` | Full contents of your `Unity_lic.ulf` file |

### Getting `Unity_lic.ulf` (Personal license)
1. Open Unity Hub, sign in, and make sure a Personal license is active (Preferences → Licenses).
2. Find the file:
   - Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
   - macOS: `/Library/Application Support/Unity/Unity_lic.ulf`
   - Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`
3. Open it in a text editor, copy **everything**, paste as `UNITY_LICENSE`.

If activation fails, check GameCI's activation docs: https://game.ci/docs/github/activation

## Notes
- First build takes ~20–40 min (downloads the Unity Android image); cached builds are faster.
- Version number comes from git tags (Semantic versioning), e.g. tag `v0.1.0`.
- For the Play Store later: switch `androidExportType` to `androidAppBundle` and add keystore secrets (`androidKeystoreBase64`, `androidKeystorePass`, `androidKeyaliasName`, `androidKeyaliasPass`).
