# Контракт Blender / Unity

Это план интеграции будущего арта. В этой задаче runtime-код, сцены, экономика и сохранения не меняются.

## Формат передачи

Предпочтение для Unity: FBX + отдельные PNG-текстуры. Исходник Blender хранить в `03_Blender`, а не в `Assets`. Unity использует FBX как основной формат моделей; прямой импорт `.blend` зависит от установленного DCC, поэтому экспортируем явно. [Unity 6.3: Importing a model](https://docs.unity3d.com/6000.3/Documentation/Manual/ImportingModelFiles.html).

Meshy GLB сохраняется как исходный результат. В текущем `Packages/manifest.json` нет специализированного glTF runtime-importer. Не считать GLB автоматической заменой prefab и не добавлять новый package без необходимости: обработать в Blender и экспортировать FBX.

## Масштаб и оси

- Контракт в Unity: 1 unit = 1 метр, Y вверх, +Z направление бега/лица модели, +X вправо. Blender может хранить другие оси; оценивать результат после FBX-экспорта, а не только подписи в Blender.
- Игрок: около 1.8 unit от подошвы до макушки; pivot между ступнями на земле. После нормализации transform visual-root scale `(1,1,1)`, rotation `(0,0,0)`.
- Не применять transforms к уже анимированному rig вслепую. Масштаб и оси фиксировать до skinning/анимации; затем проверять rest pose и каждый клип.
- FBX содержит только выбранный asset/rig, без камеры, света, пола из preview, скрытых дублей и экспериментальных коллекций.
- Привязки сегментов и предметов: [WORLDS_AND_PROPS.md](WORLDS_AND_PROPS.md). Проверить тестовым кубом 1 м перед большой партией.

## Геометрия и текстуры

1. Проверить силуэт спереди/сзади/сбоку, нормали и отсутствие лишних внутренних поверхностей.
2. Проверить UV, растяжения, швы на лице/плечах, bleed-padding и отсутствие задвоенных текстур.
3. Посчитать треугольники после triangulation/export. Проверить материал slots и фактически связанные texture paths.
4. Для персонажа сгибать плечи, локти, таз, колени и голеностоп. Невидимые в A-pose дефекты skin weights часто проявляются в беге.
5. Текстуры нейтральные к освещению; BaseColor отдельно от normal/roughness/metallic. Emission только для нужных accent-частей.
6. Общий atlas на мелкие props предпочтительнее уникального 2K на каждую монету. Рекомендуемые размеры в [ART_DIRECTION.md](ART_DIRECTION.md).

В Unity использовать совместимые материалы Built-in. Не переносить URP/HDRP shader в этот проект. Roughness map нельзя назначать в smoothness без преобразования: проверить каналы и инверсию по используемому shader. По умолчанию оставить простой цветной Standard-материал без normal map, если визуально она не нужна.

## Риг и Animator

Цель: один проверенный humanoid skeleton для игрока и десяти скинов. Имена, parent hierarchy, rest pose и пропорции согласованы. У NPC допустим тот же скелет с отдельным prefab; не делать skin rig независимым без необходимости.

Unity Humanoid Avatar должен корректно сопоставлять кости; animation-only FBX может использовать совместимый Avatar. Это проверяется в Rig/Configure, а не выводится из слова «rigged» в названии файла. [Unity: Humanoid animations](https://docs.unity3d.com/6000.3/Documentation/Manual/ConfiguringtheAvatar.html).

Все игровые клипы in-place. Runtime position остаётся у `RunnerController`, `Animator.applyRootMotion=false`. Без animation events, выдающих деньги, монеты, урон, завершение уровня или повторный результат RISK.

Текущее `CharacterVisual` вручную вращает `Body/ArmL/ArmR/LegL/LegR`. Подключение skinned mesh требует изолированного visual adapter: либо старый procedural путь, либо новый Animator, но не два контроллера одних transforms одновременно. `RunnerWorld` и `GameManager` сохраняют смысл команд Running/Lean/Stumble/Cheer/Preview. Это отдельная следующая инженерная задача, не уже выполненная функция.

## Безопасная иерархия будущего prefab

```text
Player                       existing gameplay owner
  CharacterController        retain existing tuning and collision behavior
  RunnerController           movement / input, unchanged
  VisualRoot                 replace only this part through a visual adapter
    Armature
    SkinnedMeshRenderer
    Accessory sockets
  Bankroll                   existing world-space TMP, never baked into mesh
```

Текущий CharacterController: height 1.9, radius 0.28, center Y 0.95. Не расширять hitbox из-за волос/короны или дорогого костюма. Skin не даёт скорости, удачи, защиты или другого преимущества.

Для ворот сохранять `TrackItem`, trigger, group/id, динамические `Title/Value/Odds/Loss`. Модель является визуалом вокруг прохода. Для pickups сохранять trigger и flying/magnet-поведение. Для препятствий сохранять размеры hitbox и траекторию; модель не должна закрывать больше пространства, чем игровая опасность.

`RunnerSetup.Configure` регенерирует рабочие сцены/prefabs. Перед интеграцией нужно изменить сам источник настройки либо отделить production visual-prefabs, чтобы следующая регенерация не затёрла импорт. Не редактировать сгенерированный prefab и считать задачу законченной.

## Порядок интеграции

1. Перед крупным импортом зафиксировать согласованную рабочую версию в Git. Не добавлять вложенный `.git` клонированного фреймворка как случайный gitlink.
2. Проверить provenance и лицензию в [ASSET_REGISTER.md](ASSET_REGISTER.md).
3. Один approved asset за раз в будущий `Assets/Art/Production/<Category>/`, prefab-визуал отдельно. Сохранить `.meta` и GUID; переносить Unity-ассеты вместе с `.meta`.
4. В inspector проверить mesh, rig, animation, materials, textures. Не импортировать демонстрационные сцены, сторонние скрипты или камеры из пакета.
5. Sprite: PNG RGBA, alpha реально присутствует, Sprite(2D and UI), правильный pivot и borders для 9-slice. Простым UI-спрайтам mipmaps обычно не нужны; world textures проверяются с mipmaps. Настройки сжатия выбирать по реальным Android устройствам, не назначать один codec всем файлам автоматически.
6. Материалы UI/particles держать сериализованными ссылками. В 0.2.0 уже были stripped shader/CapsuleCollider ошибки, поэтому сохранённые UI/Particles материалы и `Assets/link.xml` не удалять.
7. Unity compile, визуальный тест, PlayMode, ARM64 APK и настоящий телефон. При ошибке вернуть только новый visual-binding, сохранив рабочую procedural-версию.

## Что не входит в импорт арта

Изменение вероятностей, цен, наград, world unlocks, имен предметов сохранений, монетизации, языка по GPS, сетевой зависимости игры, physics/input или Android signing. Такие изменения не должны появляться «заодно» с заменой модели.
