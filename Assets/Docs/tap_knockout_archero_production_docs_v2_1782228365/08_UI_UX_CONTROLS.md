# UI, UX, and Controls

## Control Scheme

MVP desktop controls:

| Input | Action |
|---|---|
| WASD | Move. |
| Mouse | Aim. |
| Left Mouse | Primary attack policy, either hold-to-fire or aim source for auto-fire. |
| Right Mouse | Optional alternate active skill or dash if selected. |
| Space or Shift | Dash/evade. |
| Q/E/R/F | Active skills. |
| 1/2/3/4 | Alternate active skill hotkeys. |
| Esc | Pause/settings. |
| Tab | Build overview. |

Controller support is a future option. Touch controls are deprecated for the desktop prototype.

## Primary HUD

Required HUD elements:

- Health bar.
- XP bar and current level.
- Run timer.
- Active skill cooldown slots.
- Dash cooldown indicator.
- Boss health bar when active.
- Wave/elite/boss warning banner.
- Pickup/level-up feedback.
- Pause button or key prompt.

Optional:

- Minimap or arena compass.
- Damage numbers.
- Buff/debuff row.
- Kill count.
- Current build summary shortcut.

## Level-Up Modal

The level-up modal should:

- Pause or safely slow combat.
- Show 3 choices by default.
- Show icon, name, category, rarity, short effect text, and stack/current level.
- Support keyboard and mouse selection.
- Avoid long text and tiny fonts.
- Resume combat only after selection is applied.

## Active Skill UX

Each skill slot needs:

- Icon.
- Hotkey label.
- Cooldown radial/fill.
- Charges if applicable.
- Disabled/invalid state.
- Clear cast feedback.

The player must be able to understand which skills are ready without looking away from combat for too long.

## Readability Under Density

Readability is a core UX requirement:

- Enemy silhouettes must remain distinct from pickups and VFX.
- Boss and elite telegraphs must override background clutter.
- XP orbs should be visible without hiding danger zones.
- Damage numbers should be optional or capped.
- Important warnings should not overlap level-up cards or boss HP.
- UI must be legible at common desktop resolutions.

## Screen Flow

MVP screens:

- Boot/loading.
- Main menu.
- Character/loadout select, minimal.
- Gameplay HUD.
- Pause menu.
- Level-up choice modal.
- Run result screen.
- Settings.

Future screens:

- Ability codex.
- Meta progression.
- Challenge arena select.
- Steam demo feedback link.

## Settings

MVP settings should include:

- Resolution/fullscreen mode.
- Master/music/SFX volume.
- Mouse sensitivity if camera/aim needs it.
- Damage numbers on/off if implemented.
- Screenshake intensity.
- VSync/frame cap if supported.
- Keybinds, at least planned.

## Deprecated Mobile UX

Portrait safe-area UI, virtual joystick, touch attack controls, mobile notch layout, and rewarded-ad panels are not part of the desktop MVP.
