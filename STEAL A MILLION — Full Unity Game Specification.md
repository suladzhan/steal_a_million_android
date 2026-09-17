# STEAL A MILLION

## 0. ROLE

You are an autonomous senior Unity mobile game developer, game designer, UI/UX designer, and QA engineer.

Your task is to build a complete playable Android casual game called:

**STEAL A MILLION**

Do not only describe the implementation.

**Actually create and modify the Unity project, write the C# code, create the UI, test the game, find errors, fix them, and prepare an Android build.**

The final result should feel like a small polished mobile game, not a programming demo.

---

# 1. GAME CONCEPT

## Core idea

The player starts with a small amount of fictional in-game money.

At every step, the player must choose between:

### SAFE

A guaranteed smaller reward.

### RISK

A potentially much larger reward with a displayed probability.

Example:

```text
CURRENT MONEY

$10,000


SAFE
Take $10,000


RISK
50% → $30,000
50% → $0
```

The player repeatedly makes decisions.

The ultimate goal is:

# STEAL A MILLION

Reach:

**$1,000,000**

---

# 2. IMPORTANT LEGAL / GAMEPLAY RULE

This is a fictional casual game.

The game must NOT contain:

- real-money gambling
- betting
- deposits
- withdrawals
- real-money prizes
- casino mechanics involving real currency
- cryptocurrency
- paid random rewards

All money shown in the game is fictional in-game currency.

The game should feel like a casual risk/reward puzzle rather than a gambling app.

---

# 3. TARGET PLATFORM

Primary platform:

**Android**

Orientation:

**Portrait**

Input:

**Touch**

The game should also work inside the Unity Editor for development.

Target aspect ratios:

- 9:16
- 9:20
- 9:19.5

UI must adapt to different Android screen sizes.

---

# 4. DEVELOPMENT STACK

Use:

- Unity LTS
- C#
- Unity UI / Canvas
- TextMeshPro
- Android Build Support
- Local save system

Do NOT add unnecessary dependencies.

Do NOT create a backend.

Do NOT require user accounts.

Do NOT require internet connection for core gameplay.

---

# 5. GAME LOOP

The complete gameplay loop:

```text
MAIN MENU
    ↓
PLAY
    ↓
DECISION
    ↓
SAFE / RISK
    ↓
RESULT ANIMATION
    ↓
MONEY UPDATE
    ↓
NEXT DECISION
    ↓
...
    ↓
$1,000,000
    ↓
VICTORY
```

If the player reaches $0:

```text
GAME OVER
    ↓
RESTART
```

---

# 6. FIRST-TIME EXPERIENCE

The first game should teach the player without a tutorial screen.

The first decision should be extremely simple.

Example:

```text
YOU HAVE

$100


TAKE $100

OR


RISK

80% → $250
20% → $0
```

After the first choice, immediately show the result.

The player should understand the entire game within approximately 10 seconds.

---

# 7. STARTING MONEY

Start the player with:

**$100**

The exact starting amount should be configurable.

Create a central GameConfig so values can easily be changed later.

---

# 8. WIN CONDITION

The player wins when:

```text
currentMoney >= 1,000,000
```

Show a special victory screen.

Example:

```text
🎉

YOU STOLE A MILLION!

$1,000,000

LEVEL COMPLETE

[ PLAY AGAIN ]
```

Use:

- confetti
- large number animation
- screen effects
- victory sound
- vibration
- gold visual effects

The victory moment should feel satisfying.

---

# 9. LOSS CONDITION

If:

```text
currentMoney <= 0
```

show:

```text
GAME OVER

You reached:

$42,500

[ TRY AGAIN ]
```

Do not immediately restart automatically.

---

# 10. DECISION SYSTEM

Create a reusable, data-driven decision system.

Each decision must support:

- ID
- title
- description
- safe reward
- risk probability
- risk win reward
- risk loss reward
- optional special behavior
- difficulty
- level

Example data:

```text
Decision:

id:
decision_01

safeReward:
100

riskProbability:
0.80

riskWinReward:
250

riskLossReward:
0
```

