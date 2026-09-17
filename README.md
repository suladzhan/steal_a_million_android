# STEAL A MILLION

Offline portrait Android game made with Unity UI, TextMeshPro and C#.
All money and coins are fictional. There are no accounts, payments or network SDKs.

## Current status

The project compiles in Unity 6000.3.23f1. The standalone C# core passes 43 checks
and 10,000 seeded simulated runs; all 10 Unity EditMode tests pass. A PlayMode
integration test passes the complete gameplay flow and captures 15 screens at
540x960, 540x1170 and 540x1200. A development APK is built and signature/alignment
checks pass. Physical Android testing remains pending.
See [development status](Docs/STATUS.md) for verified results and open checks.

## Open and play

1. Install **Unity 6.3 LTS, 6000.3.23f1** with Unity Hub. Include **Android Build
   Support**, **Android SDK & NDK Tools** and **OpenJDK**. Activate your Unity license
   in the Hub if prompted.
2. In Hub choose **Projects > Add > Add project from disk**, then select this folder:
   `C:\Users\user\Desktop\game 1`.
3. Let packages and scripts import. The initial setup imports TMP Essential
   Resources, creates Boot/MainMenu/Game scenes, a GameConfig asset and an app icon,
   and configures Android. If setup reports that TMP is still importing, wait and
   run **Steal A Million > Setup Project** again.
4. Open `Assets/Scenes/Boot.unity` and press **Play**.
5. Use a portrait Game view, e.g. **540 x 960** or **540 x 1200**. Press Escape to
   pause. The equivalent Android action is the system Back button.

Internet is needed for the first Editor/package installation, not for gameplay.
The game interface is currently English, matching the specification.

## What is implemented

- Main menu, Continue, New Game with confirmation, gameplay, results, Game Over,
  victory, pause, settings and cosmetic shop.
- 30 data-driven decisions / 10 levels, explicit add/set/multiply rewards,
  displayed probabilities and committed outcomes before suspense.
- Number animation, short suspense, press feedback, loss flash/shake, pooled
  confetti, procedural vault art, synthesized sounds/music and Android haptics.
- Local checksummed saves with a backup, settings, peak money, coins and cosmetics.
- Four vault palettes, 1,000 initial coins, 25 coins per level and 250 per victory.
- Mock rewarded-ad interface and one configurable second chance per run.
- Local analytics extension point, Editor/development-only debug controls.
- Portrait Canvas scaling, safe areas, ARM64/IL2CPP Android build commands.

## Configuration and data

`Assets/Resources/GameConfig.asset` is generated on first setup. Select it in the
Inspector to change starting money, target, delays, second-chance money, mock ads,
or development controls.

Decisions: `Assets/Resources/Data/decisions.json`.
Cosmetics: `Assets/Resources/Data/shop.json`.

Each decision has an ID, title, description, level, difficulty, probability,
optional behavior label and three rewards. Reward modes are:

| Mode | Meaning | Display example |
| --- | --- | --- |
| `0` | Add/subtract from current money | `+$1,000`, `-$500`, `NO CHANGE` |
| `1` | Replace the whole total | `LOSE EVERYTHING`, `TOTAL $500` |
| `2` | Multiply the whole total | `TOTAL x2`, `KEEP YOUR TOTAL` |

Levels are ordered and contiguous, starting at 1. Append decisions to extend the
game without UI changes. Run **Steal A Million > Validate Data** after edits.

The all-SAFE route reaches exactly $1,000,000 at decision 30. RISK may shorten the
run or lose it. If a run ends decision 30 below the target, level 10 repeats until
victory or loss. Probabilities are never adjusted based on player behavior.

## Tests

Run the engine-independent tests on Windows without installing Unity:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Test-Core.ps1
```

In Unity use **Window > General > Test Runner > EditMode > Run All**. These tests
also verify Unity JsonUtility serialization and production Resources loading.
For batch execution, close the Editor instance using this project first:

```powershell
.\Tools\Test-Unity.ps1 -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe'
```

For the full PlayMode flow and rendered screenshots, use:

```powershell
.\Tools\Test-Unity.ps1 -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe' -Visual
```

This command uses a separate save in `TestResults/runtime-save` and writes images
to `TestResults/Screenshots`. It checks text truncation, graphic renderers, nonblank
frames, duplicate input, pause, loss, second chance, all 30 decisions, victory,
cosmetic purchases and settings persistence. Run it through the script to ensure
test saves are isolated. Unity UI tests do not replace physical phone testing.

See [QA checklist](Docs/QA.md) for device and UI checks. Standalone tests do not
validate Canvas rendering, Android input, sound, haptics or build compatibility.

## Android APK

In Unity choose **Steal A Million > Build > Android APK (Development)**. Output:
`Builds/Android/StealAMillion.apk`.

Alternatively, with the Editor closed:

```powershell
.\Tools\Build-Android.ps1 -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe' -Format Apk
```

Enable USB debugging on an Android device and install the resulting APK:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe' devices
& 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe' install -r '.\Builds\Android\StealAMillion.apk'
```

Package: `com.sulik.stealamillion`. Minimum API: **26**. Architecture: **ARM64**.
The verified development build is **v0.1.0**, **30,144,057 bytes**, targeting API
**36**. It passed `apksigner verify` and `zipalign -c -P 16 4`. This is a test APK,
not a store release; installation and gameplay on a physical phone remain pending.

## Android AAB

1. Configure an upload keystore in **Player Settings > Android > Publishing
   Settings**. Keep the key and passwords outside the repository.
2. Choose **Steal A Million > Build > Android AAB (Release)**, or run the build
   script with `-Format Aab`.
3. Output: `Builds/Android/StealAMillion.aab`. Debug UI is excluded from this build.
4. Validate the signed AAB and test it on a real device before uploading it to a
   Google Play internal testing track. Store submission is not automated.

The release command requires a configured custom keystore so it cannot silently
produce a debug-signed release. Mock ads remain clearly labeled and can be disabled
in GameConfig; real AdMob integration is intentionally absent.

## Project layout

- `Assets/Scripts/Core`: engine-independent money, decision, progression, shop and
  persistence logic.
- `Assets/Scripts/Gameplay`: Unity lifecycle, input and game flow.
- `Assets/Scripts/UI`: Canvas views, responsive layout, procedural graphics/effects.
- `Assets/Scripts/Systems`: save adapter, audio, haptics, mock ads and analytics.
- `Assets/Editor`: repeatable project setup, data validation and Android builds.
- `Assets/Tests/EditMode`: Unity serialization/integration checks.
- `Assets/Tests/PlayMode`: gameplay and rendered UI regression checks.
- `Tests`, `Tools`: standalone C# tests and PowerShell commands.

Scenes contain no hand-wired references: the persistent GameManager creates the
Canvas and services when Play starts. Boot initializes; MainMenu and Game are
lightweight scene boundaries. Screen state and run data remain separate.

## Sources

- [Unity 6000.3.23f1 release](https://unity.com/releases/editor/whats-new/6000.3.23f1)
- [Unity 6.3 Android compatibility](https://docs.unity3d.com/6000.3/Documentation/Manual/android-requirements-and-compatibility.html)
- [Unity UI package](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.ugui.html)
