# Runner pilot integration — 2026-09-25

Build target: Android 0.2.3 / version code 5.

The imported runner is selected for character `runner`, outfit `outfit_classic`,
and victories `jump`, `dance`, or `money_rain`. Other combinations use the existing
procedural character. Shop previews of recolored outfits or unsupported victories
also use procedural art. No purchased content, saved selection or progress is reset.

`ImportedRunnerVisual` owns animation and bone-following wealth accessories only.
Root motion is disabled. The existing RunnerController, collider, economy and save
formats are unchanged. Idle/run/celebration use the pilot Humanoid clips; lean and
stumble are applied to the visual wrapper. The older Body hierarchy is inactive
while imported art is active, so two animation systems do not drive the same bones.

## Editor validation

All five PlayMode scenarios passed (`TestResults/unity-visual.xml`), including the
new import/fallback/movement scenario. Finish framing now checks SkinnedMeshRenderer
bounds as well as static renderers on both edge lanes at two portrait aspect ratios.
Reviewed `TestResults/Screenshots/v2-imported-run.png` and finish screenshots.

## Device evidence

Pre-install save backup: `TestResults/Device/20260925-071608-870-Backup`.
The progress and backup both have SHA256
`4BCB1C937F20AF35A657326C05EE8C7667592ED7E8B710E6F265C4033F41C5DE`.
They contain an active level-22 run; equipped businesswoman/coral/backflip will
continue to use procedural graphics. Select base runner/classic/jump through the
shop to inspect the imported pilot; do not rewrite the save to force this selection.

## APK verification

Built successfully: `Builds/Android/StealAMillion-V2.apk`, 47,445,877 bytes.
SHA256: `14F7EFA98DD98B74826874912915A6746E0FF3E8476764FC5628C16CC746FFCA`.
Package/version: `com.sulik.stealamillion`, 0.2.3 / code 5. Signature v2 passes,
signer matches 0.2.2, ZIP and all six ARM64 native libraries pass 16 KB alignment.
No location permission. Previous APK preserved as
`Builds/Android/Archive/StealAMillion-V2-0.2.2.apk`.

## Installed-device validation

0.2.3 / code 5 installed with `adb install -r` on Xiaomi 2310FPCA4G,
Android 15, ARM64, 720x1600. Restarting ADB outside the sandbox restored access;
subsequent device operations also ran outside the sandbox.

Fresh pre-install backup: `TestResults/Device/20260925-184659-663-Backup`.
Post-install, before launch: `TestResults/Device/20260925-184739-798-Backup`.
All four progress/archive files match byte-for-byte across the update.
Cold launch succeeded. Main-menu screenshot:
`TestResults/Device/20260925-184815-469-Capture/screen.png`.
The screen is nonblank, has zero sampled error-magenta pixels, and retains the
existing cosmetics and Continue option for level 22. Game log has zero matching
Unity errors/warnings: `TestResults/Device/20260925-184819-021-Logs/logcat.txt`.

The user selected runner/classic/jump. The imported model and wealth accessories
render on the device: `TestResults/Device/20260925-185115-844-Capture/screen.png`.
The frame has zero sampled error-magenta pixels. Post-selection log still has zero
matching Unity errors/warnings: `TestResults/Device/20260925-185155-237-Logs/logcat.txt`.
A single memory sample reports total PSS 388,593 KB and RSS 523,848 KB; this is not
an FPS measurement or a sustained-performance claim. The user completed the
requested manual check and reported that controls and animation were normal.
The minimum phone pilot check is complete; full motion polish, extended profiling
and the remaining asset catalog are still outstanding.