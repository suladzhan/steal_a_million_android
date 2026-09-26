# Cash and vault Android QA — 2026-09-26

Five existing original models are integrated: cash stack and classic/gold/diamond/
neon vaults. Finish and shop preview share the vault prefabs. Imported door
orientation is preserved while opening around the vault's vertical axis.
Gameplay roots, trigger dimensions, IDs, movement and save format remain intact.

- Five imports passed dimensions, triangle budget and hinge pivot checks.
- Seven PlayMode scenarios passed, including all four doors opening 105 degrees,
  cash trigger checks, physical pickup flow and both-edge finish framing.
- All five source previews and the tall-screen open-vault finish were inspected.
- Android 0.2.5 / code 7, ARM64 IL2CPP; 47,665,565 bytes.
- SHA256: `1D304215562E80EEB1681EFA4ADAE920F4F47D00BC0104B39F9D7BC998BEC921`.
- Signature v2 and ZIP/native 16 KB alignment passed. Same signer as 0.2.4.
- Installed over 0.2.4 using `adb install -r` on Xiaomi 2310FPCA4G, Android 15.
- All four save files matched byte-for-byte before/after installation, before launch.
- Cold launch, loaded menu with new cash and zero detected game log issues passed.
- Device menu shows level 25, 1077 coins, $2.25K; zero sampled error-magenta pixels.

Device evidence under `TestResults/Device`:

- Before: `20260926-114750-011-Backup`.
- After: `20260926-114806-896-Backup`.
- Screenshot: `20260926-114822-455-Capture/screen.png`.
- Log: `20260926-114824-882-Logs/logcat.txt`.

Current save and backup SHA256:
`8A26663D4049EB73E3B4D26B17AA547FEB92E6817FDB6E43E3F8CEB397463A70`.
V1 archives remain
`4342CC6614AB7C9094CD5A6C7BEB9C9B34F190650893711C3993CB51BF60F62D`.

Unity package SHA256:
`2CBC6F20545650BE4693C73B22D2421FBD5FA93033D1E5BDEFF6DD232F6A6E3F`.
Rebuild/import: `StealAMillion.Editor.FinishArtSetup.Build`.
0.2.4 is archived in `Builds/Android/Archive`.

Manual device feedback identified reversed door opening in 0.2.5. This build is
not accepted for finish art QA. The correction reverses the signed opening angle
to +105 degrees, retaining the hinge orientation; rebuild/recheck is in progress.
Sustained performance and every vault variant on hardware remain outside the
launch check.

## Door direction correction

0.2.6 / code 8 reverses opening to +105 degrees (outward). Seven PlayMode
scenarios pass after the correction, including the signed direction for all four
variants and both-edge finish framing. The new open-vault screenshot was inspected.
APK verification passes: ARM64, matching signature v2, ZIP/native 16 KB alignment.
Size: 47,665,577 bytes. SHA256:
`3556F6AA895C2D80EBE52C862E6B1585A3174E3D8D0BF26B12CCDACB7240919D`.
Installed over 0.2.5; all four save files were byte-identical before/after update.
Before backup: `20260926-122435-101-Backup`; after: `20260926-122505-979-Backup`.
Current save and backup SHA256:
`F7211CEA31ED65C5D150DF32791531DE952240489BD48989DC1567DE2C625934`.
V1 archive hashes remain unchanged. The user completed a run and confirmed
"Теперь правильно" on 2026-09-26. Device finish screenshot
`20260926-122604-363-Capture/screen.png` was inspected: door opens outward,
cash interior visible, buttons unobstructed, zero sampled error-magenta pixels.
Log `20260926-122606-535-Logs/logcat.txt` contains zero detected game issues.
Minimum device finish recheck is complete. All four variants and sustained
performance have not been individually certified on hardware.
