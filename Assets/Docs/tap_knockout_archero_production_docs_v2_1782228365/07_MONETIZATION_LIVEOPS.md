# Monetization and LiveOps

## Current Position

Monetization and LiveOps are future optional topics. The immediate target is a PC/Steam-facing survivor prototype and vertical slice.

For MVP, prioritize:

- Fun run loop.
- Reliable desktop controls.
- Performance under enemy density.
- Readable UI and VFX.
- Balance telemetry.
- Steam demo readiness.

## Preferred Commercial Framing

For desktop, the cleanest starting model is:

- Free demo or playtest build.
- Premium full game later.
- Optional DLC/cosmetic expansion only after core validation.

Do not design the MVP around rewarded ads, interstitial ads, mobile daily shops, or IAP bundles.

## Future Monetization Options

Possible later options:

- Premium Steam release.
- Demo-to-full purchase.
- Cosmetic DLC.
- Soundtrack or supporter pack.
- Expansion content.

Mobile monetization can be reconsidered only after the PC direction is validated.

## LiveOps Scope

MVP liveops means operational tuning and playtest learning, not a mobile event calendar.

Useful future liveops:

- Balance patches.
- Limited-time challenge arena.
- New enemy/boss variants.
- New active skills.
- Steam playtest feedback cycles.
- Telemetry-driven difficulty tuning.

## Remote Config

Remote/local config should support safe tuning:

- Spawn rates.
- Wave timing.
- XP curve.
- Ability weights.
- Boss timing.
- Enemy health/damage multipliers.
- Active skill cooldown multipliers.

No remote config provider SDK is approved by default. Local config defaults must work offline.

## SDK Policy

No real Ads, IAP, Analytics, crash, or remote config SDKs may be added without explicit approval.

If future monetization SDKs are considered, the project must first define:

- Privacy policy.
- Data collection disclosure.
- Store/platform requirements.
- Restore/purchase behavior.
- Rollback plan.
- Test mode plan.

## Deprecated Mobile Concepts

- Rewarded revive as a core loop pillar.
- Double reward ads.
- Free chest timers.
- Interstitial pacing.
- Starter pack funnels.
- Daily login reward economy.
- Mobile event monetization.

These remain only as historical ideas.
