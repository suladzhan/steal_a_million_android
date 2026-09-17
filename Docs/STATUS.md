# Development status

## Verified locally

- The production Core C# files compile using the Windows .NET Framework compiler.
- 43 standalone checks pass, covering money bounds, formatting, production data,
  duplicate input, risk thresholds, terminal states, progression, permanent
  rewards, second chance, shop purchases and disk-save recovery.
- 10,000 seeded simulated runs with a 50/50 SAFE/RISK strategy terminate without
  invalid money or decision indices: 1,714 victories and 8,286 losses. This is a
  diagnostic strategy, not a target player win rate.
- All SAFE reaches exactly $1,000,000 after 30 decisions.

## Verified in Unity 6000.3.23f1

- Project compilation, TMP resource import, scene generation and Android setup.
- All 10 EditMode tests pass, including Unity JSON and production Resources.
- The PlayMode integration scenario passes: SAFE/RISK, duplicate input, result
  continuation, pause/menu/continue, background input blocking, loss, mock second
  chance, all 30 safe decisions, victory, cosmetic purchase and settings persistence.
- 15 rendered screenshots cover 540x960, 540x1170 and 540x1200, plus suspense,
  Game Over and victory. Checks reject truncated text, missing renderers and blank
  frames. Main menu, gameplay, shop, settings, Game Over and victory screenshots
  were also inspected directly. The scenario also passes with Android selected
  as the active build target.
- Fixed first-import scene creation, responsive stage sizing and procedural
  graphics rendering using the actual Editor runs.

## Verified Android artifact

- Development APK built successfully: `Builds/Android/StealAMillion.apk`,
  v0.1.0 (version code 1), 30,144,057 bytes, ARM64/IL2CPP, min API 26, target API 36.
- APK manifest inspection, v2 signature verification and 16 KB zip alignment pass.
- APK SHA256: `3CBBFDADA30746BB97FAD1F34E813638CCF5ED5807833DE9502B83F2D329DF13`.

## Blockers and remaining checks

- ADB found no connected Android device. Touch, system Back, notch handling,
  audio/haptics and app restart/background behavior need physical-device testing.
- 60 FPS is configured as a target; device performance has not been measured.
- Release signing requires the owner's upload keystore.

## Next work

1. Install the verified development APK on an ARM64 Android phone.
2. Complete the physical-device checks in QA.md and inspect Android logs/performance.
3. Configure the owner's upload key before generating the release AAB.
4. Playtest the decision balance, especially full-total losses and repeat level 10.
