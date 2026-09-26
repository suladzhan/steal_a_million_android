# Gate styles and launcher icon — 2026-09-26

Four Meshy crests and four decorative frame styles are integrated: SAFE, RISK,
JACKPOT and INVESTMENT. Blender mobile LOD: 1400 triangles per crest, height
0.9 m, thickness 0.18 m. Shapes sit above dynamic labels; original trigger size,
colors, gate values, probabilities and gameplay rules remain intact.
Originals, task IDs, exact prompts, previews, costs and lineage:
`AssetProduction/Reviews/Gates/REVIEW_2026-09-26.md`.
Actual Meshy cost: 80 credits (four × 20).

The launcher uses our logo. Image_gen source:
`Assets/Art/Brand/AppIcon-v001.png`; platform layers in the same directory.
Default/adaptive/round/legacy icon slots are configured. APK contains icon PNGs
at six densities and adaptive XML/foreground/background layers. Extracted
`TestResults/Icons/apk-icon-0.2.8.png` was visually inspected.

- Eight PlayMode scenarios passed, including crest mappings, label clearance,
  semantic colors, original triggers and physical gameplay.
- Combined four-style preview and live English gates were inspected.
- Android 0.2.8 / code 10, ARM64, 51,695,657 bytes.
- SHA256: `5DE02C1348A129986995B057756F049801E556E48B20131A322FFD44AA10ABEA`.
- Signature v2, matching signer, ZIP/native 16 KB alignment pass.
- Installed over 0.2.7 on Xiaomi 2310FPCA4G, Android 15.
- Four saves matched byte-for-byte before/after installation, before launch.
- Cold launch and loaded menu pass, zero sampled error-magenta pixels.
- Game log contains zero detected errors/warnings.

Device evidence under `TestResults/Device`:

- Before: `20260926-135914-849-Backup`.
- After: `20260926-135939-626-Backup`.
- Menu: `20260926-135956-222-Capture/screen.png`.
- Log: `20260926-135959-000-Logs/logcat.txt`.

Save and backup SHA256:
`41617A67D6EF16305ADDC3B05F092CBC7466D94B39A43C821301AE35AFDEAD85`.
V1 archives remain
`4342CC6614AB7C9094CD5A6C7BEB9C9B34F190650893711C3993CB51BF60F62D`.

The user checked the launcher icon and played a run on 2026-09-26, reporting
normal icon appearance, gate readability and smoothness. Minimum manual
batch check passes. Sustained FPS and
long-session performance remain unverified. 0.2.7 is archived.
