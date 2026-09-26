# STEAL A MILLION V2

Portrait, offline 3D runner in the original Unity project. All bankroll money and
cosmetic coins are fictional. There are no deposits, withdrawals, real prizes,
payments, accounts, GPS requests, or network-dependent gameplay.

The authoritative V2 specification supplied in this workspace is `version 2.md`.
The requested name `STEAL_A_MILLION_V2_MASTER.md` was not present. V1 remains in Git
at `7461e4be0ca42ba8c744f7b16b26b86c72f20cb0`; no second Unity project was created.

## Play

1. Open this folder in Unity **6000.3.23f1**, with Android SDK/NDK/OpenJDK modules.
2. Open `Assets/Scenes/Boot.unity` and press Play.
3. Use portrait Game view, preferably 540x960 or 540x1200.
4. Press Play, then drag horizontally with a mouse or swipe on Android. The runner
   advances automatically. Escape / Android Back pauses.

Saved scenes contain the stage prefab, player, camera, light, Canvas and manager.
The manager fills the modular route and localized Canvas views at runtime.
Boot, MainMenu and Game all open the same runner flow; V1 cards are not used.

## Game

- 60 authored route definitions, including 20 introductory handcrafted sequences;
  deterministic procedural progression after level 60 and streamed Endless.
- Physical SAFE/RISK gates with shown chances, committed outcomes, suspense and
  effects. Investment, business, market, jackpot, insurance, shields, luck,
  double-or-nothing, mystery and provisional risk chains.
- Tax, police, thieves, moving barriers, rotating hazards, traffic, walls, signs
  and closing gates. Split bridges, keys, bonus vault runs and five power-ups.
- Integer arbitrary-precision bankroll: K, M, B, T and larger scientific values.
  One million is a milestone, never a cap or final victory.
- Slow early cash growth, late percentage SAFE rewards, 12 ranks, milestones,
  player XP, separate cosmetic coins, nine unlockable environments.
- 38 cosmetic items: 10 characters, 3 outfits, 6 trails, 6 money effects,
  6 celebrations, 4 vaults and 3 gate effects. Preview, requirements, buy and equip.
- 36 missions, 3 daily objectives, 24 achievements, statistics and collections.
- Six complete string tables: English, Russian, Turkish, German, Arabic and
  Simplified Chinese. Other selectable languages fall back to English offline.
  Automatic device language, manual override, Arabic shaping and mirrored controls,
  and TMP fallback fonts for Cyrillic, Turkish, Arabic, CJK, Thai and Devanagari.
- Checksummed local saves, backup recovery and V1 migration with V1 archive copies.
  Old money, coins, settings and vault purchases/equipment are preserved.

See [V2 implementation report](Docs/V2_REPORT.md), [verified status](Docs/STATUS.md)
and [device QA checklist](Docs/QA.md). Automated tests are not phone certification.

## Configuration

- `Assets/Resources/Runner/Balance.asset`: starting money, cash growth, SAFE
  rewards, risk tiers, investments, losses, rewards, touch and animation timing.
- `Assets/Resources/RunnerData/levels.json`: 60 routes, seeds, segment order, speed.
- `gates.json`, `worlds.json`, `shop.json`, `missions.json` in the same directory.
- `Assets/Resources/Localization/*.json`: string tables with English fallback.
- `Assets/Resources/Runner`: serialized modular prefabs, fonts and shared materials.

Use **Steal A Million > Validate Runner Content** after data changes.
**Build Runner Assets** regenerates the three scenes and runner prefabs in-place;
commit custom scene/prefab edits before running this regeneration command.
**Build Localization Fonts** rebuilds fallback font assets when necessary.

**Steal A Million > Runner Debug** is an Editor-only QA window. It can grant test
wealth, coins and XP, unlock cosmetics/worlds, choose levels, force the next risk
outcome, enter bonus/endless and complete mission counters. It is absent from
Android builds. Forced outcomes are QA-only and never used by production logic.

## Tests

Close the Unity Editor using this project before running batch commands:

```powershell
.\Tools\Test-Core.ps1
.\Tools\Test-Unity.ps1 -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe'
.\Tools\Test-Unity.ps1 -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe' -Visual
```

The standalone suite retains 43 V1 regression checks and 10,000 legacy simulations.
Unity EditMode tests cover V2 economy, save repair/migration, fair probability,
rewards, seeded generation and localization. PlayMode tests use actual physics,
saved prefabs, rendering, purchase/save flows, background pause, risk resume,
bonus runs and endless streaming. Test saves are isolated under `TestResults`.
Screenshots and XML/log reports are written there too.

## Android

Package `com.sulik.stealamillion`, version **0.2.2 / code 4**, portrait,
ARM64 IL2CPP, minimum API 26, OpenGL ES 3.

```powershell
.\Tools\Build-Android.ps1 -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe' -Format Apk
```

Output: `Builds/Android/StealAMillion-V2.apk`. The existing V1 APK is preserved.
The verified development APK is 45.1 MB; its signature and ZIP/ELF 16 KB alignment
pass. In-place installation, migration, manual levels 3-7 and active-run recovery
to the menu were verified on Android 15. Broader device QA and signed release
AAB remain pending; see Docs/DEVICE_QA.md for the exact evidence and limits.
Install using Android SDK `adb install -r Builds/Android/StealAMillion-V2.apk`.
Use `-r` to preserve V1 data for migration testing; do not uninstall first.
Run `Tools/Verify-Android.ps1` to verify the signature, manifest, ZIP alignment and
16 KB ELF load-segment alignment of every bundled ARM64 library.

For a USB-authorized test device, use `Tools/Device-Game.ps1 -Serial <serial>`
with `-Action Info`, `Backup`, `Install`, `Launch`, `Capture`, or `Logs`.
Back up first: the helper only copies the game's save files, never clears data,
and refuses screenshots/touch input when another app is foreground. Captures
include sampled pixel checks for blank frames and missing-shader magenta.
All device evidence is local under `TestResults/Device`, excluded from Git.

For a release bundle, configure your own upload keystore in **Player Settings >
Android > Publishing Settings**, then use `-Format Aab` or **Steal A Million >
Build > Android AAB (Release)**. Output: `Builds/Android/StealAMillion-V2.aab`.
The command refuses to produce a release AAB without a custom keystore.
Never commit keys or passwords.

Rewarded ads are explicitly labeled local test rewards, not a live ad SDK.
There are no interstitials. Set `mockAds=false` in Balance before distribution
unless deliberately testing this mock. Real ad integration needs separate consent,
SDK/account configuration and device QA.

## Sources And Assets

Runner models, tracks, environment geometry, particles and animations are original
procedural Unity content. Fonts and RTL helpers retain their licenses.
See [third-party notices](Docs/THIRD_PARTY.md).
