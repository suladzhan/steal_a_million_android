# Персонажи, скины и анимации

Статус: производственное ТЗ; импортных моделей и клипов по нему ещё нет. P0 = первый проверяемый набор, P1 = текущий каталог V2, P2 = расширение спецификации. Общий стиль: [ART_DIRECTION.md](ART_DIRECTION.md).

Источник ID, цен и условий: [shop.json](../Assets/Resources/RunnerData/shop.json). Не менять ID после генерации. Художник не назначает новые цены или преимущества.

## Базовый персонаж

Один оригинальный взрослый мультяшный человек, около 1.8 м, крупная голова, короткие аккуратные волосы, простые выразительные глаза, компактные кисти с большим пальцем и условной общей формой остальных пальцев. Вид со спины особенно важен: его игрок видит в забеге. Разделённые ноги и руки; A-pose; ступни плоско, параллельно. Не генерировать деньги в руках базовой модели.

Цель: 6-10k треугольников, 1-2 материала, 1024 BaseColor. У всех скинов одинаковый gameplay-hitbox. Скины не влияют на скорость, шанс RISK и награду. Сначала один проверенный риг; затем одежда поверх согласованного скелета. Независимые генерации Meshy не гарантируют одинаковых костей и пропорций.

## Десять скинов

ID производства `CHAR_<runtime ID>`. Для Grok: общий блок стиля + текст строки + `Full body, relaxed symmetrical A-pose, both hands and both feet visible, empty hands, neutral expression, plain background.` Для Meshy передавать утверждённое одиночное изображение и общий блок модели.

| Runtime ID | Приоритет | Палитра / силуэт | Предметный EN-промпт |
|---|---|---|---|
| `runner` | P0 | Синяя футболка, тёмные брюки, белые кроссовки | Original friendly adult cartoon runner, cobalt blue short-sleeved shirt, dark indigo trousers, chunky white sneakers, short dark hair, clean simple face, no jewelry, no logos. |
| `street` | P1 | Коралловое худи, капюшон сзади | Same approved runner proportions, coral hoodie with lowered hood, charcoal-indigo trousers, white sneakers, no dangling cords, no graffiti or lettering. |
| `businessman` | P1 | Тёмный пиджак, золотой галстук | Same approved runner proportions, simplified dark blue business suit, short gold tie, clean black shoes, broad flat lapels, no briefcase, no brand marks. |
| `businesswoman` | P1 | Фиолетовый костюм, хвост волос | Original adult cartoon businesswoman sharing the approved runner skeleton proportions, violet jacket and dark trousers, compact tied-back ponytail, practical flat shoes, simple confident face. |
| `sport` | P1 | Мятная форма, повязка | Same approved runner proportions, mint sports shirt, dark blue athletic trousers, simple headband, white trainers, sporty but not muscular, no sports-team branding. |
| `gold_runner` | P1 | Золотая одежда, не слиток вместо человека | Same approved runner proportions, warm gold satin tracksuit with ochre secondary panels, natural skin, white-gold sneakers, broad soft highlights, no mirror chrome. |
| `millionaire` | P1 | Белый костюм, золотая цепь | Same approved runner proportions, clean white luxury suit with restrained gold trim, small chunky gold necklace, white shoes, friendly face, no real designer patterns. |
| `billionaire` | P1 | Почти чёрный костюм, золото | Same approved runner proportions, charcoal formal suit, gold tie and pocket accent, sleek shoes, broad clean shapes, restrained luxury, no sunglasses hiding the face. |
| `neon` | P1 | Бирюзовый костюм, фиолетовый визор | Same approved runner proportions, turquoise futuristic sports outfit with small violet luminous accent strips and a compact visor, solid opaque materials, no cables or cyberpunk clutter. |
| `king` | P1 | Золотая одежда, фиолетовый пояс, корона | Same approved runner proportions, gold ceremonial sports suit with violet accents, small five-point toy crown, no long cape, no scepter, free arms and legs, no historical royal insignia. |

Точные цвета существующего каталога использовать при финальной покраске в Blender/Unity. HEX в запросе не обеспечивает точный цвет.

### Одежда: не три новых персонажа

| Runtime ID | Производство | Совместимость |
|---|---|---|
| `outfit_classic` | Исходные материалы каждого скина | Сохранять палитру выбранного скина, а не красить всех в синий |
| `outfit_coral` | Маска/материал верхней одежды `#FF7769` | Не менять кожу, глаза, золото и волосы |
| `outfit_mint` | Маска/материал верхней одежды `#56D6B0` | Та же маска, брюки остаются в палитре скина |

В текущем коде покраска привязана к renderers `Shirt`, `Sleeve`, `Pants`, `Belt`. Для новой skinned mesh выделить отдельный материал верхней одежды или согласованный mask texture. Не обещать автоматическую покраску слитной текстуры без адаптера.

## Аксессуары состояния

| Asset ID | Состояние подключения | Задание / EN-промпт |
|---|---|---|
| `ACC_cash_bag` | Procedural вариант при peak >= $1K | Small rounded green cash backpack with white closure band, no text, compact silhouette, separate object with flat back. |
| `ACC_watch` | При peak >= $100K | Chunky simple gold wristwatch, broad round face without numbers, compact strap, separate accessory. |
| `ACC_chain` | При peak >= $1M или shape chain | Short chunky gold necklace with simplified connected segments, no pendant lettering, sized to the approved chest. |
| `ACC_shoes_premium` | P2, в спецификации $10K, hook пока нет | Pair of clean premium white sneakers with restrained green accents, no brand logos, matching approved feet. |
| `ACC_trim_luxury` | P2, $10M, новый visual hook | Restrained gold clothing trim on the approved outfit, broad readable edges, preserve cloth color regions. |
| `FX_wealth_aura` | P2, $1B, particle preset | Редкие золотые искры у ног, не непрозрачный купол |
| `FX_wealth_legendary` | P2, $1T, particle preset | Короткий золотой акцент при событии; не закрывать трассу |

