# STEAL A MILLION V2
# COMPLETE UNITY MASTER SPECIFICATION
## Long-Term 3D Money Runner

---

# 0. PURPOSE OF THIS DOCUMENT

This document is the SINGLE SOURCE OF TRUTH for Version 2 of:

# STEAL A MILLION

The existing Unity project currently contains Version 1 of the game.

Version 1 is already backed up in Git.

DO NOT create a new Unity project.

DO NOT create:

- StealAMillionV2Project
- NewRunnerProject
- RunnerGame2
- another duplicate Unity project

Modify the EXISTING Unity project directly.

Inspect the existing project before making large changes.

Reuse working systems where appropriate.

Refactor systems where necessary.

Replace obsolete Version 1 gameplay where necessary.

The final result must be a real playable Android mobile game, not a prototype consisting only of menus and buttons.

---

# 1. AGENT ROLE

Act as an autonomous:

- senior Unity developer
- C# gameplay programmer
- mobile game designer
- technical artist
- UI/UX designer
- level designer
- economy designer
- animation developer
- VFX developer
- Android build engineer
- localization engineer
- QA engineer

Do not merely describe implementation.

Actually:

- inspect files
- modify scripts
- create scripts
- create scenes
- create GameObjects
- create prefabs
- create materials
- create ScriptableObjects
- create UI
- configure levels
- configure Android
- run compilation where possible
- fix errors
- test systems
- iterate

Do not stop after producing architecture or TODO comments.

---

# 2. GAME IDENTITY

Game name:

# STEAL A MILLION

Genre:

3D / 2.5D Casual Money Runner

Platform:

Android first.

Orientation:

Portrait.

Primary control:

Horizontal finger drag / swipe.

Core fantasy:

The player starts with very little money and runs through increasingly rich environments while collecting cash, avoiding financial losses, choosing between guaranteed SAFE rewards and probabilistic RISK rewards, unlocking cosmetics, increasing wealth rank, and becoming richer indefinitely.

The game begins with:

$0 or approximately $100.

The first major legendary objective is:

# $1,000,000

However:

# $1,000,000 IS NOT THE END OF THE GAME.

It is only the first major milestone.

Progression continues through:

$10M

$100M

$1B

$10B

$100B

$1T

and far beyond.

There must be NO practical gameplay money cap.

---

# 3. CORE PLAYER FANTASY

The player should feel:

"I started with almost nothing."

Then:

"I am actually getting richer."

Then:

"I finally reached $1 million."

Then:

"Now an entirely richer world opened."

The world, character, UI and available gameplay should evolve visually as wealth increases.

---

# 4. CORE GAMEPLAY LOOP

The fundamental loop is:

RUN

↓

COLLECT MONEY

↓

DODGE OBSTACLES

↓

CHOOSE PATH

↓

SAFE OR RISK

↓

WIN / LOSE MONEY

↓

CONTINUE RUNNING

↓

COMPLETE LEVEL

↓

EARN COINS / XP / PROGRESSION

↓

UNLOCK CONTENT

↓

NEXT LEVEL

The loop must be understandable within seconds.

---

# 5. CORE DIFFERENTIATOR

This must NOT become another generic:

+10 versus ×2

mobile runner.

The central identity is:

# SAFE VS RISK

The player physically approaches two possible paths.

Example:

LEFT:

SAFE
+$500

RIGHT:

RISK
×3
60% WIN

The player swipes toward one gate.

SAFE:

guaranteed reward.

RISK:

larger potential outcome with clearly displayed probability.

Displayed probability MUST match actual probability.

---

# 6. VISUAL STYLE

The game should look:

- bright
- colorful
- glossy
- modern
- clean
- casual
- satisfying
- stylized
- highly readable
- visually attractive in vertical short-form video

Do NOT use a dark casino-style visual identity.

Do NOT use gritty realism.

Do NOT make the game look like a financial dashboard.

The player must see actual:

- track
- character
- gates
- cash
- obstacles
- environments

---

# 7. PRIMARY COLOR PALETTE

Use these as target colors.

Minor shader/material adjustments are allowed.

## Sky

Primary:

#7DD8FF

Alternative gradient top:

#55C4F5

Clouds:

#FFFFFF

---

## Track

Top:

#F4F6F8

Sides:

#D9E1EA

Shadow areas:

#BEC8D3

---

## Cash

Primary:

#35D06F

Dark:

#159447

Highlight:

#A9FFBE

---

## SAFE

Primary:

#34D17B

Secondary:

#1EB865

Glow:

#78F5AC

Text:

#FFFFFF

---

## RISK

Primary:

#FF4D5A

Secondary:

#E62D42

Highlight:

#FF8790

Probability:

#FFE76A

Text:

#FFFFFF

---

## Negative / Danger

Primary:

#FF4055

Dark:

#CA2438

---

## Gold

Primary:

#FFC928

Bright:

#FFE55C

Highlight:

#FFF5B1

---

## Mystery

Primary:

#A85CFF

Secondary:

#7F39E8

---

## Premium Blue

Primary:

#3E78FF

Light:

#65A0FF

---

## UI Dark

#16202A

---

## UI Gray

#82909D

---

## UI Panel

#FFFFFF

---

# 8. CAMERA

Use a third-person elevated runner camera.

Camera should:

- follow behind character
- stay above character
- point downward toward the track
- show upcoming gates several seconds before the player reaches them

Suggested initial relative offset:

X = 0

Y = 7 to 9

Z = -8 to -11

Suggested pitch:

25 to 35 degrees downward.

Tune visually.

Use smooth follow.

Do NOT create excessive camera lag.

Do NOT let camera rotate unpredictably.

Normal running:

stable camera.

Risk event:

slight temporary dramatic adjustment.

Large win:

small FOV pulse.

Large collision:

small brief shake.

Victory:

camera rises and pulls back.

---

# 9. PLAYER CHARACTER

Create a stylized cartoon humanoid.

If no appropriate asset is available, construct one from simple Unity primitives.

Possible geometry:

Head:
sphere.

Torso:
rounded cube or capsule.

Arms:
capsules.

Legs:
capsules.

Hands:
small spheres.

Shoes:
rounded boxes.

Approximate height:

1.6 to 2 Unity units.

Default appearance:

shirt:
#3E78FF

pants:
#253A70

shoes:
#FFFFFF

skin:
warm stylized neutral skin tone.

Face can be minimal.

Two simple eyes are enough.

Avoid uncanny realism.

---

# 10. PLAYER ANIMATIONS

The player must not slide statically.