The decision system should NOT hardcode individual decisions into the UI.

The UI should display whatever decision data is currently loaded.

---

# 11. DECISION TYPES

Implement several types.

## Type 1 — Simple risk

```text
SAFE
+$100

RISK
80% → +$250
20% → $0
```

---

## Type 2 — Bigger risk

```text
SAFE
+$500

RISK
60% → +$2,000
40% → -$500
```

---

## Type 3 — Double or lose

```text
KEEP
$5,000

RISK
50% → $10,000
50% → $0
```

---

## Type 4 — High probability / low reward

```text
SAFE
+$1,000

RISK
90% → +$1,500
10% → $0
```

---

## Type 5 — Low probability jackpot

```text
SAFE
+$10,000

RISK
10% → +$100,000
90% → $0
```

---

## Type 6 — Dangerous decision

```text
SAFE
+$50,000

RISK
25% → +$300,000
75% → -$25,000
```

---

# 12. DECISION BALANCING

Do not make the game mathematically trivial.

The player should have meaningful choices.

Avoid always making:

```text
RISK = obviously better
```

or:

```text
SAFE = obviously better
```

The player should sometimes genuinely hesitate.

Example:

```text
SAFE:
+$10,000

RISK:
70% → +$20,000
30% → -$5,000
```

The decisions should become more dangerous as the player approaches $1,000,000.

---

# 13. LEVEL STRUCTURE

Create at least:

**30 playable decisions**

distributed across:

**10 levels**

Each level contains approximately 3 decisions.

Example:

```text
LEVEL 1
Decision 1
Decision 2
Decision 3

LEVEL 2
Decision 4
Decision 5
Decision 6

...

LEVEL 10
Decision 28
Decision 29
Decision 30
```

The system must be easy to expand to 100+ decisions later.

---

# 14. PROGRESSION

Early game:

- small amounts
- high probabilities
- simple decisions

Mid game:

- larger rewards
- more risk
- more tempting options

Late game:

- huge rewards
- low probabilities
- high losses
- jackpot decisions

Final decisions should create tension.

---

# 15. MONEY SYSTEM

Create a dedicated MoneyManager.

Responsibilities:

- current money
- add money
- subtract money
- set money
- format money
- notify UI about changes

Example formatting:

```text
$100
$1,250
$25,000
$250,000
$1,000,000
```

Use commas.

Do not display unnecessary decimals.

---

# 16. MONEY ANIMATION

When money changes:

```text
$10,000
↓
$15,000
```

Animate the number smoothly.

The number should visually count toward the new value.

For large wins:

- scale animation
- particles
- sound
- vibration

For large losses:

- red flash
- small shake
- sound
- vibration

---

# 17. MAIN MENU

Create a polished main menu.

Layout:

```text
STEAL
A
MILLION


$25,000

[ CONTINUE ]

[ NEW GAME ]

[ SHOP ]

[ SETTINGS ]
```

If no saved game exists:

```text
[ PLAY ]
```

instead of Continue.

---

# 18. GAME SCREEN

Main game screen:

```text
┌─────────────────────────┐
│                         │
│       $25,000           │
│                         │
│       LEVEL 4           │
│                         │
│   What will you do?     │
│                         │
│                         │
│  ┌───────────────────┐  │
│  │      TAKE         │  │
│  │      +$5,000      │  │
│  └───────────────────┘  │
│                         │
│           OR            │
│                         │
│  ┌───────────────────┐  │
│  │      RISK         │  │
│  │  70% → +$15,000  │  │
│  │  30% → $0        │  │
│  └───────────────────┘  │
│                         │
└─────────────────────────┘
```

The most important information must be visible without scrolling.

---

# 19. BUTTON DESIGN

Buttons must be:

- large
- touch friendly
- readable
- visually distinct
- animated on press

Minimum touch target:

approximately 48dp or larger.

Button press animation:

```text
normal
↓
slightly smaller
↓
normal
```

Use a subtle scale animation.

---

# 20. RESULT SCREEN

