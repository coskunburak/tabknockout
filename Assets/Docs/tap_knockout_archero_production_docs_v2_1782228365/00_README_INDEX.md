# Tap Knockout — Production Docs v2

Bu paket, önceki 1–2 günlük arena MVP yaklaşımını tamamen değiştirir. Proje artık **Unity ile geliştirilecek, ticari hedefli, Archero tarzı mobil aksiyon roguelite** olarak ele alınır.

## Yeni Ürün Tanımı

Tap Knockout, dikey ekranda oynanan, tek parmak kontrolüne sahip, oda/room bazlı roguelite mobil aksiyon oyunudur. Oyuncu kısa odaları temizler, run sırasında geçici yetenekler seçer, run dışında gear/talent/metaprogression ile kalıcı güçlenir. Oyunun özgün kimliği **dash-impact / knockback** mekaniğidir.

## Ana Referans Yapı

Archero’dan alınacak genel tür kalıbı:

- Portrait mobil oynanış
- Oda oda ilerleme
- Wave/enemy clear
- Boss odaları
- Run sırasında ability seçimi
- Run dışında gear/talent upgrade
- Daily rewards, missions, ads/IAP, liveops

Kopyalanmayacak şeyler:

- Archero assetleri
- UI layout birebir
- İsimler
- Skill ikonları
- Balance tabloları
- Düşman/harita tasarımları
- Store/monetization ekranları

## Ana Pillarlar

1. **Tek parmakla okunabilir kontrol**
2. **Oda bazlı roguelite run loop**
3. **Dash-impact ile özgün combat kimliği**
4. **Gear, talent, currency ve liveops odaklı ticari yapı**
5. **Production-level Unity mimarisi**
6. **Analytics, remote config, QA ve monetization-ready altyapı**

## Dosyalar

| Dosya | Amaç |
|---|---|
| `AGENTS.md` | Codex kök kuralları |
| `01_PRODUCT_VISION.md` | Ürün vizyonu ve pazar konumu |
| `02_GDD_PRODUCTION.md` | Production game design document |
| `03_TECH_ARCHITECTURE_UNITY.md` | Unity teknik mimari |
| `04_COMBAT_AND_ABILITIES.md` | Combat, dash, auto-attack, ability sistemi |
| `05_LEVEL_ROOM_WAVE.md` | Chapter, room, wave, boss sistemi |
| `06_META_ECONOMY.md` | Gear, talents, currencies, rewards |
| `07_MONETIZATION_LIVEOPS.md` | Ads, IAP, shop, missions, events |
| `08_UI_UX_CONTROLS.md` | Mobil UI/UX ve kontroller |
| `09_ASSET_PIPELINE.md` | Asset, animasyon, VFX, audio, lisans |
| `10_ANALYTICS_REMOTE_CONFIG.md` | Analytics, funnel, remote config, A/B |
| `11_QA_PERFORMANCE_RELEASE.md` | QA, performans, build, release |
| `12_CODEX_AGENT_GUIDE.md` | Codex çalışma sınırları |
| `13_ROADMAP.md` | Milestone planı |
| `14_RISK_REGISTER.md` | Riskler |
| `15_STORE_COMPLIANCE.md` | Store, privacy, ads/IAP compliance |
| `16_CODEX_PROMPTS.md` | Hazır Codex promptları |
| `17_CREDITS_TEMPLATE.md` | Lisans/credit takip şablonu |
| `18_REPOSITORY_DISCOVERY_AND_DOCUMENTATION_AUDIT.md` | Current repository discovery and docs audit |
| `19_MISSING_DOCUMENTATION_PLAN.md` | Missing/weak documentation plan and consolidation decisions |
| `20_BACKLOG_MASTER.md` | Production backlog epics, priorities, dependencies, acceptance |
| `21_VERTICAL_SLICE_SPEC.md` | Commercial vertical slice scope and acceptance |
| `22_PRODUCTION_SPRINT_PLAN.md` | Detailed milestone/sprint implementation plan |
| `23_TECHNICAL_DECISIONS_ADR.md` | Architecture decision record |
| `24_DATA_CONFIG_SCHEMA.md` | ScriptableObject/config/save/event schema |
| `25_PREFAB_AND_SCENE_CONTRACTS.md` | Scene, prefab, UI, and Editor builder contracts |
| `26_MONETIZATION_ANALYTICS_REMOTE_CONFIG_SPEC.md` | Event schema, remote keys, fake ads/IAP, SDK gates |
| `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md` | QA matrix, mobile performance budget, KPI plan |
| `28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md` | Asset intake, balance sheets, Editor tools |
| `29_RELEASE_BRANCHING_AND_GIT_WORKFLOW.md` | Git, branches, commits, release channels |
| `30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md` | Initial ability, enemy, and boss catalog |

## Current Repository Reality

The current Unity project is still close to a template:

- Unity `6000.5.0f1`
- URP `17.5.0`
- One build scene: `Assets/Scenes/SampleScene.unity`
- No production gameplay scripts yet
- No `Assets/_Project` folder yet
- No `Assets/ThirdParty` folder yet
- Existing staged asset packs are under `Assets/Assets/game asset packs`
- The root was not a Git repository during the documentation audit
- A root `AGENTS.md` now points Codex to this docs package

## NotebookLM Kullanımı

NotebookLM notebook adı:

```text
Tap Knockout Production v2
```

Tüm `.md` dosyalarını kaynak olarak ekle. Codex’e her task öncesi bu kaynaklardan yalnızca ilgili dokümanları sorgulat.

## Codex Okuma Sırası

Her task öncesi Codex şunları okumalı:

1. `AGENTS.md`
2. `00_README_INDEX.md`
3. `12_CODEX_AGENT_GUIDE.md`
4. `22_PRODUCTION_SPRINT_PLAN.md` when executing sprint work
5. Göreve özel domain dokümanı

## İlk Hedef

İlk hedef artık küçük MVP değil, **commercial vertical slice foundation**:

- Universal 3D / URP Unity projesi
- Android-first portrait yapı
- Clean folder structure
- Player movement
- Stop-to-attack veya auto-attack
- Dash-impact
- Projectile/damage system
- Enemy spawner
- Room/wave loop
- Ability selection
- Basic HUD
- Run result
- Gear/talent/economy stubs
- Analytics/ads/IAP stubs
- Editor scene builder
- Android test build
