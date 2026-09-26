# Миры, трасса, ворота и предметы

Статус: каталог к производству. Рабочие аналоги построены из примитивов. Сотни уровней собираются из общих модулей; не генерировать новый город на каждый уровень. EN-промпты дополняются стилем из [ART_DIRECTION.md](ART_DIRECTION.md).

## Трасса: точная геометрия в Blender / Unity

AI нужен для концепта окружения, не для точности стыков. Модули создаются измеряемой геометрией. Y вверх, +Z вперёд, pivot на начале модуля, длина 18 м, Z 0..18. Дорога шириной 8.4 м, светлая поверхность, без ям из-за декоративных трещин.

| Asset ID | Производство | Что требуется |
|---|---|---|
| `TRK_straight` | P0, Blender / текущий mesh | Белый верх, светло-серая боковина, длина 18, толщина около 0.5; непрерывный стык |
| `TRK_split` | P1, Blender | Два визуальных прохода шириной 3.05 с центрами X +/-2.6; разрыв декоративный, physics не менять |
| `TRK_bonus` | P1, variant | Та же геометрия, золотые боковые акценты, деньги остаются читаемыми |
| `TRK_finish` | P0, variant | Общая основа, место под сейф и празднование |
| `TRK_edge` | P1, shared mesh | Полоса у X +/-4.22, не сужает реальный проход |
| `TRK_rail_gold` | P1, gold world | Золотая рейка X +/-4.35, длина 18, высота около 0.4 |
| `TRK_rail_neon` | P1, future world | Тонкая светлая рейка X +/-4.35, не яркий стеновой экран |
| `TRK_stud` | P1, instance | Небольшой боковой маркер, общая сетка и материал |

Текущий floor collider 8.6 x 0.5 x 18, center `(0,-0.3,9)`; диапазон персонажа около X +/-3.6. Не добавлять MeshCollider каждому бордюру. Cash/Bonus/Split/Keys/Choice/Jackpot/Chain/Investment/Market/Power/Tax/Police/Moving/Thief/Finish - композиции модулей и предметов, а не пятнадцать разных дорог. В Endless повторяемость маскируется декором, не уникальными 4K текстурами.

## Подбираемые предметы

| Asset ID | Приоритет / создание | EN-промпт |
|---|---|---|
| `PROP_cash_stack` | P0, Grok + Blender или Meshy | One chunky small stack of fictional green banknotes, three broad visible layers, white paper band, rounded rectangular edges, no numbers, no faces, no currency engraving, isolated object. |
| `PROP_coin` | P1, Blender | One thick toy gold coin, beveled circular rim, simple embossed four-point sparkle symbol, no denomination, no real-world currency design. |
| `PROP_key` | P1, Blender | One oversized gold toy key, rounded head and two simple teeth, strong silhouette, no lettering. |
| `PROP_gold_bar` | P1, Blender | One warm gold ingot, broad beveled edges, satin highlight, no stamped lettering, simple trapezoid profile. |
| `PROP_diamond` | P2, не текущая валюта | One cyan toy gemstone with few broad facets, solid opaque material, no complicated glass refraction. |
| `PROP_golden_ticket` | P2, не подключён | One rounded golden ticket with simple corner notches, blank center, no printed words or prices. |

Пачка: ориентир 0.84 x 0.42 м в плане; trigger остаётся 0.95 x 1.4 x 0.9 с center Y 0.4. Подлёт/магнит/вращение уже программные, не запекать их в модель. Деньги и cosmetic coins различаются формой и цветом.

## Power-ups

Пять runtime enum ID из `RunnerData.cs`. Предмет около 0.7-0.9 м, крупный силуэт, общий атлас. Новые визуалы требуют mapping, но не новой логики бонуса.

| Runtime ID / Asset ID | EN-промпт | UI-символ |
|---|---|---|
| `Shield` / `PWR_shield` | Chunky blue toy shield with a white central check-shaped inset, broad beveled edges, no text. | Щит |
| `Magnet` / `PWR_magnet` | Chunky red horseshoe magnet with two blue tips, clean single object, no sparks attached. | Подкова |
| `Luck` / `PWR_luck` | Rounded green four-leaf clover with a small gold rim, simple solid form, no text. | Четыре листа |
| `DoubleCash` / `PWR_double_cash` | Two overlapping fictional green cash stacks with white bands, clearly separated silhouette, no numbers. | Две пачки; множитель TMP |
| `SlowMotion` / `PWR_slow_motion` | Chunky blue stopwatch with white face and two simple hands, gold top button, no numerals. | Секундомер |

Не добавлять permanent `2x`, время действия или процент удачи в BaseColor. Значения меняются игрой.

## Общий каркас ворот

`GATE_frame` P0: две стойки и верхняя панель, округлённые углы; отверстие свободно. Проще изготовить точно в Blender. Стойки у X +/-1.52; панель около 3.36 x 2.35, центр Y 3.5; верх декора до Y 4.78. Два прохода у X +/-2. Trigger 3.85 x 2.4 x 0.8, center Y 1.2 остаётся gameplay-объектом.

