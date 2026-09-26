# Playability batch — 2026-09-26

Target: Android 0.2.9 / code 11. One combined device review after implementation.

Implemented together:

- Ten obstacle prefabs: receipts/tax, police barrier, masked thief, barrier,
  spinner, rolling safe, traffic car, receipt wall, warning sign, closing doors.
- Nine world building prefabs with coast terraces, storefronts, floor bands,
  luxury terraces, domes or futuristic spires according to world identity.
- Separate cash notes, safe/win sparks, loss streaks and victory/jackpot confetti.
  Eight reused particle systems; maximum 64 particles per system.
- Static meshes merged by color, cached prefab lookups and moving door references,
  distance visibility for scenery, disabled scenery shadows and one camera lookup
  per floating-text update. Obstacles instantiate the new prefab directly.

Imported art report: `AssetProduction/Reviews/Playability/import.json`.
Art has no colliders; gameplay keeps original root triggers and movement rules.
Closing door groups remain separate. Each new prefab is limited to 12 renderers
and 20,000 triangles. These budgets are not a measured device FPS guarantee.

Art import passed: 19 assets; obstacles 2–9 renderers / 60–11,236 triangles;
buildings 3–5 renderers / 276–1,684 triangles. Eight particle textures/materials.
All nine PlayMode scenarios passed on the final scripts (64.16 seconds).
Obstacle grid and world screenshots inspected. One initial run timed out while
source changes triggered recompilation; the unchanged final run passed.
APK 0.2.9 / code 11 installed on Xiaomi 2310FPCA4G, Android 15, ARM64.
52,436,091 bytes; SHA256
`BADA94F37219C67DDA38D6AC0503A04F1E43C3C826B662063AC3022CDB6CF809`.
Signature v2, ZIP/native ELF 16 KB alignment checks passed.
All four save files match across installation: backups
`TestResults/Device/20260926-151612-141-Backup` and
`TestResults/Device/20260926-151950-315-Backup`.
Game launch passed; screenshot `20260926-152048-963-Capture/screen.png` has
no missing-shader magenta pixels. Process log `20260926-152047-102-Logs/logcat.txt`
contains zero game error/warning matches.
Combined manual review: pending; obstacles, environment, effects, smoothness.