Required:

## Run

Alternating legs.

Alternating arms.

Small body bounce.

Small torso motion.

## Horizontal movement

Character leans slightly in direction of travel.

Smooth lane movement.

## Big win

Jump.

Arms upward.

Brief celebration.

## Loss

Small stumble.

Recover quickly.

## Finish

Victory pose.

## Main Menu / Shop

Idle animation.

## Millionaire milestone

Special stronger celebration.

Animations can initially be procedural if necessary.

---

# 11. MOVEMENT SYSTEM

Player automatically moves forward.

Player controls horizontal movement only.

Touch:

finger drag anywhere horizontally.

Editor:

mouse drag should simulate touch.

Player movement requirements:

- smooth
- responsive
- forgiving
- no virtual joystick
- no instant teleport
- clamp inside valid track bounds

Suggested initial speed:

6 to 8 Unity units/sec.

Forward speed may evolve moderately later.

Do not rely on extreme speed for difficulty.

---

# 12. TRACK SYSTEM

Build modular reusable track segments.

Suggested width:

7 to 10 Unity units.

Suggested segment length:

8 to 15 Unity units.

Possible segment categories:

- Straight
- Cash
- SafeRisk
- Obstacle
- SplitPath
- Investment
- RiskChain
- Jackpot
- Key
- Bonus
- Finish

Create reusable prefabs.

Do not create hundreds of manually unique track GameObjects if modular architecture is possible.

---

# 13. TRACK VARIATION

Later worlds should support:

- straight roads
- gentle curves
- bridges
- wider sections
- narrow sections
- elevated platforms
- city roads
- rooftops
- glass roads
- golden roads
- island paths
- multi-route splits

Controls must remain easy to understand.

---

# 14. CASH PICKUPS

Cash is the most frequent collectible.

Types:

Single bill.

Small bundle.

Money stack.

Large cash stack.

Rare gold stack.

Visual construction:

Thin green boxes.

Simple dollar symbol.

Cash should:

- rotate slowly
- float slightly
- look collectible
- be visually distinct from environment

On collect:

cash quickly moves toward player.

Scale down.

Disappear.

Spawn small green particles.

Play money sound.

Display floating number.

Update bankroll immediately.

---

# 15. EARLY GAME ECONOMY

This requirement is extremely important.

Money must grow SLOWLY at the beginning.

Do NOT let the player reach hundreds of thousands within the first few minutes.

The first:

$100

$1,000

$10,000

must feel meaningful.

Example opening rewards:

+$5

+$10

+$15

+$20

+$25

+$50

+$75

+$100

Early risk reward examples:

×1.5 at 70%

+$250 at 50%

×2 at 60%

Do not immediately give:

+$100K

×10

+$1M.

---

# 16. ECONOMIC PHASES

## PHASE 1 — STREET MONEY

Wealth:

$0 → $1K

Typical rewards:

$5 → $250.

---

## PHASE 2 — SIDE HUSTLE

$1K → $10K.

Rewards:

$50 → $1K.

---

## PHASE 3 — BUSINESS

$10K → $100K.

Rewards:

hundreds → several thousand.

---

## PHASE 4 — RICH

$100K → $1M.

Rewards:

thousands → tens of thousands.

---

## PHASE 5 — MILLIONAIRE

$1M → $10M.

Unlock new gameplay and visual tier.

---

## PHASE 6 — TYCOON

$10M → $1B.

---

## PHASE 7 — BILLIONAIRE

$1B → $1T.

---

## PHASE 8 — ENDLESS WEALTH

$1T+.

Use scalable formulas.

No hard ending.

---

# 17. LARGE NUMBER SYSTEM

Do not depend on a normal 32-bit integer.

Money system must support extremely large values.

Create a robust large-number representation.

Options:

- custom BigNumber
- mantissa + exponent
- scientific number structure
- safe compatible equivalent

Requirements:

No overflow.

No accidental negative rollover.

No Infinity in UI.

No NaN.

Serializable.

Save/load compatible.

Comparable.

Supports:

addition

subtraction

multiplication

percentages

formatting.

---

# 18. MONEY FORMATTING

Examples:

$0

$15

$999

$1.25K

$15.8K

$999K

$1.24M

$18.5M

$1.20B

$45.8B

$1.10T

$55T

$1.5Qa

$20Qi

For very large values:

scientific notation may be used.

Example:

$1.24e36

Suggested suffixes:

K

M

B

T

Qa

Qi

Sx

Sp

Oc

No

Dc

---

# 19. MONEY DISPLAY

Display bankroll:

## World space

Above character.

Example:

$24.5K

Always face camera.

Large readable text.

White with subtle dark shadow.

## Screen UI

Top center:

$24.5K

Below or nearby:

wealth progress.

---

# 20. FLOATING MONEY TEXT

Positive:

green.

Negative:

red.

Jackpot:

gold.

Examples:

+$25

+$500

+$5K

-$500

-25%

×3!

JACKPOT!

Animation:

spawn small

scale larger

settle

move upward

fade.

Duration:

approximately 0.6 to 1 second.

---

# 21. SAFE GATES

Physical green gate.

Structure:

Two vertical rounded pillars.

Top crossbar.

Bright material.

Large label:

SAFE

Below:

+$500

or:

+25%

Possible checkmark or shield symbol.

On crossing:

green pulse.

positive sound.

green particles.

bankroll increases immediately.

Small positive camera impulse optional.

---

# 22. RISK GATES

Physical red/pink gate.

Structure similar to SAFE gate.

Text example:

RISK

×5

30% WIN

Probability text:

yellow.

Use subtle red sparks.

Gate can gently pulse.

It should feel dangerous but tempting.

---

# 23. RISK RESOLUTION

Do not instantly resolve without feedback.

Sequence:

1. Player crosses gate.
2. Forward motion slightly slows.
3. Background audio reduces slightly.
4. Probability becomes emphasized.
5. Suspense ticking begins.
6. Short spinner / meter / animated indicator.
7. Result resolves.

Target suspense duration:

0.8 to 1.5 seconds.

WIN:

green/gold flash.

Text:

RISK WON!

×5

Money counts upward.

Character jumps.

Particles explode.

Positive haptic.

LOSS:

red flash.

Text:

RISK LOST!

-$500

or:

-25%

Character stumbles.

Money counts downward.

Red particles.

Loss sound.

Then normal running resumes quickly.

---

# 24. FAIR RANDOMNESS

If gate says:

30% WIN

actual probability must be 30%.

Do not manipulate probability secretly.

