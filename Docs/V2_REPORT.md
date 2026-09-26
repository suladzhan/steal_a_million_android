# V2 Implementation Report

## Implemented

The original project now starts as a physical 3D runner. CharacterController drives
forward motion and responsive horizontal dragging. Trigger colliders collect cash,
resolve physical gate choices, damage bankroll on hazards and finish the run.
The portrait follow camera, cartoon character, colored gates, world-space amounts,
pooled particles/floating labels, animations, audio and haptics give direct feedback.
The finish opens an animated vault and turns the runner toward the camera.
Haptics use Android View feedback and honor the device's haptic preference without
requesting VIBRATE permission, following the
[Android haptic feedback API](https://developer.android.com/develop/ui/views/haptics/haptic-feedback).

Risk outcomes use a uniform random sample compared directly to the displayed
probability. Luck changes both display and actual threshold. Market gates display
the complete 20% / 50% / 30% outcome distribution. Outcomes are saved before
suspense and can only be applied once. No loss-streak, spending or ad-based odds
modification exists.

BigMoney serializes arbitrary-precision integers as decimal strings and performs
integer arithmetic with bounded rational multipliers. It is tested beyond
10^400. There is no special terminal behavior at one million. Milestones grant
coins/XP once, update future targets and unlock wealth environments.

## Reused From V1

SaveStore checksum/backup mechanics, the SaveManager adapter, synthesized audio,
haptics, local analytics interface, rewarded-ad abstraction, Canvas helpers,
safe-area and stage layout, app package identity and Android build tooling.

V1 save files are archived once as progress.sav.v1 and progress.sav.v1.bak before
migration. Money, highest wealth, coins, settings and vault ownership/equipment
are carried forward. Invalid run state is discarded without discarding bankroll.
Reset initializes both generations with a fresh V2 save.

## Replaced From V1

GameManager and UIManager now drive the runner and its views. The static SAFE/RISK
card decision loop is not reachable as gameplay. Legacy GameSession, capped
MoneyManager, ShopManager and V1 data remain only for regression compatibility;
V2 uses RunnerSession, BigMoney and RunnerProgress instead.

## Levels

60 explicit route definitions. The first 20 introduce mechanics in a controlled
order; 21-60 combine the reusable segments with fixed seeds. Later levels are
deterministic generated routes. Content validation covers generation through 500.
Level replay is practice-only and cannot farm persistent money, XP or coins.

Endless unlocks at level 12, streams fixed-length chunks ahead, retires old objects
and consumed IDs, persists distance/seed/position and records distance before
failure. Bonus runs cost three keys and contain only cash, coins and keys.

## Worlds

Streets, Downtown, Business District, Luxury District, Millionaire Island,
Financial Capital, Billionaire Bay, Future City and Gold City. Themes use shared
modular geometry, palette changes, taller buildings, palms, water/yachts and rails.
They are stylized procedural environments, not third-party premium art packs.

## Cosmetics

38 total: 10 characters, 3 outfits, 6 trails, 6 money effects, 6 victory animations,
4 vault palettes and 3 gate effects. Purchases require cosmetic coins plus the
specified player level, level progression or wealth milestone. Preview does not
spend currency. Cosmetics never change speed, odds, damage or earnings.

## Missions And Profile

36 regular missions, 3 daily objectives and 24 achievements. Rewards are claimed
once. Daily refresh uses UTC dates and never rolls backwards when the device clock
moves backwards. Offline-only daily timing is not tamper-proof against a player
manually moving their clock forward. Statistics include peak/current/lifetime
wealth, rewards, gates, wins, cash, coins, completed levels, cosmetics and distance.

## Localization

Six tables cover every English key: en, ru, tr, de, ar and zh. Device/system locale
is used when language is Auto; Settings can override it. Nineteen language choices
are supported architecturally; untranslated languages use English. All tables and
fonts are bundled for offline use. No GPS or runtime translation service is used.

TMP uses dynamic Noto fallback assets. Arabic shaping uses the MIT-licensed
RTLTMPro core; back controls, shop categories/rows and setting toggles are mirrored.
Translations have automated placeholder/coverage and rendered layout checks, but
native-speaker editorial review has not been performed.

## How To Run / Debug

Open Assets/Scenes/Boot.unity in Unity 6000.3.23f1 and press Play. Drag horizontally.
Use Steal A Million > Runner Debug during Editor Play Mode for money milestones,
level/world selection, coins/XP, cosmetics, forced risk results, mission counters,
bonus and Endless. Debug controls are excluded from player builds.

## Android Testing / Google Play Build

Tools/Build-Android.ps1 creates a versioned development APK or a release AAB.
The AAB path requires the owner's configured upload keystore. See README for
commands and Docs/STATUS.md for actual artifact verification. Device testing,
release signing and Play Console submission are separate from Editor tests.

## Known Release Work

The first ARM64 device launch exposed issues hidden by Editor testing. The 0.2.1
hotfix makes the outer save version authoritative during migration: Unity can
instantiate a nested runner object even when it is absent from V1 JSON. Regression
tests cover the real V1 field values, an auto-created runner, and repeat loading.
V1 archive files are never overwritten by this recovery.

The 0.2.2 update corrects finish framing on tall phones after a device screenshot
showed the vault clipped at the edge. The player smoothly moves to the victory
mark and the camera/vault composition is checked for both edge-lane finishes.
The update was installed without changing the current save bytes.

Runtime UI now references a saved UI/Default material. `Assets/link.xml` preserves
the primitive mesh/collider types used by procedural geometry, including
CapsuleCollider, which the initial Android build stripped. Unity documents the
runtime component requirement for [CreatePrimitive](https://docs.unity.cn/6000.1/Documentation/ScriptReference/GameObject.CreatePrimitive.html)
and the [link.xml preservation format](https://docs.unity.cn/6000.0/Documentation/Manual/managed-code-stripping-xml-formatting.html).

- Android 15 installation, migration and Russian menu rendering are verified
  (DEVICE_QA.md). Touch, other safe areas, audio/haptics, mid-run suspend/kill
  recovery and sustained frame time still require hardware testing.
- Release AAB signing requires the owner's upload keystore and credentials.
- Ads are a labeled offline mock; no live advertising account/SDK is configured.
- Late-game economy needs longitudinal human playtesting, not only simulations.
- Broader native-speaker localization review and the remaining 13 translations
  are not completed; those locales deliberately fall back to English.

## Optional Improvements

Distinct landmark art per late world, additional handcrafted obstacle patterns,
richer cosmetic meshes, broader accessibility settings and more extensive
low-end-device profiling. These do not require a new Unity project.
