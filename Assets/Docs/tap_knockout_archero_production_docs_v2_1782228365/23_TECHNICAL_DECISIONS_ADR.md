# Technical Decisions ADR

Use this file for durable architecture decisions. New decisions should be appended, not rewritten without reason.

## ADR-0001 - No Unity MCP By Default

Status: Accepted

Decision: Work through repository and filesystem inspection unless the user explicitly confirms Unity MCP is available.

Rationale: Official Unity MCP may be unavailable due to capacity limits. Repository-safe workflows are predictable and auditable.

Consequences:

- Scene and prefab work must be done through manual Unity steps or approved Editor scripts.
- Codex must not inspect or modify the live Unity Editor unless the user changes this rule.

## ADR-0002 - Do Not Directly Edit `.unity` Scene YAML

Status: Accepted

Decision: Future scene setup should use Unity Editor manual steps or Editor tooling, not raw YAML scene edits.

Rationale: Raw Unity scene YAML is fragile and easy to corrupt without Editor validation.

Consequences:

- Implementation prompts must explain manual setup when scene objects are needed.
- Editor builders are allowed after approval because Unity serializes resulting assets.

## ADR-0003 - Production Code Under `Assets/_Project`

Status: Accepted

Decision: New production scripts, prefabs, ScriptableObjects, art, UI, tests, and tools should live under `Assets/_Project`.

Rationale: The repository currently has template/tutorial files and staged asset packs. A clean production root prevents sprawl.

Consequences:

- Existing tutorial files should not be extended into gameplay.
- Third-party assets should not be mixed with production prefabs.

## ADR-0004 - Third-Party Assets Under `Assets/ThirdParty`

Status: Accepted

Decision: Approved external assets should live under `Assets/ThirdParty/<Source>/<PackName>`.

Rationale: Licensing, updates, and credits are easier to audit when source assets are separated from game-authored assets.

Consequences:

- Current `Assets/Assets/game asset packs` folder is a staging area until migration is approved.
- Unknown-license assets must not be used.

## ADR-0005 - Data-Driven Configs With Stable IDs

Status: Accepted

Decision: Gameplay, enemy, room, ability, economy, monetization, and remote config data should be driven by ScriptableObject assets with stable ids.

Rationale: Mobile roguelites require frequent tuning, remote config readiness, and content expansion.

Consequences:

- Hardcoded balance values are acceptable only as local defaults in configs.
- IDs use lowercase snake_case and must not change after save/analytics exposure.

## ADR-0006 - Service Abstractions Before SDKs

Status: Accepted

Decision: Analytics, remote config, ads, IAP, save, and audio must be behind interfaces before real SDK integration.

Rationale: This keeps gameplay code independent from vendor SDKs and makes compliance review possible.

Consequences:

- Initial implementations are console/local/fake services.
- Real SDK integration requires explicit user approval and compliance review.

## ADR-0007 - Dash-Impact Is a First-Class Combat Source

Status: Accepted

Decision: Dash hits are represented in shared combat data with `isDashHit`, impact damage, knockback, and dash-specific events.

Rationale: Dash-impact is the product differentiator, not a cosmetic movement extension.

Consequences:

- Ability, analytics, VFX, and enemy interrupt systems must observe dash events.
- Dash tuning belongs in configs and remote-config-ready values.

## ADR-0008 - No Real Monetization SDKs In Vertical Slice

Status: Accepted

Decision: The vertical slice may include fake rewarded ad and IAP flows only.

Rationale: Real monetization SDKs introduce privacy, consent, store, testing, and account obligations too early.

Consequences:

- Rewarded revive, double rewards, free chest, and reroll flows can be simulated.
- SDK work is deferred to soft-launch readiness.

## ADR-0009 - Console Analytics First

Status: Accepted

Decision: Analytics events should first route to a console/no-op implementation with strict event names and parameter schemas.

Rationale: Event correctness can be validated before vendor selection and dashboard work.

Consequences:

- Gameplay code emits events through `IAnalyticsService`.
- Event names and parameters are documented in `26_MONETIZATION_ANALYTICS_REMOTE_CONFIG_SPEC.md`.

## ADR-0010 - Git Required Before Production Implementation

Status: Proposed

Decision: Initialize Git before any production code sprint.

Rationale: The current directory is not a Git repository. Production implementation without history makes rollback unsafe.

Consequences:

- Use Unity `.gitignore`.
- Commit docs baseline first.
- Keep implementation branches small and named by sprint.