No fake near misses.

No probability changes based on:

ads

purchases

player wealth

previous outcomes

retention.

If a power-up changes probability, the displayed probability must update.

---

# 25. BASIC GATE EXAMPLES

Early:

SAFE +$50
vs
RISK ×1.5 — 70%.

SAFE +$100
vs
RISK ×2 — 60%.

Later:

SAFE +$5K
vs
RISK ×4 — 45%.

SAFE +$20K
vs
RISK ×6 — 30%.

Very late:

SAFE +5%
vs
RISK ×5 — 20%.

Use percentage rewards increasingly at high wealth levels.

---

# 26. ADDITIONAL GATE TYPES

Support:

Safe

Risk

AddMoney

SubtractMoney

Multiplier

Tax

Investment

Business

Market

Insurance

Shield

Luck

Mystery

Jackpot

DoubleOrNothing

CashOut

RiskChain

Bonus

KeyGate.

Gate configuration must be data-driven.

---

# 27. TAX

Red gate or obstacle.

Examples:

TAX

-$500

or later:

TAX

-25%

Prefer avoidable placement.

On hit:

red money loss.

receipt/paper particles optional.

---

# 28. POLICE OBSTACLE

Cartoon police barrier.

Blue/white.

No realistic violence.

No weapons required.

Possible penalty:

-$500

-10%

Player can dodge.

On collision:

small red/blue flash.

short siren sound.

stumble.

money loss.

---

# 29. THIEF

Small stylized NPC.

Possible appearance:

dark hoodie.

cartoon mask.

money bag.

Crosses track.

Collision steals money.

No combat.

Player simply dodges.

---

# 30. MOVING OBSTACLES

Examples:

Sliding police barricade.

Rotating arm.

Moving tax wall.

Swinging sign.

Closing gate.

Rolling safe.

Traffic barrier.

Movement must be predictable.

Early:

slow.

Later:

moderate.

Avoid frustrating twitch gameplay.

---

# 31. BUSINESS GATES

Example:

BUY BUSINESS

-$2K

After purchase:

business may generate additional income during current run or later segments.

Keep mechanic simple initially.

---

# 32. INVESTMENT

Example:

INVEST

-$5K

Several seconds later:

investment resolves.

Possible return:

+$8K

+$12K

or loss according to clearly defined configuration.

Do not overuse.

---

# 33. INSURANCE

Gate:

INSURANCE

-$500

Effect:

protect next negative obstacle or reduce next loss.

Visual:

blue shield.

---

# 34. LUCK BOOST

Temporary buff.

Example:

LUCK +10%

Next RISK gate:

base 30%

becomes:

40%.

Displayed gate must show 40%.

---

# 35. POWER-UPS

Possible:

SHIELD

MAGNET

LUCK

DOUBLE CASH

SLOW MOTION.

Visuals:

Shield:
transparent blue sphere.

Magnet:
red/blue magnet.

Luck:
gold star/clover.

Double:
green ×2.

Slow Motion:
blue clock.

Power-ups are earned through gameplay.

---

# 36. MYSTERY GATE

Purple gate.

Text:

?

Possible free in-game results:

positive cash

multiplier

small loss

coin reward

power-up

bonus route.

Do not connect mystery rewards to real money.

---

# 37. JACKPOT GATE

Rare.

Large.

Golden.

Text:

JACKPOT

×10

10% WIN

Visual:

gold particles.

rotating coins.

glow.

special sound.

Strong suspense sequence.

On success:

major celebration.

---

# 38. RISK CHAINS

Occasional feature.

Example:

First risk:

×2
70%.

After win:

TAKE $5K

or

CONTINUE

×2
50%.

After another win:

TAKE $10K

or:

CONTINUE

×3
30%.

Temporary chain reward is separate until cash-out.

Player may stop safely.

Use occasionally.

---

# 39. COMPLEX RISK OUTCOMES

Later gates may support multiple possible outcomes.

Example:

RISK

20% ×5

50% +20%

30% -25%

If multiple probabilities exist:

display them clearly.

Do not hide important outcomes.

---

# 40. CHARACTER WEALTH PROGRESSION

The player should visually become richer.

Example default progression:

$0:

basic clothes.

$1K:

small cash bag.

$10K:

better sneakers/accessory.

$100K:

watch.

$1M:

gold chain / premium details.

$10M:

more premium outfit details.

$1B:

gold aura.

$1T:

legendary visual effect.

Equipped cosmetic skins remain dominant.

---

# 41. TRACK CASH TRAIL

Normal:

small dust.

Higher wealth:

occasional green money particles.

Very high wealth:

subtle gold trail.

Avoid particle spam.

---

# 42. LEVEL LENGTH

Typical level:

30 to 75 seconds.

Early levels:

shorter.

Later:

slightly longer.

Endless:

unlimited until run ends.

---

# 43. LEVEL ARCHITECTURE

Use one main Game scene where possible.

Levels should be data-driven.

Do NOT create 100 Unity scenes.

Suggested:

Boot

MainMenu

Game

Gameplay scene loads:

LevelConfig.

---

# 44. INITIAL LEVEL TARGET

Strong first release target:

50+ playable levels.

Architecture must support:

100+

500+

procedural late-game levels.

First 20 levels should receive extra handcrafted attention.

---

# 45. LEVEL PROGRESSION EXAMPLE

Level 1:

movement + cash.

Level 2:

SAFE/RISK.

Level 3:

tax.

Level 4:

moving barrier.

Level 5:

first jackpot.

Level 6:

police.

Level 7:

shield.

Level 8:

split paths.

Level 10:

first major world transition.

Level 12:

investment.

Level 15:

risk chain.

Level 20:

bonus level.

Later:

mechanics combine.

---

# 46. WORLD SYSTEM

Create multiple visual worlds.

## WORLD 1 — STREETS

Low wealth.

Simple city outskirts.

Small buildings.

Basic roads.

---

## WORLD 2 — DOWNTOWN

Larger buildings.

Traffic.

Stores.

---

## WORLD 3 — BUSINESS DISTRICT

Banks.

Office towers.

Financial signs.

---

## WORLD 4 — LUXURY DISTRICT

Mansions.

Palm trees.

Luxury props.

---

## WORLD 5 — MILLIONAIRE ISLAND

Ocean.

White architecture.

Yachts in background.

---

## WORLD 6 — FINANCIAL CAPITAL

Skyscrapers.

Large financial buildings.

Premium roads.

---

## WORLD 7 — BILLIONAIRE BAY

Gold accents.

