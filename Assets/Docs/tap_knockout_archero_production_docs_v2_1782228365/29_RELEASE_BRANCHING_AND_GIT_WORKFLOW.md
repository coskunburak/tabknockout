# Release, Branching, and Git Workflow

## Current State

The current workspace is inside a Git work tree. Continue using Git hygiene before production implementation and avoid mixing broad gameplay changes with documentation-only work.

## Branch Naming

Use the Codex branch prefix unless the user requests another pattern:

| Type | Pattern | Example |
|---|---|---|
| Documentation | `codex/docs-<topic>` | `codex/docs-desktop-survivor-pivot` |
| Feature | `codex/feat-<system>` | `codex/feat-arena-run-director` |
| Fix | `codex/fix-<bug>` | `codex/fix-level-up-resume` |
| QA | `codex/qa-<topic>` | `codex/qa-survivor-stress-test` |
| Release | `codex/release-<version>` | `codex/release-demo-0.1.0` |

## Commit Grouping

Keep commits reviewable:

- Docs and code in separate commits when possible.
- Runtime code separate from config/data assets.
- Scene/prefab changes separate from scripts.
- Asset imports separate from prefab bindings.
- Generated folders should not be committed.
- `.meta` files should be included only when Unity assets/folders are intentionally created or changed.

Suggested labels:

- `docs:`
- `feat:`
- `fix:`
- `test:`
- `chore:`
- `qa:`
- `build:`

## Pull Request Checklist

- Scope matches sprint/prompt.
- No out-of-scope systems added.
- No direct `.unity` YAML edits unless explicitly approved.
- No unapproved packages or SDKs.
- No generated folders committed.
- Changed files listed.
- Tests/validation listed.
- Manual Unity setup explained.
- Asset licenses updated if assets were used.
- Rollback path clear.

## Versioning

Recommended early versions:

- `0.1.0-docs-pivot`
- `0.2.0-desktop-prototype`
- `0.3.0-core-arena-loop`
- `0.4.0-ability-level-up`
- `0.5.0-vertical-slice`
- `0.6.0-steam-demo-candidate`

## Release Channels

| Channel | Purpose |
|---|---|
| Unity Editor | Fast local validation. |
| Local Desktop Build | Internal smoke test. |
| Internal Playtest | Trusted testers. |
| Steam Playtest | External controlled feedback. |
| Steam Demo | Public demo when quality gates pass. |
| Full Release | Future commercial release. |

## Rollback Policy

- Every sprint branch should be revertible without removing unrelated work.
- Config changes should be easy to disable.
- Experimental systems should be isolated behind configs.
- Build outputs should be reproducible from source, not committed.

## Deprecated Release Channels

Android Debug APK, mobile closed test, mobile soft launch, and mobile global launch are future port channels only.
