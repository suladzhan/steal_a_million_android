# MVP QA

Unchecked items are pending, not claimed as passed.

## Automated

- [x] Run `Tools/Test-Core.ps1`: 43 checks, 10,000 simulations.
- [x] Unity import completes with no compiler errors.
- [x] All 10 Unity EditMode tests pass, including JSON and Resources integration.
- [x] PlayMode integration scenario passes with isolated test saves.

## Portrait UI

Test 540x960, 540x1170 and 540x1200, plus a device with a notch.

- [x] Main menu, game, settings and shop pass truncation checks at all three sizes;
  terminal screens pass at 540x1200.
- [x] Current money and both choices remain visible without scrolling.
- [ ] Icons and purchase controls are easy to tap; safe area is respected.
- [x] Vault graphic, font atlas and confetti render correctly in captured Unity frames.
- [x] Large amounts such as $1,000,000 fit their containers.

## Gameplay

- [x] New run begins at $100; SAFE shows $200 and advances once.
- [x] Simultaneous/repeated SAFE/RISK clicks resolve only one decision.
- [x] RISK displays suspense, then the correct result and changed total.
- [x] Continue on a result skips the delay and number animation.
- [x] Every third decision advances the level and grants 25 coins once.
- [x] A zero total shows Game Over with the run's peak, not an automatic restart.
- [x] Test second chance restores $500 once per run and cannot be double-claimed.
- [x] Reaching the target celebrates and grants 250 coins once.
- [x] Core NewRun preserves cosmetics and permanent coins.

## Persistence and lifecycle

- [ ] Close/reopen during a decision: money and decision are restored.
- [ ] Close/reopen during suspense: same committed outcome, no reroll.
- [ ] Close/reopen on victory: no duplicate coin reward.
- [ ] Background during suspense/result/mock ad: flow pauses, then resumes.
- [ ] Back/Escape pauses gameplay; Resume and Main Menu both work.
- [x] Return from pause to Main Menu during a result, then Continue: result resolves once.
- [ ] New Game and Reset Progress require confirmation; Cancel preserves data.
- [ ] Reset clears both save generations, cosmetics, coins and settings.
- [ ] Sound, music and haptics toggles persist through restart.
- [x] Cosmetic purchase/equip and sound setting persist to disk in Unity.

## Android release

- [x] Development ARM64 APK builds; manifest, signature and zip alignment pass.
- [ ] APK installs and launches on a physical ARM64 device.
- [ ] Airplane-mode play works after installation.
- [ ] Android logcat contains no uncaught exceptions.
- [ ] Frame timing stays near the configured 60 FPS target on a mid-range phone.
- [ ] Back, touch, focus loss, audio and haptics work on the device.
- [ ] Signed release AAB builds with development/debug UI disabled.
- [ ] Verify adaptive icon appearance and store packaging before distribution.
