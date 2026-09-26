# V2 Development Status

## Current assessment — quality rework, 2026-09-26

User rejected models, worlds, remaining skins and obstacles after 0.3.0.
Functional prototype; art not accepted and release incomplete. "Completed"
below describes implementation/testing, not product approval. Current art is draft.
Audit: [GAME_AUDIT_2026-09-26.md](GAME_AUDIT_2026-09-26.md).
Active workflow: [NEXT_WORK_PLAN.md](NEXT_WORK_PLAN.md).
Audit checked specification, source/catalogs and existing visual evidence;
no new generation, build or manual run. Next is a coherent playable reference
before bulk world/skin/obstacle production. Publication is not ready.

## Major update 0.3.0 / code 12 — 2026-09-26

Completed nine new Meshy assets, Blender mobile geometry and texture rebaking,
female Humanoid character, animated costumes, redesigned menu/HUD/results,
panel/button animation, swipe onboarding, original music and local frame timing.
Final automated checks: 10 PlayMode + 45 EditMode passed. Android checks pass:
API 36, ARM64, non-debuggable, ZIP and six ELF libraries aligned for 16 KB.
48,042,298-byte APK installed; four save files match before/after installation.
One combined manual review and measured gameplay performance remain pending.
Meshy spent 300 existing credits, balance 752. No purchases or Play upload.
Publication materials and owner requirements: GOOGLE_PLAY_RELEASE.md.
Device evidence and checklist: MAJOR_DEVICE_QA.md. Older entries follow.

## Playability batch — 2026-09-26

0.2.9 / code 11 implementation: ten obstacles, nine world building variants,
distinct pooled effects and rendering/runtime optimizations. Art import and nine
PlayMode scenarios pass. APK checks pass; installed with four save files unchanged.
Loaded device screenshot and logs pass. Combined manual run review is pending. See
[playability QA](PLAYABILITY_DEVICE_QA.md). Earlier sections are historical.

## Four gate styles and launcher icon — 2026-09-26

Current Android: 0.2.8 / code 10, installed. Four Meshy crests were optimized in
Blender and integrated with distinct SAFE/RISK/JACKPOT/INVESTMENT frames. New
logo launcher icons are configured and packaged. Eight PlayMode scenarios and
APK checks pass; four saves match across update. Loaded device menu/logs pass.
The user confirmed normal launcher icon, gate readability and run smoothness.
See [gate styles QA](GATE_STYLES_DEVICE_QA.md). Actual Meshy cost: 80 credits.

## UI, gates and logo update — 2026-09-26

Current Android: 0.2.7 / code 9. Four elements shipped together: beveled buttons,
action icons, imported gate frame and new transparent logo. Eight PlayMode
scenarios and APK checks pass. Installed with four save files unchanged; device
menu was inspected and logs pass. The user reported normal menus/gameplay;
gate art needs greater variety. Next batch: four distinct gate styles.
See [interface QA](INTERFACE_ART_DEVICE_QA.md). Earlier APK sections are historical.

## Door correction — 2026-09-26

Current Android: 0.2.6 / code 8, installed and verified. Door now opens outward;
seven PlayMode scenarios pass. Four saves matched across update. Device finish
image and logs pass, and the user confirmed the corrected direction. The 0.2.5
feedback below is resolved. See [finish art QA](FINISH_ART_DEVICE_QA.md).

## Cash and vault update — 2026-09-26

Imported cash and all four finish vaults are connected to gameplay; the same
vault prefabs appear in the shop preview. Seven PlayMode scenarios pass, including
all four door hinges and cash triggers. Android 0.2.5 / code 7 passes verification
and is installed with all four save files unchanged. Loaded menu and logs pass;
manual device finish feedback is pending. See [finish art QA](FINISH_ART_DEVICE_QA.md).
The 0.2.4 APK is archived.

## Pickup art update — 2026-09-26

Seven new original models and the existing coin are imported; keys, coins and
all five powerups now use them. Six PlayMode scenarios pass. Android 0.2.4 / code 6
passes APK verification and is installed on the phone with all four save files
unchanged. Loaded menu rendering and logs pass. The user reported normal pickup
visibility and behavior in the manual run. See [pickup QA](PICKUP_ART_DEVICE_QA.md). Earlier APK sections below
describe archived baselines.

## Runner pilot update — 2026-09-25

Imported base-runner visual integrated for classic outfit and supported victories;
other cosmetics retain procedural art. All five PlayMode scenarios pass, including
new import/fallback checks and SkinnedMeshRenderer finish framing. Android 0.2.3 /
code 5 builds and passes signature/ARM64/16 KB alignment verification. Installed on Xiaomi 2310FPCA4G with all four save/archive files unchanged.
Cold launch, imported-model rendering and game logs pass. The user selected the
pilot cosmetics and reported normal controls/animation in the manual device check. See [pilot QA](RUNNER_PILOT_DEVICE_QA.md) for artifact hashes and limits.
The older baseline below describes 0.2.2, not the new APK.

Verified on 2026-09-18 in the original Unity project.

## Unity And Logic

- Unity 6000.3.23f1 compiles the project and builds Android without C# errors.
- 45 EditMode tests pass, including V2 money through 10^400, fair probability,
  migration, save repair, retry bankroll, rewards, generated content and locales.
