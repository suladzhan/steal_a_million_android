# Pickup art Android QA — 2026-09-26

Seven new original models and the existing coin are integrated. Five powerups,
keys and coins use imported visuals; gold bar remains an exported asset.
Pickup rules, trigger radius, labels, economy and save format are unchanged.

- Six PlayMode scenarios passed, including pickup models, triggers and dynamic labels.
- All eight imports passed the Unity geometry and prefab checks.
- Android 0.2.4, code 6, ARM64 IL2CPP: 47,508,969 bytes.
- APK SHA256: `6B86C579415F20DAE9A9067E70A7E7B5A489928C2E9F82B94CEEC4338AFEC342`.
- Signature v2 and 16 KB alignment verification passed; signer matches 0.2.3.
- Installed with `adb install -r` on Xiaomi 2310FPCA4G, Android 15.
- All four save files were byte-identical before/after installation, before launch.
- Cold launch passed. Loaded menu shows level 24 and the equipped imported runner.
- Loaded screenshot has zero error-magenta samples; game log contains zero detected errors/warnings.

Evidence under `TestResults/Device`:

- Before: `20260926-113430-622-Backup`.
- After: `20260926-113439-669-Backup`.
- Loaded screenshot: `20260926-113522-514-Capture/screen.png`.
- Log: `20260926-113524-611-Logs/logcat.txt`.

Current save and backup SHA256:
`672211B1EB0CCC60CC736FDC0D305D1E641833982182A21F5073691329C16666`.
Both V1 archives:
`4342CC6614AB7C9094CD5A6C7BEB9C9B34F190650893711C3993CB51BF60F62D`.

The first screenshot was captured before rendering finished and was blank;
the subsequent loaded frame was inspected. The user completed the requested
manual run and reported normal pickup visibility and behavior on 2026-09-26.
Long-session performance and all five bonus visuals on hardware are
not certified by the launch check.

Pickup Unity package SHA256:
`18B0ECB214D665AB10E9147C40EF5E7A11478E92C99C943558C0D4FC68F63BE9`.