After choosing an option, temporarily show the result.

Example win:

```text
YOU WON!

+$15,000

$25,000
↓
$40,000
```

Then automatically continue to the next decision after a short delay.

Allow the player to tap to continue immediately.

---

# 21. RISK ANIMATION

When choosing Risk:

Do NOT reveal the result instantly.

Create a very short suspense animation.

Example:

```text
RISKING...

70%

...

YOU WON!

+$15,000
```

Keep it fast.

Target:

approximately 1–2 seconds.

Do not create an annoying long animation.

---

# 22. BIG WIN ANIMATION

For large wins:

- confetti
- particles
- number scaling
- screen flash
- vibration
- sound

Use different intensity depending on reward size.

---

# 23. BIG LOSS ANIMATION

For major losses:

- screen shake
- red flash
- number animation
- vibration
- loss sound

Keep it satisfying but not frustrating.

---

# 24. AUDIO SYSTEM

Create:

```text
AudioManager
```

Support:

- button click
- safe choice
- risk choice
- win
- loss
- jackpot
- level complete
- victory
- game over

Also support:

```text
Sound ON/OFF
Music ON/OFF
```

Use placeholder sounds if necessary.

The game must work even if audio assets are temporarily unavailable.

---

# 25. VIBRATION

Support:

- button press
- small win
- big win
- loss
- jackpot
- victory

Create:

```text
HapticsManager
```

with easy enable/disable settings.

---

# 26. SAVE SYSTEM

Use local persistence.

Save:

```text
currentLevel
currentMoney
highestMoney
soundEnabled
musicEnabled
vibrationEnabled
coins
unlockedCosmetics
```

Save automatically after meaningful progress.

Do not require login.

---

# 27. CONTINUE

If the player closes the game during a run:

On next launch:

```text
CONTINUE

Level 6
$42,500

[ CONTINUE ]

[ NEW GAME ]
```

Starting a New Game should reset the current run but not necessarily permanent cosmetics/coins.

---

# 28. SHOP

Create a simple cosmetic shop architecture.

The shop is NOT required to contain real purchases in the MVP.

Use fictional coins.

Possible items:

### Character skins

- Classic
- Gold
- Diamond
- Neon

### Money effects

- Coins
- Cash
- Gold particles

### Victory effects

- Confetti
- Fireworks
- Money rain

The system must be data-driven.

---

# 29. COINS

Coins are separate from the main fictional money.

Example:

```text
Money:
$42,500

Coins:
1,250
```

Coins are persistent.

Coins can be used for cosmetics.

Initially give the player:

**1,000 coins**

for testing.

---

# 30. REWARDED ADS ARCHITECTURE

Prepare the game for AdMob rewarded ads.

Do NOT require AdMob during the first development phase.

Create:

```text
AdManager
```

with methods conceptually similar to:

```text
ShowRewardedAd()
IsRewardedAdReady()
```

During development, create a mock implementation.

Example:

```text
GAME OVER

[ WATCH AD ]
Second Chance
```

The mock ad can simply wait 1–2 seconds and grant the reward.

Later replace the implementation with Google Mobile Ads / AdMob.

Do not couple gameplay directly to the ad SDK.

---

# 31. SECOND CHANCE

After losing:

```text
GAME OVER

$0

[ WATCH AD ]

GET SECOND CHANCE
```

If the player uses it:

Restore a small amount of money.

Example:

```text
$500
```

or restore the previous decision.

Make this configurable.

---

# 32. OTHER REWARDED AD

Optional future feature:

```text
WATCH AD

2X YOUR NEXT REWARD
```

Do not implement this unless the core game is already stable.

---

# 33. ADS RULES

Do not spam ads.

Never interrupt:

- active decision making
- risk animation
- victory animation

Possible future placements:

- after completing a level
- after game over
- optional rewarded ads

Core gameplay must remain playable without ads.

---

# 34. ANALYTICS ARCHITECTURE

Prepare a lightweight AnalyticsManager interface.

Potential future events:

