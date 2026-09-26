# Project State

## Active direction — audit and quality rework, 2026-09-26

User rejected models, worlds, remaining skins and obstacles after 0.3.0.
Current game is a functional prototype, not an art-approved major release.
Treat current art as draft; earlier technical passes do not certify visual quality.
Audit complete: Docs/GAME_AUDIT_2026-09-26.md. Active workflow/backlog:
Docs/NEXT_WORK_PLAN.md. Next: style sheet and coherent Streets/Runner playable
reference, then batches of 3–4 related elements and combined device review.
No new paid tasks, assets, gameplay changes or APK during this audit.
Do not call the game store-ready. Historical implementation state follows.

## Latest major update — 2026-09-26

0.3.0 / code 12 supersedes the historical state below. User authorized a full
major update, existing Meshy credits, Blender production and one combined review.
Nine new models, an animated female runner and costumes, menu/HUD/results/settings,
onboarding, original music and local performance capture are integrated.
10/10 PlayMode and 45/45 EditMode tests pass. Release-mode APK verification passes:
API 36, ARM64, non-debuggable, ZIP and six native libraries aligned for 16 KB.
Installed on the phone with all four save files unchanged. Manual review pending.
Local Android debug signing permits updating the previous phone installation;
this APK is not the Google Play upload artifact. Cost 300 credits; balance 752.
See Docs/MAJOR_DEVICE_QA.md and Docs/GOOGLE_PLAY_RELEASE.md for remaining checks.
Owner account, upload signing, publisher/contact, hosted privacy policy and
generation-time Meshy plan remain unconfirmed. No store upload was performed.

## Project

- Name: STEAL A MILLION V2 (existing Unity project).
- Unity version: 6000.3.23f1.
- Target platform: Android ARM64, portrait, Built-in render pipeline.
- Package identifier: com.sulik.stealamillion.
- Active branch: main; existing V2 work is uncommitted. V1 backup remains intact.
- Framework: Get-Game-Done_Android at 0369d1b; its own PROJECT_STATE describes the framework, not this game.
- Product specification: version 2.md.

## Current Pipeline Status

- User preference 2026-09-26: produce/integrate batches of 3–4 related elements;
  current request covers UI/buttons, gates and a new logo. Existing Meshy credits
  and Blender use are authorized; no subscription/credit purchases requested.

- Current stage: 05 Art Direction & Asset Acquisition.
- Stage status: in_progress.
- Current lead agent: Orchestrator.
- Last completed stage: not retrospectively certified; existing implementation and validation are recorded in Docs/STATUS.md.
- Approved scope: create the models and UI specified in AssetProduction, requested 2026-09-24; proceed through the documented pilot and QA stages. Existing authorization recorded in the production journal permits up to 1100 existing Meshy credits; no credit purchases or subscriptions.
- Follow-up authorized 2026-09-25: integrate the base-runner visual and perform Android/device QA. No gameplay rules, economy, save format, monetization or render-pipeline changes.
- Large batches, purchases, paid-service activation and uncertain-license imports require explicit approval.

## Current Playable State

Latest implementation: 0.2.9 / code 11. User requested consecutive script work
and one combined device check after all four elements. Ten obstacle models,
nine world buildings, differentiated pooled effects and runtime/rendering
optimizations are complete. Import and all nine PlayMode scenarios pass.
APK checks pass; installed with four saves unchanged. Device launch/screenshot/logs
pass. One combined manual run review is pending. See Docs/PLAYABILITY_DEVICE_QA.md.
Older entries below are historical.

Latest Android: 0.2.8 / code 10, installed 2026-09-26. Four differentiated gate
styles (SAFE/RISK/JACKPOT/INVESTMENT) use Meshy crests optimized in Blender to
1400 triangles each. Actual spend: 80 existing credits. Launcher icon uses our
logo with default/adaptive/round/legacy resources verified inside APK. Eight
PlayMode scenarios and APK checks pass. Four saves match across update; loaded
menu and logs pass. The user confirmed normal launcher icon, gate readability
and run smoothness in the manual check. Minimum batch device QA is complete.
Evidence: Docs/GATE_STYLES_DEVICE_QA.md. Prior versions below are historical.

Current APK: Android 0.2.7 / code 9, installed 2026-09-26. UI/buttons, action icons,
imported gate frame and new logo shipped as one four-element batch. Eight PlayMode
scenarios, six-locale screenshots and APK checks pass. Four saves match across
installation; loaded device menu/new UI and log pass. The user reported normal
menus/gameplay and requested more varied gate art. Next batch: SAFE, RISK,
JACKPOT and INVESTMENT variants with distinct forms/symbols and readable values.
Evidence: Docs/INTERFACE_ART_DEVICE_QA.md. Continue in batches of
3–4 related elements per the user's preference. Previous versions below are
historical; full catalog and sustained performance remain in progress.