Very luxurious environment.

---

## WORLD 8 — FUTURE CITY

Neon accents.

Advanced architecture.

---

## WORLD 9 — GOLD CITY

High-level wealth environment.

Large golden elements.

---

Architecture must support more worlds later.

---

# 47. ENVIRONMENT OBJECTS

Use simple mobile-friendly geometry.

Possible:

trees

clouds

houses

offices

banks

shops

cars

palm trees

mansions

skyscrapers

yachts

private jets in background

vaults

billboards

gold statues.

Gameplay objects must remain more visually important than decoration.

---

# 48. FINISH AREA

Each normal level ends with a strong visual finish.

Use:

FINISH gate.

Vault.

Cash.

Gold bars.

Possible vault colors:

Metal:

#566575

Highlight:

#AAB7C4

Gold:

#FFC928

Vault should feel increasingly premium in higher worlds.

---

# 49. END-OF-LEVEL SECTION

Optional short reward runway.

Can use:

×1

×1.2

×1.5

×2

etc.

Do not let this become the game's main mechanic.

SAFE/RISK remains the core identity.

---

# 50. $1,000,000 MILESTONE

When player reaches $1M for the first time:

Do NOT end game.

Trigger major milestone sequence.

Sequence:

slow gameplay.

gold screen flash.

large text:

MILLIONAIRE!

YOU REACHED

$1,000,000

Vault opens.

Cash explosion.

Gold coins.

Character celebrates.

Confetti.

Camera pulls back.

Strong sound.

Haptic.

Then:

NEW WORLD UNLOCKED

and gameplay continues.

Possible next displayed objective:

$10,000,000.

---

# 51. WEALTH RANK SYSTEM

Example:

BROKE

$0+

HUSTLER

$1K+

EARNER

$10K+

BIG SPENDER

$100K+

MILLIONAIRE

$1M+

MULTI-MILLIONAIRE

$10M+

TYCOON

$100M+

BILLIONAIRE

$1B+

MOGUL

$10B+

EMPIRE BUILDER

$100B+

TRILLIONAIRE

$1T+

LEGEND

$100T+

Architecture must support more.

Each major rank should unlock something.

---

# 52. PROGRESS DISPLAY

Do not show progress permanently only toward $1M after the player passes it.

Progress target should evolve.

Examples:

Current target:

$1K

then:

$10K

then:

$100K

then:

$1M

then:

$10M

then:

$100M

then:

$1B

etc.

UI:

$240K / $1M.

After millionaire:

$2.3M / $10M.

---

# 53. PLAYER XP

Separate from money.

XP earned from:

completing levels.

missions.

achievements.

special challenges.

risk performance.

Player Level should unlock:

skins.

features.

worlds.

effects.

---

# 54. COINS

Separate persistent cosmetic currency.

Do NOT confuse with bankroll.

Coins purchase:

skins

trails

effects

victory animations.

Earn coins from:

levels.

missions.

achievements.

special pickups.

rewarded ads.

bonus levels.

---

# 55. SHOP OVERHAUL

Shop sections:

CHARACTERS

OUTFITS

TRAILS

MONEY EFFECTS

VICTORY ANIMATIONS

VAULT STYLES

GATE EFFECTS.

Shop must have:

3D character preview.

current equipped item.

BUY.

EQUIP.

OWNED.

LOCKED.

unlock requirement.

---

# 56. INITIAL CHARACTER SKINS

Create at least approximately 10 initial visual styles.

## DEFAULT

Blue shirt.

## STREET

Hoodie.

Sneakers.

## BUSINESSMAN

Suit.

Tie.

## BUSINESSWOMAN

Professional outfit.

## SPORT

Athletic clothing.

## GOLD RUNNER

Gold outfit.

## MILLIONAIRE

White outfit.

Gold accessories.

## BILLIONAIRE

Black suit.

Gold tie.

## NEON

Futuristic colors.

## KING OF CASH

Stylized crown.

Gold clothing.

Avoid copyrighted designs.

---

# 57. MODULAR CHARACTER ARCHITECTURE

Where practical, support:

Head

Hair / Hat

Top

Bottom

Shoes

Accessory.

Initial implementation may use full skins, but architecture should allow modular cosmetics later.

---

# 58. TRAILS

Examples:

Default Dust

Cash Trail

Coin Trail

Gold Trail

Sparkle Trail

Neon Trail

Rainbow Trail

Millionaire Trail.

Cosmetic only.

---

# 59. MONEY EFFECTS

Examples:

Classic Green Cash

Gold Cash

Neon Cash

Blue Digital Cash

Rainbow Cash

Spark Cash.

---

# 60. VICTORY ANIMATIONS

Examples:

Jump.

Dance.

Money Rain.

Confetti.

Backflip.

Golden Pose.

Cash Tornado.

---

# 61. COSMETIC RARITY

Optional:

COMMON

RARE

EPIC

LEGENDARY.

Rarity must not provide gameplay power.

---

# 62. COLLECTION PROGRESS

Show:

CHARACTERS
6 / 20

TRAILS
4 / 15

EFFECTS
3 / 10.

Locked items may appear as silhouettes.

---

# 63. MISSIONS

Create mission system.

Examples:

Collect 50 cash stacks.

Choose SAFE 10 times.

Win 3 RISK gates.

Avoid 10 obstacles.

Complete 5 levels.

Earn $10K in one level.

Reach $1M.

Collect 20 coins.

Finish a level without collision.

Win a 25% risk.

Rewards:

coins

XP

cosmetics

keys.

---

# 64. DAILY MISSIONS

Create 3 daily missions.

Examples:

Complete 3 levels.

Collect 100 cash pickups.

Win 2 RISK gates.

Avoid 20 obstacles.

Daily missions are optional.

Do not punish player for missing days.

---

# 65. ACHIEVEMENTS

Examples:

FIRST $1K

FIRST $10K

FIRST $100K

MILLIONAIRE

BILLIONAIRE

TRILLIONAIRE

RISK TAKER

SAFE PLAYER

LUCKY

UNTOUCHABLE

COLLECTOR

BIG WIN

PERFECT RUN.

---

# 66. PROFILE / STATISTICS

Display:

Player Level.

Current Wealth.

Highest Wealth.

Wealth Rank.

Levels Completed.

Risk Gates Chosen.

Risk Wins.

Risk Win Rate.

Safe Gates Chosen.

Cash Collected.

Total Money Earned.

Highest Single Reward.

Coins Earned.

Skins Owned.

Achievements.

Best Endless Distance.

---