```text
game_started
decision_made
safe_selected
risk_selected
risk_won
risk_lost
level_completed
game_over
victory
rewarded_ad_started
rewarded_ad_completed
shop_opened
item_purchased
```

Do not add a complicated analytics SDK during the initial MVP unless necessary.

Create clean event methods so analytics can be added later.

---

# 35. TUTORIAL

Do not create a long tutorial.

Use the first decision as the tutorial.

Optionally highlight the two buttons during the first decision:

```text
Choose one.
```

After the first decision, remove the tutorial hint permanently.

---

# 36. SETTINGS

Create:

```text
SETTINGS

Sound       ON/OFF
Music       ON/OFF
Vibration   ON/OFF

[ RESET PROGRESS ]
[ BACK ]
```

Reset Progress must show a confirmation dialog.

---

# 37. RESET CONFIRMATION

Example:

```text
RESET ALL PROGRESS?

This cannot be undone.

[ CANCEL ]

[ RESET ]
```

---

# 38. VISUAL STYLE

Overall style:

**modern casual mobile game**

Use:

- dark background
- high contrast
- rounded cards
- large typography
- simple 2D graphics
- subtle gradients if appropriate
- green positive feedback
- red negative feedback
- gold for jackpot/million

Avoid:

- clutter
- excessive text
- complicated menus
- tiny buttons
- photorealistic assets

---

# 39. COLOR LOGIC

Use color semantically.

Positive:

Green

Negative:

Red

Neutral:

White/gray

Jackpot:

Gold/yellow

Do not rely only on color.

Use icons/text as well.

---

# 40. TYPOGRAPHY

Use TextMeshPro.

The main money amount should be one of the largest elements on screen.

Example:

```text
$125,000
```

The player should understand their current money instantly.

---

# 41. RESPONSIVE UI

The game must support different screen sizes.

Use:

- Canvas Scaler
- anchors
- safe area handling
- responsive layouts

Implement a SafeArea component for devices with notches.

---

# 42. PERFORMANCE

Target:

**60 FPS**

Avoid unnecessary:

- Update loops
- allocations
- instantiated objects every frame
- heavy particles
- large textures

The game is simple and should run smoothly on mid-range Android devices.

---

# 43. ARCHITECTURE

Use a simple modular architecture.

Recommended systems:

```text
GameManager
GameStateManager
DecisionManager
MoneyManager
LevelManager
SaveManager
UIManager
AudioManager
HapticsManager
AdManager
AnalyticsManager
ShopManager
CurrencyManager
```

Do not over-engineer.

Managers should have clear responsibilities.

---

# 44. DATA STRUCTURES

Decisions should be data-driven.

Use ScriptableObjects or another simple Unity-native approach.

Example conceptual structure:

```text
DecisionData

id
title
description

safeReward

riskProbability
riskWinReward
riskLossReward

difficulty
```

This should allow new decisions to be created without rewriting gameplay code.

---

# 45. LEVEL DATA

Level data should contain:

```text
levelID
decisions[]
```

Example:

```text
Level 1
    decision_01
    decision_02
    decision_03
```

---

# 46. RANDOMNESS

Use Unity's random functionality for risk outcomes.

The displayed probability must correspond to the actual probability.

Example:

```text
70%
```

must actually mean approximately 70% probability.

Do not secretly manipulate outcomes based on player behavior.

---

# 47. FAIRNESS

The player should be able to understand the risk before choosing.

Always display the relevant probability.

Example:

```text
70% → +$20,000
30% → $0
```

Do not hide critical information.

---

# 48. TEST MODE

Create a development/debug mode that can optionally:

- add money
- skip levels
- force risk win
- force risk loss
- reset save
- unlock cosmetics

This should be disabled in release builds.

---

# 49. DEBUG UI

In Unity Editor only, optionally show:

```text
DEBUG

Money:
Level:
Decision:
Probability:
```

Do not show debug information in release builds.

---

# 50. ERROR HANDLING

The game must not crash because of:

- missing save data
- invalid decision data
- missing optional audio
- missing cosmetic
- unexpected money value