Сторона, видимая приближающемуся игроку, смотрит к -Z. `Title`, `Value`, `Odds`, `Loss` остаются TMP-узлами; текущие высоты 4.3, 3.62, 2.91, 2.45. Не скрывать цифры венцом или частицами.

```text
One modular cartoon runner gateway, two thick rounded vertical posts and a broad blank overhead sign panel, unobstructed opening beneath, simple beveled geometry, no floor, no wall filling the passage, no letters, no percentages, no numbers. Front elevation, entire frame visible. Recolorable green, coral red or gold without changing its silhouette.
```

### Все 19 gate ID

P1 уже встречаются в текущих маршрутах; P0 входит в pilot; P2 есть в данных, но как отдельный gate не спавнится текущим генератором. Цвета ниже - арт-цель; раскрашивание при интеграции сверять с кодом, не менять расчёты.

| Runtime ID | Приоритет | Визуальный вариант |
|---|---|---|
| `safe` | P0 | Зелёный каркас, check crest; гарантированная сумма TMP |
| `risk` | P0 | Красно-розовый, расходящиеся стрелки, жёлтый процент |
| `addmoney` | P2 | Зелёный, символ пачки |
| `subtractmoney` | P2 | Красный, предупреждение; loss TMP |
| `multiplier` | P1 | Зелёный, восходящие ступени; множитель TMP |
| `tax` | P2 gate | Красный, квитанция; tax obstacle уже есть |
| `investment` | P1 | Синий, росток/ступенчатая диаграмма; сумма TMP |
| `business` | P1 | Синий, портфель; стоимость и возврат TMP |
| `market` | P1 | Синий, вверх/вниз стрелки, место под длинный TMP |
| `insurance` | P1 | Синий, щит с галочкой; стоимость TMP |
| `shield` | P1 | Синий, крупный щит |
| `luck` | P1 | Синий с клевером; фактический бонус TMP |
| `mystery` | P1 | Фиолетовый локальный акцент, question crest без скрытого исхода |
| `jackpot` | P1 | Золотой, лучистый crest; вероятность на контрастной панели |
| `doubleornothing` | P1 | Красный, раздвоенный символ; полная потеря и шанс TMP |
| `cashout` | P1 | Зелёный, маленький сейф; накопленная цепочка TMP |
| `riskchain` | P1 | Красный, три звена; каждый этап со своим процентом |
| `bonus` | P2 gate | Золотой, звезда; не путать с bonus mode |
| `keygate` | P2 gate | Синий/золотой, скважина; key pickup отдельный |

Общий frame + recolor + сменный crest, а не 19 платных моделей. Crest - Blender mesh или UI-символ. `?`, `+`, `x` можно вывести TMP. Jackpot multiplier зависит от уровня: не запекать `x10`. Cosmetic gate effect не меняет семантический цвет SAFE/RISK.

## Препятствия: десять типов

ID производства `OBS_<type>`. Общий prompt: `One original nonviolent cartoon runner obstacle, chunky readable proportions, isolated complete object, no road attached, no lettering or realistic brand marks.` Добавить строку.

| Runtime type | EN-добавка / форма | Движение / контракт |
|---|---|---|
| `tax` | Red tax checkpoint barrier, blank white receipt-shaped emblem, two sturdy feet. | Около 1.7 x 1.1 x 0.45, сумма не нарисована |
| `police` | Blue portable road barricade with one red and one blue rounded beacon, no official insignia. | Маячки отдельные; умеренное мерцание |
| `thief` | Использовать `NPC_thief` из каталога персонажей | Движение root остаётся у `TrackItem` |
| `barrier` | Coral and white construction barrier with broad diagonal bands and two feet. | Может двигаться вбок; прежняя physics |
| `spinner` | Low round hub with one thick horizontal padded coral rotating arm, simple base. | Размах около 2.4; код вращает root |
| `safe` | Small heavy cartoon safe, blue-gray cuboid, round gold lock, inset front door. | Около 1.25 x 1.3 x 1.25; не finish vault |
| `traffic` | Small chunky coral toy car, four dark wheels, opaque pale windows, no driver or brand. | Ширина 1.55, длина 2.5; pivot на земле |
| `wall` | Tall narrow red wall with broad blank white receipt-like stripes, simple block. | Около 1.75 x 2.6 x 0.35 |
| `sign` | Rounded diamond warning sign on a short sturdy pole, simple exclamation shape. | Плоскость около 1.15, качается root |
| `closing` | Compact security doorway with two sliding coral doors, separate panels, gray frame. | Узлы `DoorL` / `DoorR` сохранить; frame около 2.2 |

Обычный hitbox: ширина 1.65, высота 1.8, глубина 0.75, center Y 0.9; spinner ширина 2.4, traffic глубина 2.5. Это размеры collider, не автоматические bounds модели. Для closing код меняет положения створок и включение collider; цельный неразделённый door mesh непригоден.

## Финиш: четыре vault-скина

