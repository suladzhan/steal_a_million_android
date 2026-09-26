# V2 QA Checklist

Unchecked items are pending, not claimed as passed.

## Automated Logic

- [x] Legacy standalone suite: 43 checks and 10,000 V1 simulations.
- [x] Unity EditMode: 45 tests.
- [x] BigInteger money round-trip beyond 10^400; no $1M terminal state.
- [x] Probability thresholds and 600,000 random samples match configured chances.
- [x] Risk commit/resume/idempotence, shield/luck, investments and chains.
- [x] Old money/settings/purchases/equipped vault migration.
- [x] Invalid run recovery, endless record retention and consumed-ID retirement.
- [x] Practice cannot farm permanent money/XP/coins.
- [x] Retry seed money agrees with the menu and save before the first pickup.
- [x] Rewards and claims cannot be duplicated; UTC daily rollback is rejected.
- [x] Bonus generation has no loss gates or hazards.
- [x] All curated routes and generated levels through 500 validate.
- [x] Six locale tables cover English keys with valid format placeholders.

## Unity PlayMode

Four scenarios passed after the 0.2.2 changes.

- [x] Physical forward motion, horizontal dragging and cash trigger.
- [x] Physical risk gate, suspense and committed outcome.
- [x] Pause stops motion; return from menu resumes the same risk.
- [x] Background pause freezes gameplay.
- [x] Physical finishes for levels 1-20.
- [x] Obstacle loss, key entry to bonus and continuous endless chunk streaming.
- [x] Endless position/record retained across menu and Continue.
- [x] Shop purchase and language/sound settings persist to disk.
- [x] Mock second chance and one-time double coins.
- [x] No rewarded ad while a milestone is playing.
- [x] Nonblank rendered views and TMP truncation checks at 540x960, 540x1170,
  540x1200, and six localized menu/settings/shop/gate flows.
- [x] Nine environment frames captured; Arabic mirrored shop inspected.
- [x] Finish character/vault fully framed from both edge lanes at 540x960/1200.
- [ ] All 60 complete levels played by a human, including every optional branch.
- [ ] Long late-game playtest of all-SAFE, mixed and aggressive risk strategies.

## Android Device

- [x] ARM64 APK builds; package/version, signature, ZIP and ELF 16 KB alignment pass.
- [x] V2 and preserved V1 APK signing certificates match; no GPS permission.
- [x] Install V2 APK over V1 with adb install -r; verify migration and archive.
- [x] Fix Android-only missing UI shader/CapsuleCollider and repeat clean launch.
- [x] Verify automatic Russian locale and Cyrillic menu on 720x1600 Android 15.
- [x] Manual device play: levels 3-7, cash, SAFE/RISK, obstacle, power-up, $1K.
- [x] Force-stop/relaunch active run: bankroll, coins, XP, X/Z, consumed IDs intact.
- [x] Update to 0.2.2 preserves the exact save bytes; clean launch and menu frame.
- [ ] Launch with airplane mode enabled; play multiple levels and use shop.
- [ ] Swipe controls feel responsive at 30/60/90 Hz without accidental UI drags.
- [ ] Test notches, cutouts, system bars and tall portrait displays.
- [ ] Android Back, focus loss and suspend/resume during risk and test ads.
- [ ] Kill/restart during risk, finish, milestone and purchase; no duplicate payout.
- [ ] Validate Cyrillic, Turkish, Arabic and CJK fonts in the APK.
- [ ] Sound/music toggles and Android vibration on supported devices.
- [ ] Thirty-minute Endless session: bounded memory and no visible chunk gaps.
- [ ] Measure frame time, thermals, memory and battery on a mid-range ARM64 phone.
- [x] Review launch logcat for uncaught exceptions and shader/font errors.
- [x] Repeat log review during initial manual gameplay; no Unity errors.
- [ ] Review all remaining locale/scene flows on hardware.

See DEVICE_QA.md. ADB touch injection is blocked by this phone's security policy;
only the specific manual gameplay and recovery checks listed above have passed.

## Release

- [ ] Configure the owner's upload keystore outside Git.
- [ ] Build and validate signed AAB without development/debug controls.
- [ ] Disable labeled mock rewards or integrate a separately reviewed real ad SDK.
- [ ] Native-speaker review, age/content ratings, store text and privacy disclosures.
- [ ] Upload to Play internal testing and test the delivered split APKs.