# 67. GAMEPLAY COMBO

Consecutive cash collections may build combo.

Example:

5 CASH COMBO

10 CASH COMBO

20 CASH COMBO.

Combo can slightly increase:

score

coins

XP.

Do not let combo destroy money economy.

---

# 68. LEVEL SCORE

Possible factors:

cash collected.

obstacles avoided.

perfect sections.

risk choices.

completion.

Use:

3 stars

or:

Bronze / Silver / Gold.

Allow replaying completed levels.

---

# 69. LEVEL REPLAY

Player can replay previous levels.

Reasons:

better score.

missions.

collectibles.

cosmetics.

Do not allow economy exploits.

---

# 70. SPECIAL COLLECTIBLES

Possible:

Vault Key

Diamond

Golden Ticket

Special Coin.

Use them for optional bonus content.

---

# 71. VAULT KEYS

Example:

Collect 3 keys.

Unlock:

BONUS VAULT.

Possible free in-game rewards:

coins.

cosmetics.

bankroll bonus.

trail.

effect.

---

# 72. BONUS LEVELS

Examples:

CASH RUSH

Collect maximum cash.

JACKPOT RUN

Many risk decisions.

TAX ESCAPE

Avoid taxes.

GOLD VAULT

Collect gold.

MILLIONAIRE RUN

High-end environment.

---

# 73. ENDLESS MODE

Unlock after progression milestone.

Endless Mode:

procedurally selects track segments.

continues until failure or player exits.

Tracks:

distance.

money gained.

best combo.

best distance.

Difficulty increases gradually.

Do not make movement speed absurd.

---

# 74. PROCEDURAL LEVEL SYSTEM

Create reusable segment library:

EasyCashSegment

SafeRiskSegment

ObstacleSegment

SplitPathSegment

InvestmentSegment

JackpotSegment

KeySegment

BonusSegment

FinishSegment.

Generated level parameters:

difficulty

world

cash density

obstacle density

risk intensity

gate complexity

branch frequency

special event chance.

---

# 75. PROCEDURAL VALIDATION

Never generate:

fully blocked road.

impossible gate placement.

overlapping choices.

unavoidable bankruptcy.

impossible moving obstacle combination.

gate text hidden by another object.

Validate generated segments.

---

# 76. LEVEL SEED

Store procedural seed where useful.

Useful for bug reproduction.

Example:

Level 124

Seed 582194.

---

# 77. BRANCHING PATHS

Support temporary track splits.

Example:

LEFT:

SAFE ROUTE

more cash

less danger.

RIGHT:

RISK ROUTE

larger potential reward.

Paths merge later.

---

# 78. SECRET ROUTES

Occasionally include optional hidden-looking paths.

Possible:

gold arrow.

side ramp.

breakable wall.

Reward:

coins.

key.

special gate.

Do not make mandatory progression depend on them.

---

# 79. SPECIAL GAMEPLAY EVENTS

Possible set-piece sections:

THE TAXMAN

Multiple tax barriers.

MARKET CRASH

Protect wealth.

MEGA VAULT

Collect keys.

BANK RUN

Fast cash section.

No combat required.

---

# 80. GAME OVER

If current run bankroll reaches zero:

character stumbles.

show:

BROKE!

Buttons:

RETRY

SECOND CHANCE

MAIN MENU.

Rewarded second chance may restore:

configurable fixed value

or percentage of run start money.

Use mock rewarded ads during development.

---

# 81. ADS

Build ad architecture but do not spam.

Rewarded ads:

Second Chance.

Double Coins after level.

Optional bonus reward.

Potential future interstitial:

after several completed levels.

Never show ad:

mid-run.

before important gate.

during suspense.

during victory.

immediately after every level.

---

# 82. MAIN MENU

Bright 3D environment.

Character visible.

Possibly running/idling on preview track.

Display:

STEAL A MILLION

Current Wealth.

Wealth Rank.

Player Level.

Coins.

Buttons:

PLAY

SHOP

MISSIONS

PROFILE

SETTINGS.

Optional:

WORLD MAP.

PLAY must remain the largest button.

---

# 83. LOGO

Suggested:

STEAL
A MILLION

"STEAL":

white or dark navy.

"A MILLION":

gold.

Use subtle depth/shadow.

Readable on mobile.

---

# 84. HUD

During run:

Top Left:

LEVEL 12

Top Center:

$24.5K

Below:

progress toward current wealth milestone.

Top Right:

Pause.

Optional:

coins/key indicator.

Keep HUD minimal.

---

# 85. MILESTONE MESSAGES

Examples:

$1K:

FIRST THOUSAND!

$10K:

$10K!

$100K:

SIX FIGURES!

$1M:

MILLIONAIRE!

$1B:

BILLIONAIRE!

$1T:

TRILLIONAIRE!

Show once per first achievement.

---

# 86. VISUAL FEEDBACK

Cash:

small particles.

Safe:

green pulse.

Risk start:

suspense.

Risk win:

green/gold explosion.

Risk loss:

red pulse.

Obstacle:

stumble + small shake.

Jackpot:

large gold effect.

Finish:

confetti.

Major milestone:

large cinematic effect.

Every important action needs feedback.

---

# 87. PARTICLES

Use mobile-friendly Unity particle systems.

Cash:

small green rectangles.

Positive:

green sparks.

Negative:

red particles.

Jackpot:

gold sparkles / coins.

Finish:

confetti.

Millionaire:

gold + green + white.

Pool frequently used particles.

---

# 88. AUDIO

Required categories:

UI click.

Cash pickup.

Cash stack.

Safe gate.

Risk entry.

Suspense ticking.

Risk win.

Risk loss.

Tax.

Police.

Jackpot.

Obstacle collision.

Level finish.

Millionaire milestone.

World unlock.

Shop purchase.

Achievement.

Music.

Use copyright-safe sounds/music.

Placeholders acceptable during development.

---

# 89. HAPTICS

Cash:

light.

Gate:

medium.

Risk win:

medium/strong.

Large loss:

medium.

Jackpot:

strong.

Millionaire:

success pattern.

Respect setting.

---

# 90. LOCALIZATION

The entire game must be localization-ready.

Do NOT hardcode visible UI strings directly inside gameplay code.

Use keys.

Example:

UI_PLAY

UI_SHOP

UI_SETTINGS

UI_PROFILE

UI_MISSIONS

UI_LEVEL

GATE_SAFE

GATE_RISK

GATE_TAX

GATE_JACKPOT

RESULT_RISK_WIN

RESULT_RISK_LOSS