Один `VAULT_base`, отдельная дверь, петля, колесо/замок, внутренние пачки и слитки. Корпус примерно 4.8 x 4 x 2; prefab масштабируется на финише. `DoorHinge` у `(-1.6,2,-1.4)`, открывание около 0..-105 градусов Y. Проверить петлю в Unity до деталей текстуры.

| Runtime ID | Приоритет | EN-промпт |
|---|---|---|
| `vault_classic` | P0 | Rounded blue-gray toy bank vault, thick circular door, gold lock wheel, clean satin metal, no engraved text. |
| `vault_gold` | P1 material + trim | Same vault geometry, warm gold panels, pale gold edges, broad satin highlights, no mirror reflections. |
| `vault_diamond` | P1 variant | Same vault geometry, opaque icy cyan door with few large facets, no expensive transparent crystal. |
| `vault_neon` | P1 material + trim | Same vault geometry, light gray body and restrained violet luminous accent strips, bright daytime readability. |

В preview и забеге одинаковые геометрия и материалы. Не заменять открывающийся сейф красивым неподвижным mesh.

## Девять миров

Общий kit `ENV_shared`: `cloud_a`, `cloud_b`, `tree_a`, `tree_b`, `palm_a`, `planter`, `lamp`, `bench`, `bush`, `building_low_a`, `building_low_b`, `building_tall_a`, `building_tall_b`. Вариации через пропорции, фасадные модули и палитру. Облака допустимо оставить процедурными.

На мир: один Grok concept и небольшой kit. Concept не становится плоским фоном вместо 3D. Для Meshy выбрать **один** предмет из списка и изолировать его. ID: `ENV_<world>_<name>`, пример `ENV_streets_shopfront`.

| World ID | Приоритет | Предметы kit | EN-промпт концепта |
|---|---|---|---|
| `streets` | P0 | `shopfront`, `house`, `kiosk`, `crate` | Bright friendly neighborhood, low pastel peach houses, small unbranded corner shop, clean sidewalk planters, modest welcoming scale, blue sky, wide clean white runner track kept empty. |
| `downtown` | P1 | `apartment`, `cafe`, `bus_stop`, `billboard_blank` | Cheerful downtown, medium-rise blue and peach apartments, small cafe, simple bus shelter, blank advertising panel, open blue sky, white runner track with unobstructed sightlines. |
| `business` | P1 | `office`, `bank`, `plaza_planter`, `column` | Bright modern business district, pale blue office towers, stylized unbranded bank facade, geometric plaza planters, wide white track, daytime architecture, no stock-exchange text. |
| `luxury` | P1 | `mansion`, `fountain`, `hedge`, `luxury_car` | Sunny luxury district, white villas with restrained gold trims, geometric fountain, clipped hedges, unbranded luxury car, generous open space, no dark casino mood. |
| `island` | P1 | `villa`, `yacht`, `jetty`, `beach_umbrella` | Tropical wealthy island, cyan water, white modern villa, compact white yacht, palms, simple pier, gold accents, bright blue sky, white runner track fully visible. |
| `capital` | P1 | `tower`, `bank_landmark`, `plaza_statue`, `wide_steps` | Prosperous financial capital, broad blue-white towers, original abstract gold plaza statue, clean monumental bank facade, open daytime sky, no real landmarks or flags. |
| `bay` | P1 | `waterfront_house`, `superyacht`, `marina_post`, `helicopter_static` | Sunny billionaire marina, pale gold waterfront buildings, white superyacht, static unbranded helicopter on a distant pad, cyan water, white foreground runner track. |
| `future` | P1 | `tower_curved`, `skybridge`, `hover_car_static`, `energy_pylon` | Optimistic bright future city, white curved towers, opaque pale cyan window panels, restrained violet lines, background skybridge, daylight not cyberpunk night, clean white track. |
| `gold` | P1 | `palace`, `arch`, `abstract_statue`, `treasury` | Celebratory wealth city, pale gold and white palace forms, small gold arches, original abstract wealth sculpture, green landscaping, blue sky, no gold floor hiding cash or safe gates. |

Не генерировать сразу все 36 world-specific предметов: по одному landmark, общий kit, затем декор. `luxury_car` может быть вариантом traffic без trigger; superyacht - переработкой yacht. Фоновый helicopter не нуждается в rig/пилоте. Ступени не становятся новой игровой дорогой.

### Расстановка

Генератор ставит здания примерно X +/-10..15, деревья +/-6.2, яхты +/-21, облака около Y 13. Низкие здания 3-6 м, поздние 6-17 м. Проверять camera frustum и перекрытия ворот. Небо светло-голубое, туман не скрывает шанс. Вода - лёгкий материал, не видеотекстура. Свет/тени в Unity, не внутри FBX.

Пакет prop: FBX, BaseColor при необходимости, preview и вид с камеры раннера, измеренные bounds/triangles/materials, карточка происхождения. Двери/створки/вращающиеся детали - отдельные pivots. Gameplay colliders остаются в Unity, не берутся из AI mesh.

