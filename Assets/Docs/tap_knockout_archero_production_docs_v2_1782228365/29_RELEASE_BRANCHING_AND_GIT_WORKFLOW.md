# Release, Branching, and Git Workflow

## Current State

The project directory is not currently a Git repository. Production implementation should not start until Git is initialized or another version-control plan is approved.

## Initial Git Setup Recommendation

1. Initialize Git at `/Users/burakcoskun/TapKnockout`.
2. Add Unity `.gitignore` rules for generated folders:
   - `Library/`
   - `Temp/`
   - `Logs/`
   - `Build/`
   - `Builds/`
   - `UserSettings/` if team-specific settings should stay local
3. Commit current documentation baseline first.
4. Commit project settings only after deliberate review.

## Branch Naming

| Type | Pattern | Example |
|---|---|---|
| Documentation | `docs/<topic>` | `docs/production-sprint-plan` |
| Chore | `chore/<topic>` | `chore/production-folder-structure` |
| Feature | `feat/<system>` | `feat/dash-impact-foundation` |
| Art | `art/<topic>` | `art/licensed-placeholder-pass` |
| QA | `qa/<topic>` | `qa/android-vertical-slice-gate` |
| Fix | `fix/<bug>` | `fix/room-clear-stuck-state` |
| Release | `release/<version>` | `release/0.1.0-vslice` |

## Commit Grouping

Keep commits reviewable:

- Docs and code in separate commits when possible.
- Data/config assets separate from runtime code.
- Scene/prefab changes separate from scripts.
- Asset imports separate from prefab bindings.
- Generated files should not be committed unless Unity requires `.meta` files for real assets.

Suggested commit labels:

- `docs:`
- `chore:`
- `feat:`
- `fix:`
- `test:`
- `art:`
- `qa:`
- `build:`

## Pull Request Checklist

- Scope matches sprint/prompt.
- Out-of-scope items were not added.
- No direct `.unity` YAML edits unless explicitly approved.
- No real SDKs unless explicitly approved.
- No generated folders committed.
- Changed files are listed.
- Tests/validation steps are listed.
- Manual Unity setup is explained.
- Asset licenses are updated if assets were used.
- Rollback path is clear.

## Versioning

Recommended early versions:

- `0.1.0-docs-foundation`
- `0.2.0-core-combat`
- `0.3.0-room-loop`
- `0.4.0-vertical-slice`
- `0.5.0-soft-launch-candidate`

Android:

- Increment version code for each shared build.
- Keep debug/internal builds separate from release builds.

## Release Channels

| Channel | Purpose |
|---|---|
| Local Editor | Fast development and manual setup. |
| Android Debug APK | Device smoke testing. |
| Internal Test | Team/QA distribution. |
| Closed Test | Pre-soft-launch external testers. |
| Soft Launch | Limited geography/public test. |
| Global Launch | Public release after KPI validation. |

## Rollback Policy

- Every sprint branch should be revertible without removing unrelated work.
- Config changes should be easy to disable.
- Monetization and LiveOps features need remote/local toggles.
- Build outputs should be reproducible from source, not committed.

