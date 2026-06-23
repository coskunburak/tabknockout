# Unity Technical Architecture

## Engine

- Unity 6 / current LTS
- Template: Universal 3D / URP
- Platform: Android first, iOS later
- Orientation: Portrait
- Target FPS: 60
- Art style: stylized 3D / low-poly / mobile-friendly

## Folder Structure

```text
Assets/
  _Project/
    Art/
    Audio/
    Prefabs/
    Scenes/
      Boot.unity
      Home.unity
      Gameplay.unity
    Scripts/
      Core/
      Bootstrap/
      Input/
      Player/
      Combat/
      Enemy/
      Ability/
      Projectile/
      Level/
      Room/
      Wave/
      Economy/
      Meta/
      UI/
      Camera/
      Audio/
      Analytics/
      Ads/
      IAP/
      Save/
      Config/
      Utilities/
    ScriptableObjects/
      Player/
      Weapons/
      Enemies/
      Abilities/
      Chapters/
      Rooms/
      Economy/
      Monetization/
      Balance/
    Editor/
      Tools/
    Tests/
      EditMode/
      PlayMode/
    Docs/
  ThirdParty/
    Kenney/
    KayKit/
    Quaternius/
    Mixamo/
    VFX/
```

## Scenes

### Boot

- Initialize services
- Load configs
- Load save
- Load home/gameplay

### Home

- Chapter select
- Gear
- Talents
- Shop
- Daily rewards
- Missions

### Gameplay

- Runtime chapter
- Player/enemies
- Room/wave manager
- HUD
- Ability selection
- Run result

## Services

Use abstractions:

```text
IAnalyticsService
IRemoteConfigService
IAdService
IIapService
ISaveService
IAudioService
```

Initial implementations:

```text
ConsoleAnalyticsService
LocalRemoteConfigService
FakeAdService
FakeIapService
JsonSaveService
AudioService
```

## Core Components

Player:

- PlayerMovementController
- PlayerAttackController
- PlayerDashController
- PlayerHealth
- PlayerStats
- PlayerAnimationController

Combat:

- IDamageable
- HitContext
- DamageSystem
- ProjectileController
- ProjectilePool
- KnockbackReceiver
- StatusEffectController

Enemy:

- EnemyController
- EnemyMovement
- EnemyAttack
- EnemyHealth
- EnemySpawner

Level:

- ChapterRunner
- RoomManager
- WaveManager
- RoomRewardController
- BossController

UI:

- HudController
- AbilityChoicePanel
- RunResultPanel
- HomeController
- GearScreenController
- ShopScreenController

## ScriptableObject Configs

- PlayerConfig
- WeaponConfig
- EnemyConfig
- AbilityConfig
- ChapterConfig
- RoomTemplateConfig
- WaveConfig
- RewardTableConfig
- EconomyConfig
- MonetizationConfig

## Performance

Use pooling for:

- Projectiles
- Enemies
- Hit VFX
- Damage text
- Pickups

Avoid runtime Instantiate/Destroy during combat where possible.
