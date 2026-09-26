# Google Play release package — 0.3.0

Current product status: visual quality rejected by the user; functional prototype,
not release-ready. This is a draft publication package. Rework art/content and
complete device acceptance first; follow NEXT_WORK_PLAN.md and
GAME_AUDIT_2026-09-26.md. Existing store graphics must be reviewed/replaced after
the new art is accepted. Technical APK passes alone do not approve publication.

## Build and technical checks

- Package: `com.sulik.stealamillion`; version 0.3.0 / code 12.
- Unity 6000.3.23f1, IL2CPP ARM64, Android target API 36.
- APK check: `Tools/Verify-Android.ps1`: signer, architecture, ZIP alignment,
  every native ELF library 16 KB alignment. APK success does not replace Play's AAB checks.
- `AndroidBuild.ReleaseApk`: non-development local review build; mock rewarded
  ads disabled while serializing the player. Uses configured signing credentials.
- `AndroidBuild.Aab`: upload build; requires publisher, support email, HTTPS
  privacy URL in `Runner/ReleaseSettings` and a custom upload keystore.
- Restore editor mock-ad setting after the build; editor tests can still exercise
  the simulated ads. No production advertising or billing SDK is integrated.

Upload keystore stays outside this repository. Set its path/alias/password in the
local Unity publishing settings or a secure build environment. Do not paste secrets
into a task, source file or console log. Google Play App Signing and the upload key
are separate concerns; the account owner must retain recovery access.

## Draft store listing

Title: **STEAL A MILLION**

Short description (73 characters):

Risk or play safe. Collect cash, grow your fortune and unlock new worlds.

Full description:

Build a fictional fortune one run at a time. Slide across the track, collect cash
and decide between a guaranteed reward and a chance at a bigger win. Every choice
can change your run.

Explore colourful districts, dodge moving obstacles and reach the finish vault.
Unlock outfits and cosmetic rewards, complete missions and improve your best runs.
The game can be played offline and supports multiple languages.

All money and rewards are fictional game values. They cannot be purchased for
cash, exchanged for money, or withdrawn. There are no real-money prizes.

## Artwork

- Launcher icon: existing game logo; legacy/adaptive/round PNG resources.
- Store icon: 512 x 512 PNG, separate from adaptive padding.
- Feature graphic: 1024 x 500 PNG, use the actual game's artwork.
- Phone screenshots: genuine Unity/game captures, portrait; do not invent mechanics
  or add misleading claims. Capture menu, active run, gate choice, finish and worlds.
- Assets and provenance: `AssetProduction/ASSET_REGISTER.md` and Major task receipts.

## Privacy / Data Safety audit for this version

Source audit: no account registration; save files stored locally; analytics is an
in-process event interface with no networking; no analytics, ads, billing or login SDK
in Packages/manifest.json. Development performance diagnostics stay on the device.

Draft declarations, requiring owner confirmation before submission:

- App does not collect or share user data with the developer/server in this build.
- No paid digital goods, subscriptions or integrated ads in the release build.
- Reset game progress in Settings; uninstall/clear app data to remove local files.
- Public privacy policy is still required. In-game text does not replace its public URL.
- Verify the final manifest and every dependency again if any SDK is added.

## Console checklist (owner actions)

1. Confirm developer/account type, store contact and distribution countries.
2. Enter publisher name, support email and public privacy URL; host final policy.
3. Configure upload signing; enable/verify Play App Signing in the Console.
4. Upload signed AAB to **internal testing**, run Play's pre-launch report.
5. Complete Data Safety, ads declaration, app access and IARC content rating.
   Answer chance-based fictional-money gameplay questions accurately; do not guess
   an age rating from screenshots.
6. For personal accounts created after 2023-11-13, check whether closed testing with
   at least 12 continuously opted-in testers for 14 days is required.
7. Review crashes/ANRs, device compatibility, install size and 16 KB report.
8. After testing and store review, approve a production release in the Console.

## Sources verified 2026-09-26

- [Target API 36](https://developer.android.com/google/play/requirements/target-sdk)
- [16 KB support](https://developer.android.com/guide/practices/page-sizes)
- [New personal account testing](https://support.google.com/googleplay/android-developer/answer/14151465)
- [Prepare a release](https://support.google.com/googleplay/android-developer/answer/9859348)
- [Data Safety](https://support.google.com/googleplay/android-developer/answer/10787469)

## Current status

Implementation complete; 10 PlayMode and 45 EditMode checks pass. See
MAJOR_DEVICE_QA.md for APK and device evidence. Account, upload signing, publisher/contact and hosted
privacy policy not yet confirmed. No production upload/publication performed.

## Meshy asset provenance

Nine models were generated from original text prompts, processed in Blender and
integrated into Unity. Task receipts and geometry reports are preserved under
AssetProduction. 300 existing credits were spent; final observed balance: 752.
The account plan at generation time still needs confirmation for the release
license record. Meshy permits commercial use; free-plan outputs require CC BY 4.0
attribution, while paid-plan ownership applies to qualifying private outputs.
Do not label paid private assets CC BY without checking the generation plan.
Suggested factual credit: "3D models created with Meshy, adapted in Blender and Unity."
If free-plan terms apply, add the required license attribution before distribution.
Sources: [Meshy terms](https://www.meshy.ai/terms-of-use),
[commercial use](https://help.meshy.ai/en/articles/16102098-can-i-use-meshy-assets-commercially).
