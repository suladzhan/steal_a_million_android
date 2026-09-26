# Third-Party Notices

## Noto Fonts

Source repositories:
- https://github.com/notofonts/noto-fonts
- https://github.com/notofonts/noto-cjk

Bundled Noto Sans Bold, Noto Sans Arabic, Noto Sans CJK SC, Noto Sans Devanagari and
Noto Sans Thai are distributed under the SIL Open Font License 1.1.
Full licenses: Assets/Art/Fonts/Noto-LICENSE.txt and NotoCJK-LICENSE.txt.
Generated TextMeshPro font assets retain these bundled source fonts for dynamic
glyph population and offline fallback.

## RTLTMPro

Source: https://github.com/pnarimani/RTLTMPro
Revision: f480419bbbffed1be3c129d68cc0182afcfbcac3
License: MIT, included at Assets/ThirdParty/RTLTMPro/LICENSE.txt.

Only the runtime shaping/bidi helper code is vendored, not the replacement TMP
UI components or Editor integration. The local assembly definition references
Unity.TextMeshPro; LocalizationManager invokes RTLSupport before assigning text.

## Unity / TextMeshPro

Unity built-in primitives, PhysX CharacterController, ParticleSystem, uGUI and
TextMeshPro are used under the installed Unity/package license terms.
The package versions are recorded in Packages/packages-lock.json.

## Original Project Content

Runner geometry, environments, cash stacks, gate geometry, cosmetic combinations,
animation logic, UI layouts and synthesized audio are authored in this project.
No commercial character packs or copyrighted franchise characters are included.