Latest APK: Android 0.2.6 / code 8, installed 2026-09-26. Imported vault door opens
outward after the user identified reversed opening in 0.2.5. Seven PlayMode
scenarios pass. All four saves matched across update, device finish screenshot
and logs pass, and the user confirmed correct opening. Minimum batch device QA
is complete. Evidence: Docs/FINISH_ART_DEVICE_QA.md. Earlier versions below are
historical; full art catalog and sustained performance remain in progress.

Current APK: Android 0.2.5 / code 7, installed 2026-09-26. Imported cash and all
four vaults now appear in gameplay and vault shop previews. Seven PlayMode
scenarios and APK verification pass. Four saves are unchanged across installation;
loaded menu/new cash rendering and logs pass. Manual finish feedback is pending.
Evidence: Docs/FINISH_ART_DEVICE_QA.md. The 0.2.4 update below is the prior batch.

Latest build: Android 0.2.4 / code 6, installed 2026-09-26. Seven new original
pickup models plus the existing coin are imported. Keys, coins and five powerups
use the new art; gold bar is exported only. Six PlayMode scenarios and APK checks
pass. All four device saves remain unchanged across the update. Cold launch,
loaded menu and logs pass; the user reported normal pickup visibility and behavior
in the manual run on 2026-09-26. Evidence:
Docs/PICKUP_ART_DEVICE_QA.md. The complete art catalog remains in progress.

Boot/MainMenu/Game contain the existing 3D runner. Auto-run, swipe, physical cash/gates/obstacles, finish, progression, cosmetics, missions, worlds, bonus and Endless exist. Current art is mostly original procedural primitives, shared materials, TMP, particles and procedural body animation.

## Systems Implemented

- Core loop: RunnerWorld, RunnerController, GameManager, RunnerSession.
- Progression/economy: BigMoney, 60 routes, procedural levels, 9 worlds, 38 cosmetics, XP/coins/missions.
- Save/load: checksummed V2 saves, V1 archives, migration and active-run recovery.
- UI: uGUI/TMP, localized runtime views and safe area.
- Localization: six full tables, English fallback, RTL and font fallbacks.
- Audio/haptics: local synthesized cues and Android View feedback.
- Analytics/monetization: local analytics and explicitly labeled mock rewarded ads; no live SDK.

## Validation Baseline

- Date: 2026-09-18.
- APK: 0.2.2 / code 4, installed on Xiaomi 2310FPCA4G, Android 15.
- Previously passed: 45 EditMode, 4 PlayMode, 43 legacy checks; manual levels 3-7, migration, active-run recovery.
- See Docs/STATUS.md and Docs/DEVICE_QA.md. These are baseline results, not tests of future imported art.

## Known Risks / Blockers

- Imported skins cannot directly replace procedural Body/Arm/Leg hierarchy without an isolated visual adapter.
- Generated models need topology, scale, materials, rig and licensing review before production import.
- Meshy CLI 0.4.0 installed with Node 24.19.0; browser login and verified API access confirmed 2026-09-24 (balance at verification: 1100 credits).
- Recorded pilot cost: 38 credits (15 model + 5 rig + six animations at 3 each); failed backflip reports 0. No paid tasks were resubmitted during continuation.
- All 15 local FBX models pass isolated Unity scale/hierarchy checks, including positive-Z track direction and moving vault doors. Art pilot package contains 38 VFX presets and 10 UI motion templates; integration/device QA is still pending.
- Release AAB still requires the owner's upload signing configuration.

## Next Approved Action

Complete pilot export/QA, then continue the remaining asset catalog. Runner geometry, rig, bundled run and six additional animation GLBs are downloaded in AssetProduction/02_Generated/CHAR_runner. The runner Unity pilot was exported on 2026-09-25: seven adapted clips pass isolated Humanoid import, scale, sampled mesh bounds and root-drift checks. Package: AssetProduction/04_Exports/SAM_RunnerPilot__v001.unitypackage. Evidence and remaining motion-quality limits: AssetProduction/Reviews/Runner/REVIEW_2026-09-25.md. Full moving-clip fidelity/loop/foot-sliding review, wireframe, accessories, outfit masks and gameplay integration remain pending. Backflip failed remotely and has not been resubmitted. See AssetProduction/PRODUCTION_RUN_2026-09-24.md for the latest QA evidence. The base runner now uses an isolated imported visual adapter for classic outfit and jump/dance/money_rain victories; other combinations retain procedural visuals. Five PlayMode scenarios pass. Android 0.2.3/code 5 is installed on Xiaomi 2310FPCA4G. All four save/archive files match before and after installation. Cold launch and existing-cosmetic menu render pass with no sampled missing-shader magenta and no matching Unity errors/warnings. ADB requires restarting and operating outside the sandbox. The user selected runner/classic/jump; the imported model and wealth accessories render correctly on hardware, the post-selection Unity log is clean, and the user reported no movement/animation problems in the requested manual check. Minimum pilot device QA is complete. Next: continue the remaining asset catalog and motion polish; sustained device profiling remains pending. See Docs/RUNNER_PILOT_DEVICE_QA.md.