MILESTONE_MILLIONAIRE.

---

# 91. AUTOMATIC LANGUAGE DETECTION

On first launch:

detect device/system language.

Do not request GPS.

Do not depend on precise geographic location.

Use locale/system language.

Examples:

Russian device → Russian.

Turkish device → Turkish.

Spanish device → Spanish.

If unsupported:

fallback to English.

Player can manually override language in Settings.

Save manual preference.

---

# 92. SUPPORTED LANGUAGES

Architecture should allow any number of languages.

Initial target:

English

Russian

Turkish

Spanish

Portuguese

German

French

Italian

Polish

Ukrainian

Arabic

Hindi

Indonesian

Vietnamese

Thai

Japanese

Korean

Simplified Chinese

Traditional Chinese.

If full high-quality translation is not available:

fallback to English rather than breaking UI.

---

# 93. NO RUNTIME MACHINE TRANSLATION REQUIREMENT

Do not depend on live translation API every launch.

Preferred:

translations are prepared during development.

Stored in localization tables.

Game works offline.

Adding a new language should mostly require adding a new localization table.

---

# 94. RTL SUPPORT

Support right-to-left languages where required.

Especially Arabic.

Test:

alignment.

Text rendering.

Button layout.

Gate labels.

---

# 95. FONT SUPPORT

Use TextMeshPro.

Ensure glyph support for:

Latin.

Cyrillic.

Turkish characters.

Arabic.

CJK languages.

Use fallback font assets where required.

Never show missing-square characters.

---

# 96. LOCALIZED UI RESIZING

UI must handle long translations.

Especially:

German.

Russian.

Turkish.

Arabic.

Use:

auto sizing.

flexible layout.

reasonable wrapping.

Do not assume English text length.

---

# 97. SETTINGS

Settings screen:

Sound.

Music.

Vibration.

Language.

Graphics quality if needed.

Reset progress.

Privacy / legal placeholder if required.

Language:

AUTO.

English.

Русский.

Türkçe.

etc.

---

# 98. SAVE SYSTEM

Save:

SaveVersion.

CurrentMoney.

HighestMoney.

LifetimeMoneyEarned.

CurrentLevel.

HighestUnlockedLevel.

CurrentWorld.

UnlockedWorlds.

PlayerXP.

PlayerLevel.

Coins.

OwnedCharacters.

SelectedCharacter.

OwnedOutfits.

OwnedTrails.

SelectedTrail.

OwnedEffects.

SelectedEffect.

VictoryAnimations.

SelectedVictoryAnimation.

Achievements.

MissionProgress.

DailyMissionData.

CollectedKeys.

TutorialFlags.

Sound.

Music.

Vibration.

Language.

BestEndlessDistance.

Statistics.

---

# 99. SAVE MIGRATION

Existing project may already contain older save data.

Do not crash if fields are missing.

Implement:

SaveVersion.

Example:

saveVersion = 2.

Provide defaults for new fields.

Do not silently destroy previous save unless necessary.

---

# 100. CLOUD-SAVE PREPARATION

Backend is NOT required for MVP.

Use local save.

Keep architecture modular enough that cloud save could be added later.

---

# 101. ANALYTICS ABSTRACTION

Create/use AnalyticsManager.

Events:

game_started

level_started

level_completed

cash_collected

safe_selected

risk_selected

risk_won

risk_lost

obstacle_hit

tax_hit

police_hit

jackpot_entered

jackpot_won

world_unlocked

wealth_milestone

skin_unlocked

skin_equipped

mission_completed

achievement_unlocked

powerup_used

bonus_started

bonus_completed

endless_started

endless_ended

rewarded_started

rewarded_completed

language_changed.

No heavy external SDK required unless already configured.

---

# 102. DATA-DRIVEN ARCHITECTURE

Use:

ScriptableObjects

serializable configs

JSON where useful

prefabs.

Avoid hardcoding large amounts of content in scripts.

---

# 103. RECOMMENDED MANAGERS

GameManager

GameStateManager

RunnerController

CameraController

MoneyManager

BigNumberManager / BigNumber utility

GateManager

RiskManager

LevelManager

TrackManager

ProceduralLevelGenerator

ObstacleManager

CollectibleManager

CurrencyManager

ProgressionManager

WealthRankManager

PlayerLevelManager

MissionManager

AchievementManager

WorldManager

ShopManager

CharacterVisualManager

PowerUpManager

SaveManager

LocalizationManager

UIManager

AudioManager

HapticsManager

ParticleManager

AdManager

AnalyticsManager

TutorialManager

DebugManager.

Do not create pointless duplicate singletons if existing architecture already solves this.

---

# 104. GATE DATA

Suggested fields:

Id

GateType

TitleLocalizationKey

Value

PercentageValue

Probability

SuccessMultiplier

FailureType

FailureValue

ColorTheme

VFXType

SFXType

MinimumWealthTier

MinimumLevel.

---

# 105. LEVEL DATA

Suggested fields:

LevelNumber

World

StartMoneyRule

ForwardSpeed

Difficulty

TrackSegments

AllowedGateTypes

AllowedObstacleTypes

CashDensity

CoinDensity

KeyChance

RiskIntensity

FinishReward

EnvironmentTheme

Seed.

---

# 106. BALANCE CONFIG

Create centralized balance data.

Possible fields:

StartMoney.

CashRewardCurve.

SafeRewardCurve.

RiskRewardCurve.

ObstaclePenaltyCurve.

LevelXP.

LevelCoinReward.

WorldUnlockThresholds.

PowerUpDuration.

MissionRewards.

WealthMilestones.

AdRewards.

DifficultyScale.

Everything important should be tunable without rewriting code.

---

# 107. PERFORMANCE

Target:

60 FPS on reasonable Android devices.

Stable 30 FPS on weaker supported devices.

Avoid:

heavy post-processing.

very high polygon models.

huge textures.

constant allocations.

excessive Update loops.

hundreds of simultaneous particles.

complex MeshColliders.

unnecessary rigidbodies.

frequent Instantiate/Destroy.

Use pooling where useful.

---

# 108. LIGHTING

Simple mobile-friendly lighting.

Directional light.

Bright ambient light.

Soft shadows where practical.

Avoid expensive real-time lights.

Optional subtle bloom only if performance remains good.

---

# 109. MATERIALS

Shared reusable materials.

Example:

MAT_Track

MAT_TrackSide

MAT_SafeGreen

MAT_RiskRed

MAT_Cash

MAT_Gold

MAT_PlayerBlue