Use safe defaults.

---

# 51. PROJECT STRUCTURE

Use a clean structure similar to:

```text
Assets/
├── Art/
│   ├── UI/
│   ├── Characters/
│   ├── Effects/
│   └── Icons/
│
├── Audio/
│
├── Data/
│   ├── Decisions/
│   ├── Levels/
│   └── Shop/
│
├── Prefabs/
│   ├── UI/
│   ├── Effects/
│   └── Gameplay/
│
├── Scenes/
│   ├── Boot
│   ├── MainMenu
│   └── Game
│
├── Scripts/
│   ├── Core/
│   ├── Gameplay/
│   ├── UI/
│   ├── Systems/
│   └── Data/
│
└── Settings/
```

Do not create unnecessary folders.

---

# 52. SCENES

Use a simple scene structure.

### Boot

Initializes persistent systems.

### MainMenu

Main menu.

### Game

Gameplay.

Keep the number of scenes small.

---

# 53. GAME STATES

Implement states such as:

```text
MainMenu
Playing
ShowingResult
GameOver
Victory
Paused
```

Prevent invalid interactions.

For example, the player must not be able to press Safe and Risk simultaneously.

Disable decision buttons immediately after one is selected.

---

# 54. PAUSE

Support Android pause/background behavior.

When the app loses focus:

- save progress
- preserve state
- prevent accidental input

On return:

- continue normally

---

# 55. BACK BUTTON

Android back button:

During gameplay:

```text
Pause Menu
```

Pause menu:

```text
RESUME
MAIN MENU
```

Main menu:

Exit confirmation if appropriate.

---

# 56. FIRST 30 DECISIONS

Create at least 30 balanced decisions.

Use varied values and probabilities.

Examples:

```text
1.
SAFE +$100
RISK 80% → +$250 / 20% → $0

2.
SAFE +$250
RISK 70% → +$600 / 30% → $0

3.
SAFE +$500
RISK 60% → +$1,500 / 40% → $0

4.
SAFE +$1,000
RISK 75% → +$2,000 / 25% → -$500

5.
SAFE +$2,000
RISK 50% → +$6,000 / 50% → $0
```

Continue creating varied decisions.

Do not simply multiply every number.

Create interesting risk/reward situations.

---

# 57. BALANCE TESTING

Create automated or editor-testable logic for:

- money cannot become NaN
- money cannot become negative unless intentionally allowed temporarily
- probability is between 0 and 1
- level IDs are valid
- every level has decisions
- every decision has valid rewards

---

# 58. GAME FEEL PRIORITY

The most important thing is not technical complexity.

The game should feel:

- fast
- responsive
- satisfying
- understandable
- slightly tense
- rewarding

Every interaction should happen quickly.

Avoid unnecessary loading screens.

---

# 59. FIRST DEVELOPMENT PHASE

Build only:

```text
Main Menu
↓
Game
↓
Money
↓
Decision
↓
Safe/Risk
↓
Result
↓
Next Decision
↓
Game Over
↓
Victory
```

Do not implement Shop, Analytics or real Ads before this works.

---

# 60. SECOND DEVELOPMENT PHASE

Add:

- 30 decisions
- 10 levels
- save system
- settings
- audio
- vibration
- animations

---

# 61. THIRD DEVELOPMENT PHASE

Add:

- cosmetic shop
- coins
- mock rewarded ads
- second chance
- polish
- particle effects

---

# 62. FOURTH DEVELOPMENT PHASE

Prepare:

- Android configuration
- application icon
- splash screen
- package name
- release build
- performance testing

---

# 63. ANDROID CONFIGURATION

Configure:

```text
Platform:
Android

Orientation:
Portrait

Package:
com.sulik.stealamillion
```

Use a sensible minimum Android version supported by the selected Unity LTS.

Use ARM64.

Prepare an AAB build for Google Play.

Also create an APK for direct device testing if convenient.

---

# 64. APPLICATION ICON

Create a simple icon concept:

A large gold:

```text
$
```

or:

```text
$1M
```