- Four PlayMode scenarios pass using isolated saves and actual runner prefabs.
  They cover physical movement/drag/pickup/gates/obstacles/finishes, first 20 level
  finishes, nine worlds, bonus, streamed Endless and resume, risk persistence,
  background pause, cosmetic purchase, settings and one-time mock rewards.
  Finish framing checks both extreme lanes at 540x960 and 540x1200, projecting
  every character/vault renderer bound to verify it fits outside the result UI.
- Screenshots cover 540x960, 540x1170 and 540x1200, with localized menu, settings,
  shop and gate frames for en/ru/tr/de/ar/zh. TMP truncation and nonblank frame
  checks pass. Selected runner, shop, Arabic, environment and finish frames were
  inspected manually, and visible overlap issues were corrected.
- 43 standalone V1 compatibility checks and 10,000 legacy simulations still pass.
  These simulations are V1 regressions, not V2 economy certification.
- Saved Boot/MainMenu/Game scenes, Stage/Player/gate/cash/track/vault prefabs,
  shared materials, particle material, Balance asset and TMP fallback assets exist.
- Runtime source contains no unfinished TODO or NotImplementedException branches.

## Verified V2 Android Artifact

- Path: Builds/Android/StealAMillion-V2.apk
- Package: com.sulik.stealamillion
- Version: 0.2.2, version code 4
- Development APK, ARM64 IL2CPP, OpenGL ES 3, portrait activity
- Minimum Android API 26; target/compile API 36
- Size: 45,074,819 bytes
- SHA256: 8492C55E74C252B75E3493A023E953C35E044567DBBBEB6B474768C94B6A51FA
- APK Signature Scheme v2 verification passes.
- ZIP 16 KB alignment passes.
- ELF LOAD segments in all six bundled ARM64 libraries have at least 16 KB alignment.
- Manifest contains no fine/coarse location permission.
- Signer SHA256 matches the preserved V1 APK:
  9b24983d51efddd997b25eebc95c803bb97245a7b7cdaae50ba0983c5d818fb8
- Reproduce verification with Tools/Verify-Android.ps1.
- Build log: Logs/android-build.log. Test logs/XML/screenshots: TestResults.

This is a test APK, not a signed Google Play release. A matching package/signer and
higher version code permit an update over this project's V1 APK. In-place update
and V1 migration were verified on the connected Android 15 phone.

## Real Device Check

- Xiaomi 2310FPCA4G, Android 15, ARM64, 720x1600 portrait.
- Installed over V1 0.1.0 using adb install -r, without uninstalling/clearing data.
- Initial 0.2.0 device run exposed a V1 deserialization/migration bug, missing UI
  shader and stripped CapsuleCollider. All three were fixed in 0.2.1, followed by
  passing EditMode/PlayMode suites, APK rebuild and repeat device installation.
- Verified outer save version 2, migrated=true, money $0 (the exact V1 value),
  highest $1,016,300, level 3, 25 coins, gold/diamond ownership, equipped diamond
  vault and all three enabled sound/music/haptics preferences.
- Original .v1 and .v1.bak archives match the pre-install files byte-for-byte.
- Russian was selected automatically. Menu renders with correct materials and
  no sampled missing-shader magenta; launch log has no Unity warnings/errors.
- Manual device play completed levels 3-7, including cash, SAFE/RISK, an obstacle,
  a power-up and $1K milestone. Gameplay log has no Unity errors; running and
  finish frames were inspected. The finish frame exposed vault edge clipping,
  addressed by a portrait finish-framing regression test.
- Force-stop/relaunch of an active level-8 run preserved $1,635, 263 coins, XP 622,
  exact saved X/Z and consumed IDs. The menu offers Continue.
- Installed 0.2.2 after the finish-framing fix. Save and backup are byte-identical
  before/after this update; menu pixels and launch log remain clean.
- Local evidence and exact backup hashes: Docs/DEVICE_QA.md and TestResults/Device.
- Device blocks ADB input injection with INJECT_EVENTS SecurityException. Touch,
  swipe and Android Back testing require manual interaction; no security bypass
  or global device setting changes were attempted.

## Preserved V1

- Git backup: 7461e4be0ca42ba8c744f7b16b26b86c72f20cb0.
- Builds/Android/StealAMillion.apk remains unchanged at 30,144,057 bytes.
- V1 APK SHA256:
  3CBBFDADA30746BB97FAD1F34E813638CCF5ED5807833DE9502B83F2D329DF13.
- V2 edits remain in the existing working tree; no V2 commit or push was made.

## Remaining External Checks

- Touch feel, other cutouts, audio/haptics, interruption during risk/milestones and
  sustained performance remain unverified on hardware.
- Target frame rate is 60 FPS; no measured phone frame-rate claim is made.
- Release AAB signing is blocked on the owner's upload keystore/passwords.
  androidUseCustomKeystore is currently false; the release build command rejects
  this state rather than silently producing a debug-signed store bundle.
- Mock rewarded ads are clearly labeled and offline. No live ad account is wired.
- Native-speaker review and the other 13 target language translations are pending;
  those languages use English fallback.
- Late-game balance and long-session device profiling need human/device playtests.

See QA.md for device checks and V2_REPORT.md for the full implementation report.
