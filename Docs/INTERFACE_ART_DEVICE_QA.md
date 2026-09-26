# UI, gates and logo — 2026-09-26

Batch requested by user: work in groups of 3–4 elements. This update includes
rounded beveled buttons, action icons, imported modular gates and a new original
title logo. Existing localized text, click handlers, button states, gate colors,
probabilities, trigger sizes, gameplay and saves are retained.

Eight PlayMode scenarios pass, including all gate mappings and physical gameplay.
Screens cover six locales and multiple phone proportions; English menu, Arabic
shop and English gate frames were inspected. Logo alpha is preserved (1942×809).
Logo source and exact prompt: `AssetProduction/04_Exports/Brand/Logo-v001-prompt.md`.
Generated via built-in image_gen. Meshy credits consumed for this batch: 0.

Android 0.2.7 / code 9 verification passes: matching signature v2, ARM64,
ZIP and six native libraries aligned for 16 KB pages. Size: 51,165,197 bytes.
SHA256: `3C47C05A760FB49E2841651585A338FDA370D9C4944024E0F3C76F184A4D1413`.
0.2.6 retained in `Builds/Android/Archive`.

Installed with `adb install -r` on Xiaomi 2310FPCA4G, Android 15. Four save files
matched byte-for-byte before/after installation, before launch.

Evidence under `TestResults/Device`:

- Before: `20260926-123915-411-Backup`.
- After: `20260926-123952-416-Backup`.
- Menu: `20260926-124006-786-Capture/screen.png` (inspected).
- Log: `20260926-124008-579-Logs/logcat.txt` (zero detected game issues).

Menu logo/buttons/icons render correctly, zero sampled error-magenta pixels.
Loaded progress: level 27, 1147 coins, $17.5K. Save and backup SHA256:
`6798C26A19133B0308C2DB898B1EFA95F9B417EABCA3D911E7E5DC7569114FFE`.
V1 archives remain
`4342CC6614AB7C9094CD5A6C7BEB9C9B34F190650893711C3993CB51BF60F62D`.
Logo SHA256: `C6151A19C0364685C1A3C41B71C616A12E9EB011DEFC542323D67005581C192F`.

The user checked menus and a run on 2026-09-26 and reported normal behavior.
Art feedback: gates are too simple and should be diversified. Functional device
QA passes; richer gate art remains follow-up work. Next batch: four distinct
SAFE/RISK/JACKPOT/INVESTMENT forms and symbols, preserving readable values.
Sustained performance and complete
manual inspection of every screen remain outside this batch check.
