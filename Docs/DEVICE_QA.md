# Android Device QA - 2026-09-18

## Environment

- Xiaomi 2310FPCA4G, Android 15 / API 35, ARM64, 720x1600.
- Original installed package: com.sulik.stealamillion, 0.1.0 / code 1.
- Current package: same identifier and signing certificate, 0.2.2 / code 4.
- Development IL2CPP APK; no uninstall, data clear, rooted access, global network
  changes or security-setting changes performed.

## Migration Evidence

Before installation, copied only the game's progress.sav and progress.sav.bak to
TestResults/Device/20260918-145738-444-Backup. Both files have SHA256:

`4342CC6614AB7C9094CD5A6C7BEB9C9B34F190650893711C3993CB51BF60F62D`

The initial 0.2.0 APK exposed Unity nested-object deserialization behavior that
caused the migration to return an empty runner too early. It retained all legacy
fields and V1 archive copies. The 0.2.1 fix uses the outer save version and was
tested without restoring/rewriting phone data manually.

After the fixed APK launch, TestResults/Device/20260918-151227-407-Backup contains:

- progress.sav and .bak: version 2, runner.migrated=true.
- Bankroll $0, record $1,016,300, level 3, 25 coins: exact original values.
- Gold/diamond vaults owned; vault_diamond equipped.
- Sound, music, haptics enabled, as in V1.
- progress.sav.v1 and .v1.bak: exact pre-install SHA256 above.
- Worlds through the millionaire island unlocked from the preserved record.

## Rendering And Logs

Initial phone launch exposed pink UI and missing CapsuleCollider errors. The
fixed build bundles the UI material and preserves procedural native components.
Re-ran 45 EditMode tests and 3 PlayMode scenarios before packaging the hotfix.

Corrected menu screenshot:
TestResults/Device/20260918-151220-573-Capture/screen.png.
Pixel sampling: 3,235 distinct sampled colors; error-magenta fraction 0.
Russian selected from the device locale without GPS or manual selection.

Clean launch log:
TestResults/Device/20260918-151239-901-Logs/logcat.txt.
No Unity warnings/errors, FATAL EXCEPTION or fatal native signal in this capture.
Nonfatal vendor debug messages are not treated as game exceptions.

One menu memory snapshot reported total PSS 320,254 KB and RSS 457,256 KB. This is
not a sustained memory, frame-rate, battery or thermal measurement.

## Manual Gameplay And Recovery

The owner played on the phone while game-only evidence was collected. The save
at TestResults/Device/20260918-173745-628-Backup confirms five completed levels
(3-7), 142 cash pickups, 7 SAFE gates, 4 risks (2 wins), 1 obstacle hit, 1 power-up
and the first $1K milestone. Horizontal movement/tutorial flags are set. An earlier
live level-5 frame clearly shows both gates, displayed 60% odds, cash HUD and the
runner; a second frame shows the completed level and opened vault:

- TestResults/Device/20260918-151712-558-Capture/screen.png
- TestResults/Device/20260918-151743-738-Capture/screen.png

Gameplay log TestResults/Device/20260918-151727-374-Logs/logcat.txt has no Unity
errors/warnings. Both captures have zero sampled error-magenta pixels.

A forced stop/relaunch occurred with an active level-8 run, not during risk or
on the result screen. Compared the pre-stop save above with
TestResults/Device/20260918-173822-425-Backup: money $1,635, 263 coins, XP 622,
records, statistics, claims, milestones and owned items are unchanged. The active
run's exact X/Z position and consumed IDs are unchanged; rewardGranted remains
false. The menu offers Continue. This proves recovery to the menu, not yet manual
resumption or interruption during risk. No manual save editing/restoration.

## Final 0.2.2 Update

The phone's level-5 finish frame revealed a clipped right edge of the vault. The
0.2.2 update moves the vault inward, widens the finish camera framing and smoothly
positions the celebrating character. A fourth PlayMode test finishes from both
extreme lanes at 540x960 and 540x1200 and checks every character/vault bounds corner
against the visible area between the result text and buttons. Four PlayMode
scenarios, 45 EditMode tests and the 43 legacy checks pass; selected new finish
screenshots were visually inspected. Final finish appearance on hardware still
needs a new manual finish after this update.

Installed 0.2.2 in-place and launched it. These before/after saves are identical:

- TestResults/Device/20260918-174516-662-Backup/progress.sav
- TestResults/Device/20260918-174600-940-Backup/progress.sav
- SHA256: 8A13A1B490D7F04D0B6E6539E5C914D62259EA70D320999CE12BB25B4DAFE2F1

Final menu: TestResults/Device/20260918-174551-012-Capture/screen.png, 3,162 sampled
colors, zero error-magenta. Final clean launch log:
TestResults/Device/20260918-174553-371-Logs/logcat.txt.

## Pending Interaction Tests

The phone denies ADB input with INJECT_EVENTS SecurityException. Installation,
launch, app-save backup and game-only screenshot/log capture are permitted.
No input-permission bypass was attempted. Manual play is required to verify:

- Subjective swipe feel and manual Continue after process death.
- Back, pause, focus loss, suspend and force-stop during active risk.
- Shop previews, Turkish/Arabic/CJK fonts, audio and haptics on hardware.
- Airplane-mode play, long Endless sessions, memory bounds and sustained FPS.

Tools/Device-Game.ps1 reproduces device checks and restricts capture/input to the
game foreground. Save backups, full logs and screenshots remain local and are
excluded from Git by TestResults/.