Часы, цепь и сумка крепятся к костям/сокетам. Имена будущих сокетов: `Socket_Back`, `Socket_Wrist`, `Socket_Chest`, `Socket_Head`. Это контракт нового рига, не существующие procedural узлы. Корона, хвост и визор входят в skin geometry, их не заказывать повторно.

## NPC

`NPC_thief`, P1: один оригинальный мультяшный вор, около 85% масштаба игрока. Промпт:

```text
One original mischievous adult cartoon thief for a bright mobile runner, compact body, simple black eye mask, coral shirt, dark trousers, soft sneakers, small empty sack on the back. Friendly nonviolent stylization, empty hands, complete separated limbs, relaxed A-pose, no weapon, no real uniform, no lettering. Match the approved runner's visual proportions and material style.
```

Полиция в текущем уровне = дорожное ограждение с маячками, не полицейский персонаж. Офицер для текущей механики не нужен.

## Риг и движения

Один humanoid rig, без face rig/cloth/IK dependency в runtime. Проверить плечи, локти, кисти, таз, колени и стопы. Пальцевой риг необязателен. Корневая позиция не движет игрока; клипы in-place. Источник истины движения: `RunnerController`.

30 FPS - рабочая частота авторинга, не ограничение игры. Длительности ниже целевые; синхронизация настраивается в visual adapter. Meshy rig может уже включать walk/run: проверить выдачу до заказа платной анимации. Grok video = референс, не клип Unity.

| Asset ID | Приоритет | Длина / loop | EN-задание для референса или аниматора |
|---|---|---|---|
| `ANIM_idle` | P0 | 2 s, loop | Relaxed balanced standing, subtle breathing, small hand motion, feet planted, in place. |
| `ANIM_run` | P0 | 0.6-0.8 s, loop | Energetic readable cartoon run in place, alternating arms, clean foot contacts, small vertical bounce, no forward drift. |
| `ANIM_lean_left` | P1 | 0.2 s pose | Small left lean during lateral running, head stays readable, legs continue the run cycle. |
| `ANIM_lean_right` | P1 | 0.2 s pose | Mirrored right lean, no root translation; additive pose or code-driven visual lean. |
| `ANIM_stumble` | P1 | 0.45-0.6 s | Brief backward shoulder recoil, regain balance without falling or stopping the gameplay root. |
| `ANIM_risk_wait` | P1 | 0.8-1.2 s, loop | Tense anticipation pose, slight forward lean and held hands, no indication of win or loss. |
| `ANIM_risk_win` | P1 | 0.6-0.9 s | Quick happy fist lift and bounce, blend back to running without turning around. |
| `ANIM_risk_loss` | P1 | 0.5-0.8 s | Brief surprised shoulder drop, recover immediately, no death or uncontrolled fall. |
| `ANIM_finish_stop` | P1 | 0.35-0.6 s | Settle from running into a balanced finish stance; visual motion only, no root displacement. |
| `ANIM_game_over` | P1 | 1-1.5 s, hold | Disappointed but lighthearted pose, shoulders down, no injury or violence. |
| `ANIM_preview` | P1 | 2-3 s, loop | Calm confident standing, small wave, face toward shop camera, do not rotate the gameplay root. |
| `ANIM_thief_idle` | P1 | 1.5 s, loop | Small side-to-side look, planted feet, cheeky expression, no attack action. |

Наклоны можно сделать additive poses или кодом без платных клипов. Нельзя одновременно вращать одни кости procedural-скриптом и Animator. Нет jump/slide-кнопок: прыжок через препятствия не заказывать как новую механику.

### Шесть покупаемых побед

| Runtime ID / Asset ID | Длина | EN-задание |
|---|---|---|
| `jump` / `ANIM_victory_jump` | 1.2 s | Happy short vertical celebratory hop, arms raised, land at original position, balanced ending. |
| `dance` / `ANIM_victory_dance` | 2 s loop | Original cheerful two-step dance in place, compact arm gestures, no imitation of a named dance or person. |
| `money_rain` / `ANIM_victory_money_rain` | 1.8 s | Raise both open hands and look up happily; banknotes spawned separately as Unity particles, not embedded in the rig. |
| `backflip` / `ANIM_victory_backflip` | 1.5 s | One controlled stylized backward somersault, feet return to start, no traveling, crown and accessories stay attached. |
| `gold_pose` / `ANIM_victory_gold_pose` | 1.6 s, hold | Confident compact hero pose, hands near hips, no jump, readable gold costume silhouette. |
| `tornado` / `ANIM_victory_tornado` | 1.8 s | One playful fast spin in place, arms kept compact, settle facing the initial direction; wind particles separate. |

В текущей версии победы процедурные; отдельные импортные клипы для всех шести ещё не подключены.

## Пакет сдачи персонажа

`CHAR_runner__v001__model.fbx`, `CHAR_runner__v001__basecolor.png`, исходный GLB, `.blend` с ригом отдельно, front/back/side screenshots, wireframe, список костей, клипы с точными диапазонами кадров, карточка происхождения. Для следующего скина - подтверждение совместимого Avatar, не повторная копия всех анимаций.

Приёмка: A-pose целая; силуэт читается сзади; нет слипшихся рук/ног; нет разрывов коленей; одежда не пересекает тело; часы следуют запястью; корона не обрезается bounds; ноги не скользят; подошвы на земле; TMP над головой не перекрыт. Детали: [UNITY_IMPORT.md](UNITY_IMPORT.md), [QA_CHECKLIST.md](QA_CHECKLIST.md).

