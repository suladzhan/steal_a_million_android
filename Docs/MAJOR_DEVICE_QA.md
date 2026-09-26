# Major update 0.3.0 / code 12 — QA

2026-09-26: implementation and installation complete. Combined manual review
and measured gameplay performance pending. No Google Play upload.

## Automated checks

- TestResults/unity-visual.xml: 10 passed, 0 failed, 58.908 seconds.
- TestResults/unity-editmode.xml: 45 passed, 0 failed, 0.371 seconds.
- Coverage includes imported cosmetics, female Humanoid locomotion/body facing,
  movement ownership, nine worlds, major obstacles, six locales/RTL,
  HUD/results/privacy/onboarding, collision/save/finish behavior.
- Logs/android-build.log: non-development build succeeded, batch exit 0.
  Build settings restoration reloads RunnerConfig after BuildPlayer, which can
  invalidate the original asset reference. Source mockAds restored to 1;
  release player built with mock ads disabled.
- Tools/Verify-Android.ps1 -Release: signature, manifest, ZIP alignment and
  16 KB ELF alignment for all six ARM64 native libraries passed.

## APK

- Builds/Android/StealAMillion-V2.apk; archive:
  Builds/Android/Archive/StealAMillion-V2-0.3.0.apk.
- 48,042,298 bytes.
- SHA256: E53D4D5CEE985D6E7E5B6BF8C095BDC2E88C956CA57EE70A060DDF9F382AEDE2.
- com.sulik.stealamillion; min API 26, target API 36, ARM64, non-debuggable.
- No Internet or location permission in inspected manifest.
- Existing Android Debug certificate allows local update testing. This APK
  is not the upload-signed Google Play AAB; owner upload signing remains needed.

## Phone

Xiaomi 2310FPCA4G, Android 15, 720×1600, serial 5XSKCIUC8HR8RCM7.
adb install -r succeeded without uninstalling. Package reports 0.3.0/code 12.
Backups before/after installation:

- TestResults/Device/20260926-194717-964-Backup
- TestResults/Device/20260926-194726-772-Backup

All four files match SHA256:

| Files | SHA256 |
| --- | --- |
| progress.sav, progress.sav.bak | 877C5CBFD001006A74B0F8660A283C2AFEFBD9FC8A31B62E064DE2C8CA554A55 |
| progress.sav.v1, progress.sav.v1.bak | 4342CC6614AB7C9094CD5A6C7BEB9C9B34F190650893711C3993CB51BF60F62D |

Launch succeeded. Inspected actual menu:
TestResults/Device/20260926-194816-281-Capture/screen.png.
6,900 sampled colors, zero error-magenta fraction. Visible progress: world level
33, player level 8, $12.7K and 1,275 coins. Game logs:
TestResults/Device/20260926-194818-732-Logs/logcat.txt — zero matched game errors.

Local performance capture enabled before cold launch. After a complete run:
Tools/Device-Game.ps1 -Serial 5XSKCIUC8HR8RCM7 -Action Performance.
No measured 60 FPS claim until the report is pulled.

## Combined manual review pending

Shop/costume/female if unlocked, settings/sound, then a full run with lane swipes,
bonuses, gates, obstacles and finish. Check readability, animation, controls and
stutter; leave game open for metrics. ADB touch injection is blocked by this
phone. Publication prerequisites: GOOGLE_PLAY_RELEASE.md.