MAT_PlayerPants

MAT_Skin

MAT_TaxRed

MAT_PoliceBlue

MAT_MysteryPurple

MAT_Vault

MAT_White

MAT_WorldGrass.

Avoid creating unique material instance for every object unnecessarily.

---

# 110. COLLIDERS

Use simple colliders.

Player:

CapsuleCollider.

Cash:

Trigger box/sphere.

Gate:

Trigger box.

Obstacle:

BoxCollider / CapsuleCollider.

Avoid MeshCollider unless needed.

---

# 111. SAFE AREA

Support:

notches.

camera cutouts.

rounded screens.

All important HUD elements must remain within Safe Area.

---

# 112. ANDROID CONFIG

Platform:

Android.

Orientation:

Portrait.

Package:

com.sulik.stealamillion

Architecture:

ARM64.

Development:

APK.

Google Play:

AAB.

Use installed Unity Android:

SDK.

NDK.

OpenJDK.

---

# 113. INPUT

Touch must work on Android.

Mouse drag must work in editor.

Do not require keyboard during gameplay.

---

# 114. FIRST-LAUNCH EXPERIENCE

First launch:

automatically detect language.

Main menu.

PLAY.

One tap.

Level begins.

Within first 10 seconds player should:

move.

collect cash.

see money rise.

make a choice.

Tutorial should be visual and minimal.

---

# 115. TUTORIAL

First level:

animated finger.

Text:

SWIPE TO MOVE.

After player moves:

hide.

Before first SAFE/RISK:

CHOOSE YOUR PATH.

Do not show long tutorials.

---

# 116. FIRST 5 LEVELS

## LEVEL 1

Start approximately:

$0 or $100.

Simple cash line.

No dangerous obstacles.

Teach movement.

First small SAFE/RISK near end.

---

## LEVEL 2

Introduce:

TAX.

Avoidable.

---

## LEVEL 3

Introduce:

slow police barrier.

---

## LEVEL 4

Introduce:

simple multiplier.

---

## LEVEL 5

Introduce:

first small Jackpot gate.

Do not give huge money yet.

---

# 117. FIRST HOUR EXPERIENCE

Within first few minutes:

basic controls.

money collecting.

SAFE/RISK.

First cosmetic reward.

Within approximately 15 minutes:

more gate types.

obstacles.

mission progress.

Within approximately 30 minutes:

new environment/mechanic.

Within first longer session:

clear long-term targets.

Do not unlock everything instantly.

---

# 118. RETENTION PRINCIPLE

Create long-term interest through:

progression.

mastery.

collections.

world unlocks.

cosmetics.

missions.

achievements.

varied levels.

new mechanics.

Do NOT depend on:

unfair outcomes.

fake probabilities.

forced ads.

punishing login streaks.

artificial frustration.

---

# 119. SHORT-FORM VIDEO READABILITY

The game should look understandable in TikTok / Shorts.

Viewer should instantly see:

money.

character.

upcoming gates.

SAFE reward.

RISK probability.

large wins/losses.

Examples of naturally interesting situations:

$900K and a 20% ×5 gate.

SAFE +$100K versus RISK ×10.

One gate before millionaire.

10% jackpot.

Do not fake outcomes solely for marketing.

---

# 120. GAME ICON

Concept:

simple high-contrast icon.

Possible:

dark blue / green background.

Large gold:

$1M

Small runner silhouette.

Readable at tiny size.

---

# 121. SPLASH SCREEN

Short.

Bright.

STEAL A MILLION.

Gold money motif.

Do not make splash too long.

---

# 122. SOURCE CONTROL

Version 1 should remain preserved in Git.

Do not rewrite Git history.

Continue V2 development inside same project.

Logical commits recommended.

Examples:

runner core

safe-risk gates

world system

economy

shop

localization

levels

android polish.

---

# 123. REUSE FROM V1

Before replacing code, inspect current implementation.

Possible reusable systems:

MoneyManager.

Save system.

Audio.

Haptics.

Ad architecture.

Analytics.

Shop architecture.

Settings.

Probability logic.

Localization if present.

UI utilities.

Do not delete functional reusable systems without reason.

---

# 124. REMOVE / REFACTOR V1 PRIMARY GAMEPLAY

Version 1's card/button-based gameplay must NOT remain the main gameplay.

Old:

static screen

SAFE button

RISK button.

New:

physical runner.

physical path choice.

physical gates.

moving character.

world progression.

The underlying decision logic may be reused.

---

# 125. DEBUG TOOLS

Editor/development build only.

Provide controls:

Add $100.

Add $1K.

Set $100K.

Set $1M.

Set $1B.

Set $1T.

Add Coins.

Add XP.

Unlock World.

Unlock All Worlds.

Unlock All Skins.

Force Risk Win.

Force Risk Loss.

Complete Mission.

Complete Achievement.

Start Bonus Level.

Start Endless.

Skip Level.

Restart.

Reset Save.

Show Colliders.

Production builds must not expose debug tools.

---

# 126. PROBABILITY QA

Create simulation tests.

For example:

100,000 outcomes.

30% gate should converge close to 30%.

Test:

10%.

25%.

30%.

50%.

70%.

90%.

---

# 127. LARGE NUMBER QA

Test:

$0.

$999.

$1K.

$999K.

$1M.

$999M.

$1B.

$1T.

$1Qa.

very large values.

Verify:

display.

comparison.

multiplication.

saving.

loading.

no overflow.

---

# 128. LOCALIZATION QA

Test at minimum:

English.

Russian.

Turkish.

German.

Arabic.

Chinese.

Test:

buttons.

gate labels.

shop.

missions.

settings.

profile.

RTL.

fonts.

long text.

---

# 129. GAMEPLAY QA

Test:

horizontal movement.

track boundaries.

cash collection.

gate trigger once only.

SAFE.

RISK win.

RISK loss.

risk probability.

tax.

police.

thief.

moving obstacle.

shield.

luck.

investment.

jackpot.

finish.

world unlock.

millionaire milestone.

billionaire milestone.

game over.

second chance.

saving.

resume.

pause.

Android back button.

touch controls.

different phone aspect ratios.

---

# 130. CONTENT TARGET FOR STRONG RELEASE

Target approximately:

50+ levels.

5+ worlds initially.

10+ character skins.

5+ trails.

5+ money effects.

5+ victory animations.

10+ gate types.

10+ obstacle types.

30+ missions.

20+ achievements.

bonus levels.

endless mode.

The architecture must support much more later.

---

