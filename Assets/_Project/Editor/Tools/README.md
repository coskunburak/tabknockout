# Editor Tools

Project-owned Unity Editor tools live here.

Place here:

- Scene builders
- Config validators
- content reports
- build preflight tools
- controlled scene wiring tools

Do not place here:

- Runtime gameplay code
- Destructive tools without confirmation
- Tools that silently overwrite production scenes

Rule:

- Tools must log created or modified assets.
- Scene wiring tools must use Undo, mark scenes dirty, and prefer deactivating legacy test objects over deleting them.

Current safe content tools:

- `Tools > Tap Knockout > Content > Create Vertical Slice Chapter Content` creates the 10-room Phase 1 vertical slice chapter, room, wave, and placeholder enemy config assets without editing scenes or prefabs.
- `Tools > Tap Knockout > Chapter > Wire Chapter Room Flow` wires the open scene to `Chapter_VerticalSlice_01` and sets visible RoomManager/WaveManager inspector config fields to the first vertical slice room/wave.
- `Tools > Tap Knockout > Chapter > Validate Vertical Slice Room Flow` checks the open scene for the required 10-room chapter, reward flow, continue panel, and visible RoomManager/WaveManager assignments.
- `Tools > Tap Knockout > Chapter > Deactivate Legacy Room Test Objects` disables old scene-only test helpers such as `Ground_Test`, `SpawnPoints`, and standalone `CameraBounds` without deleting them.