on a dark background.

Do not use copyrighted logos.

The icon should remain readable at small sizes.

---

# 65. SPLASH SCREEN

Use a minimal splash screen.

Show:

```text
STEAL A MILLION
```

Do not create a long intro animation.

---

# 66. TESTING CHECKLIST

Before considering the MVP complete, test:

### Main menu

- Play works
- Continue works
- New Game works
- Settings work

### Gameplay

- Money updates correctly
- Safe works
- Risk works
- Probability works
- Buttons cannot be double-clicked
- Next decision loads
- Level progression works

### Game over

- $0 triggers game over
- Restart works
- Second chance mock works

### Victory

- $1,000,000 triggers victory
- Celebration works
- Replay works

### Save

- Close app
- Reopen
- Continue
- Verify money and level

### Android

- Touch input works
- UI fits screen
- Back button works
- No critical errors
- No crashes
- Performance is smooth

---

# 67. QUALITY BAR

Do not stop when the game merely compiles.

The MVP should:

- look coherent
- feel responsive
- have working animations
- have readable UI
- work on an Android phone
- have no obvious broken states

---

# 68. DEVELOPMENT RULES

## Rule 1

Do not over-engineer.

## Rule 2

Do not add a backend.

## Rule 3

Do not add accounts.

## Rule 4

Do not add multiplayer.

## Rule 5

Do not spend hours searching for perfect assets.

Use simple assets and placeholders when necessary.

## Rule 6

Do not stop after writing code.

Run/test/fix.

## Rule 7

If something fails, diagnose and fix it.

## Rule 8

Do not rewrite working systems without a reason.

## Rule 9

Keep the project easy for another developer to understand.

## Rule 10

Always prioritize a playable game over theoretical architecture.

---

# 69. AUTONOMOUS DEVELOPMENT WORKFLOW

Follow this exact workflow.

### STEP 1

Inspect the workspace.

Determine whether a Unity project already exists.

If not, create or initialize the project.

### STEP 2

Verify Unity and Android support.

### STEP 3

Create the project structure.

### STEP 4

Implement the core gameplay.

### STEP 5

Run the project.

### STEP 6

Fix compile/runtime errors.

### STEP 7

Implement progression.

### STEP 8

Implement UI polish.

### STEP 9

Implement save system.

### STEP 10

Implement audio/haptics.

### STEP 11

Implement shop architecture.

### STEP 12

Implement mock rewarded ads.

### STEP 13

Prepare Android.

### STEP 14

Build APK/AAB.

### STEP 15

Test the build.

### STEP 16

Fix remaining issues.

### STEP 17

Report the final status.

---

# 70. IMPORTANT AGENT BEHAVIOR

Do not repeatedly ask me for permission to perform normal development tasks.

Make reasonable decisions yourself.

Only stop and ask me if:

- a required external account/login is needed
- Unity cannot be installed or accessed
- an external service requires credentials
- a design decision would fundamentally change the game concept

Otherwise continue autonomously.

---

# 71. FINAL DELIVERABLE

At the end, provide:

## 1. Project status

What was successfully implemented.

## 2. How to run

Exact Unity steps.

## 3. Android testing

Exact steps to install and test the APK.

## 4. Android release

Exact steps to create the AAB.

## 5. Remaining issues

List only real remaining issues.

## 6. Next improvements

Suggest the highest-value improvements after the MVP.

---

# 72. FINAL GOAL

The final result should be:

```text
A real Android casual game
        ↓
Playable
        ↓
Polished
        ↓
Offline
        ↓
Save system
        ↓
30+ decisions
        ↓
10 levels
        ↓
Victory condition
        ↓
Game Over
        ↓
Rewarded-ad architecture
        ↓
Ready for Google Play testing
```

The game should be simple enough to understand immediately but polished enough that a stranger could play it and understand the objective without instructions.

# START NOW

Do not only provide a plan.

Inspect the workspace and begin implementation.

Build the game incrementally.

Test after every major milestone.

Fix errors yourself.

The goal is to finish a playable Android build today.