# 131. DEVELOPMENT PRIORITY

Priority 1:

Runner movement must feel good.

Priority 2:

SAFE/RISK must feel fun.

Priority 3:

Money economy must feel meaningful.

Priority 4:

Visual feedback must feel satisfying.

Priority 5:

First 20 levels must be strong.

Priority 6:

Progression systems.

Priority 7:

Shop/customization.

Priority 8:

More content.

Priority 9:

Procedural/endless expansion.

Do not sacrifice quality just to claim huge level counts.

---

# 132. DEVELOPMENT PHASE 1

Create playable runner core.

Required:

player.

track.

camera.

forward motion.

swipe movement.

cash pickup.

money display.

finish.

One playable level.

---

# 133. DEVELOPMENT PHASE 2

Implement identity.

Required:

SAFE gates.

RISK gates.

real probabilities.

risk suspense.

money animations.

win/loss feedback.

---

# 134. DEVELOPMENT PHASE 3

Implement obstacles.

Tax.

Police.

Moving obstacles.

Thief.

Jackpot.

Power-up basics.

---

# 135. DEVELOPMENT PHASE 4

Implement progression.

Wealth ranks.

Milestones.

Player XP.

Coins.

World unlocks.

Save expansion.

---

# 136. DEVELOPMENT PHASE 5

Shop.

Characters.

Trails.

Effects.

Victory animations.

Preview.

Equip system.

---

# 137. DEVELOPMENT PHASE 6

Missions.

Daily missions.

Achievements.

Profile/statistics.

Collections.

---

# 138. DEVELOPMENT PHASE 7

Expand content.

20 handcrafted high-quality levels.

Then 50+ total.

Add world variety.

Then procedural infrastructure.

---

# 139. DEVELOPMENT PHASE 8

Endless Mode.

Procedural segments.

Difficulty director.

Validation.

Seeds.

---

# 140. DEVELOPMENT PHASE 9

Localization.

Automatic locale.

Manual language selection.

Font fallbacks.

RTL.

UI resizing.

---

# 141. DEVELOPMENT PHASE 10

Android optimization.

Safe Area.

Touch.

ARM64.

APK.

AAB.

Real-device testing.

---

# 142. QUALITY BAR

Before calling V2 finished, confirm:

The game is actually a runner.

The character visibly runs.

Player can swipe.

Money collection feels good.

SAFE and RISK are instantly readable.

Risk probability is fair.

Money growth is slow enough early.

$1M feels important.

$1M does not end the game.

Progression continues far beyond $1M.

Shop feels useful.

Skins can be equipped.

Worlds change visually.

There are long-term goals.

Game supports localization.

First levels are polished.

Game can generate long-term content.

Android build works.

No critical console errors.

---

# 143. FINAL VISUAL TARGET

When gameplay starts:

The player sees a bright blue sky.

A clean elevated road stretches forward.

A colorful stylized runner moves automatically.

Above the character:

$245

Green cash pickups rotate on the road.

The player swipes.

+$10

+$25

+$50

The amount rises gradually.

Ahead, the road divides.

Left gate:

SAFE
+$100

Bright green.

Right gate:

RISK
×2
60%

Bright red/pink.

The player moves right.

The character enters RISK.

Movement slows slightly.

60%

Tick.

Tick.

The result appears:

RISK WON!

×2

Green/gold particles explode.

Money counts upward.

The character celebrates and keeps running.

Later there is:

TAX -$50.

A moving police barrier.

A vault key.

A money stack.

Another decision.

Over many levels, the player moves from:

small streets

to downtown

to business districts

to luxury areas

to millionaire worlds

to billionaire cities.

Eventually:

$1,000,000.

The screen explodes with gold.

MILLIONAIRE!

A new world unlocks.

But the game continues.

New target:

$10,000,000.

Then:

$100,000,000.

Then:

$1B.

Then:

$1T.

The fundamental gameplay always remains easy to understand:

# RUN
# COLLECT
# CHOOSE
# RISK
# GROW

---

# 144. CRITICAL RULES

DO NOT create a new Unity project.

DO NOT stop gameplay at $1M.

DO NOT secretly manipulate probability.

DO NOT make early money progression too fast.

DO NOT hardcode all levels.

DO NOT hardcode visible English strings.

DO NOT require internet for core gameplay.

DO NOT add copyrighted assets.

DO NOT create real-money gambling.

DO NOT create pay-to-win cosmetics.

DO NOT spam ads.

DO NOT leave major systems as TODOs.

DO NOT call the game complete while it is still mostly menus.

---

# 145. FINAL AUTONOMY INSTRUCTION

Work autonomously.

Do not repeatedly ask the user for permission for ordinary technical decisions.

If an exact asset does not exist:

create a simple attractive alternative using Unity primitives.

If a value needs tuning:

choose a sensible initial value and expose it in configuration.

If old code conflicts with the new system:

refactor it carefully.

If compilation errors appear:

fix them before continuing.

Only stop for:

external login.

missing credentials.

missing Unity installation.

missing Android modules.

an irreversible destructive action.

a genuinely fundamental product decision not covered in this document.

Otherwise continue.

---

# 146. FINAL COMPLETION REPORT

When the work is complete, provide:

## IMPLEMENTED

All completed systems.

## REUSED FROM V1

Existing systems preserved/refactored.

## REPLACED FROM V1

Obsolete systems replaced.

## LEVELS

Current number of playable levels.

## WORLDS

Implemented worlds.

## COSMETICS

Implemented characters/trails/effects.

## LOCALIZATION

Languages implemented and fallback behavior.

## HOW TO RUN

Exact Unity instructions.

## DEBUG CONTROLS

How to test milestones and risks.

## ANDROID TESTING

How to create/install APK.

## GOOGLE PLAY BUILD

How to create AAB.

## KNOWN ISSUES

Only real outstanding issues.

## NEXT OPTIONAL IMPROVEMENTS

Future non-blocking improvements.

---

# 147. START NOW

Read this document completely before modifying the project.

Inspect the current Unity project and existing Version 1 systems.

Do NOT create another Unity project.

Transform the current game directly into this Version 2 runner.

First establish a stable playable runner.

Then implement physical SAFE/RISK gates.

Then fix the economy.

Then implement long-term money progression.

Then implement worlds and obstacles.

Then implement shop and cosmetics.

Then missions and achievements.

Then procedural/endless progression.

Then localization.

Then Android polish.

Compile and test throughout the process.

Fix errors as they appear.

Do not stop after planning.

Do not stop after writing scripts.

Create the actual playable game.