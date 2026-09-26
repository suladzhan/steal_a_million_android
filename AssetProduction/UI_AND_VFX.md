# UI, иконки, эффекты и UI-анимации

Статус: ТЗ на замену визуалов, не готовая библиотека спрайтов. UI остаётся uGUI + TextMeshPro, а забег - настоящим 3D. Основные ссылки: [арт-правила](ART_DIRECTION.md), [импорт](UNITY_IMPORT.md).

## Что генерировать, а что собирать

Grok: concept-борд UI, предметные иконки, визуальные образцы частиц. Blender/рендер Unity: точные shop/world thumbnails из настоящих моделей. Unity: панели, кнопки, полосы, числа, проценты, локализованные подписи, все анимации интерфейса.

Нельзя использовать готовую AI-картинку всего экрана вместо UI. Нельзя вшивать в PNG деньги, цены, XP, имена рангов, SAFE/RISK, вероятности и кнопочные надписи. Нельзя генерировать шрифт вместо имеющихся Noto/TMP fallback.

## Общий промпт предметной иконки

Одна иконка на запрос, затем нормализация размеров и alpha. Сначала 1024 PNG source, export обычно 256 или 128; 64 для небольших native-symbol иконок. Это наш production target, не требование магазина приложений.

```text
One isolated original mobile game UI icon: [SUBJECT]. Bright toy-like cartoon rendering, clean broad color regions, rounded bevels, simple readable silhouette, consistent soft light from upper left, front three-quarter view, centered with generous margin. No letters, no numbers, no words, no logo, no watermark, no decorative card, no circular background badge, no cast shadow outside the object. Transparent background if supported; otherwise a plain neutral background for later clean alpha masking. Must remain recognizable at 48 pixels.
```

Перед использованием подставить SUBJECT из таблицы. Нельзя считать нарисованную шахматную сетку прозрачностью. Проверить alpha на белом, тёмном и синем фоне; вокруг краёв не должно быть белого ореола.

## Полный набор UI-символов

Простые навигационные символы рисовать точно существующим `GameArt` или в редакторе. Для них AI-картинка не даёт преимуществ. ID ниже производственные, не обязательные новые имена runtime enum.

| Группа | Asset IDs | SUBJECT / форма | Производство |
|---|---|---|---|
| Валюта | `UI_cash`, `UI_coin`, `UI_key`, `UI_xp` | fictional green cash stack; gold coin with sparkle; gold key; blue four-point star | Grok reference / render 3D |
| Прогресс | `UI_trophy`, `UI_crown`, `UI_level`, `UI_distance` | gold cup; gold crown; short staircase; dotted route with a flag | Grok reference / точная отрисовка |
| Главное меню | `UI_play`, `UI_shop`, `UI_worlds`, `UI_missions`, `UI_profile`, `UI_settings` | triangle; shopping bag; globe; checklist; person bust; gear | Простые символы, без текста |
| Навигация | `UI_back`, `UI_close`, `UI_pause`, `UI_resume`, `UI_retry`, `UI_home` | arrow; X; two bars; triangle; circular arrow; house | Точная геометрия |
| Статус | `UI_lock`, `UI_check`, `UI_info`, `UI_warning`, `UI_claim` | padlock; check; info symbol; warning triangle; gift box | Общие состояния всех экранов |
| Настройки | `UI_sound`, `UI_music`, `UI_haptics`, `UI_language` | speaker; musical note; phone with two short vibration strokes; globe | Enabled/disabled через tint или slash overlay |
| Режимы | `UI_endless`, `UI_bonus`, `UI_daily`, `UI_achievement`, `UI_collection` | infinity loop; star; calendar; medal; three overlapping tiles | Один общий стиль |
| Power-ups | `UI_shield`, `UI_magnet`, `UI_luck`, `UI_double_cash`, `UI_slow_motion` | shield; horseshoe magnet; clover; two cash stacks; stopwatch | Соответствуют 3D pickup |
| События | `UI_tax`, `UI_police`, `UI_thief`, `UI_investment`, `UI_risk_chain`, `UI_jackpot` | receipt; beacon; eye mask; growing bars; chain links; gold starburst | Без процентов и реальных эмблем |
| Прочее | `UI_vault`, `UI_rewarded`, `UI_safe`, `UI_risk` | round vault door; small video play; shield-check; branching arrows | Rewarded только добровольное действие |

`GameArt` уже имеет Vault/Coin/Pause/Back/Settings/Shield/Bolt/Check/Cross. Сначала переиспользовать, потом расширять по потребности экранов. Иконки не обязаны быть новыми texture-файлами, если уже есть точный procedural symbol.

## UI-компоненты и состояния

| Asset / компонент | Необходимые состояния | Как изготовить |
|---|---|---|
| `UI_panel` | Светлая поверхность, тонкая граница | Один 9-slice sprite 128-256 или текущий primitive |
| `UI_button_primary` | Normal / pressed / disabled / selected | Общая форма, runtime tint; иконка и TMP отдельно |
| `UI_button_secondary` | Те же состояния, без заливки либо слабая заливка | Общий style, не новый AI-рендер |
| `UI_icon_button` | Normal / pressed / disabled | Стабильный размер, знакомый символ |
| `UI_tab` | Selected / unselected / disabled | Underline/маркер, не крупная карточка |
| `UI_progress` | XP, уровень, milestone, mission | Отдельно track/fill; динамический fillAmount |
| `UI_toggle` | On/off, disabled | Thumb + track, подпись TMP |
| `UI_scroll` | Content / empty / scrollbar | Native ScrollRect + clipping, без картинки списка |
| `UI_shop_item` | Locked / available / unaffordable / owned / equipped | Thumbnail, lock/check overlay; цена TMP |
| `UI_reward_row` | In progress / ready / claimed | Общие иконки, прогресс и кнопка |
| `UI_modal` | Confirm / cancel / waiting / error | Одна панель и backdrop, не вложенные decorative cards |
| `UI_toast` | Success / warning / unlock | Короткий локализуемый текст; не закрывает выбор ворот |

Touch-target задаётся layout, не размером рисунка; ориентир 48 logical px и проверка на телефоне. Не увеличивать HUD настолько, что перекроется путь. Форма 9-slice не содержит baked текста, теней персонажа или цвета конкретной валюты.

## Все текущие экраны

Названия соответствуют методам/состояниям `UIManager`; это чек-лист coverage, не требование 16 нарисованных фонов.

| Экран | Нужные элементы | Обязательные дополнительные состояния |
|---|---|---|
| MainMenu | Название, герой/мир 3D, play, coins, навигация | Новый игрок / продолжение / Endless locked |
| Playing | Bankroll, progress, pause, active power icons | Очень большие числа, несколько power-ups |
| Suspense | Текущий шанс, короткий индикатор ожидания | Нельзя менять шанс декоративной анимацией |
| RiskResult | Win/loss, delta, краткий FX | Zero money; HUD не дублирует награду |
| Milestone | Значок, достигнутая сумма, награда | $1K/$10K/$100K/$1M и дальнейшие степени |
| Terminal | Finish/broke, итоги, replay/continue | Bonus/Endless результаты |
| PauseMenu | Resume, restart/menu, настройки | Без потери активного сохранения |
| Confirm | Вопрос, confirm/cancel | Длинный текст и RTL |
| AdWaiting | Статус ожидания, допустимый выход | Mock не маскировать под live SDK |
| Shop | Категории, preview, coins, buy/equip | Нет монет, lock, owned, equipped |
| Settings | Sound/music/haptics/language | Persisted values; без GPS |
| Languages | Auto, manual, список | Длинные названия, RTL, выбранное auto |
| MissionScreen | Missions/daily/achievements | Progress/ready/claimed, reset display |
| Profile | Rank, XP, records, collection | Новый профиль и большие суммы |
| WorldMap | 9 миров, selected/locked | Требование unlock отдельно от thumbnail |

Для всех: SafeArea, 540x960/540x1170/540x1200/720x1600, English/Russian/Turkish/Arabic/CJK, большие числа; не растягивать текст в картинку. Не зеркалить деньги/скин целиком для RTL, зеркалить нужные navigation controls. Существующие TMP fonts/fallback не заменяются AI-артом.

## Значки рангов, заданий и превью

`UI_rank_00` ... `UI_rank_11`: 12 badge variants одной формы, не 12 отдельных генераций. Начать с простого светло-синего медальона; постепенно добавить зелёный/золотой обод, одну/две/три звезды, корону и финальный лучистый венец. Названия и значения только TMP. Не объявлять верхний badge лимитом денег.

Ранги определяет `RunnerProgress.Rank`, а не файл изображения. Импорт сопоставляет индекс 0..11; thresholds не менять ради картинки.

Missions/daily/achievements используют общие metric-иконки: cash, coin, distance, safe, risk, trophy, level, collection, jackpot, shield, key. Не нужно генерировать отдельную иллюстрацию для каждого задания. Для редкого achievement можно добавить рамку/звезду, сохраняя общую семантику.

`THUMB_shop_<id>`: 38 превью, но не 38 новых AI-иллюстраций. Персонажи и сейфы рендерятся из финальной модели; outfit показан на той же базе; trail/FX/victory показаны кадром реального preview. `THUMB_world_<id>`: 9 кадров реального kit. Никаких обещаний визуала, которого в игре нет. Цель thumbnails 256x256 RGBA, одинаковые камера/свет/масштаб внутри категории.

## Текстуры частиц

8 общих исходников заменяют десятки тяжёлых видеороликов.

| Asset ID | Размер target | Форма / способ |
|---|---|---|
| `PT_soft` | 128 RGBA | Мягкая белая radial-alpha точка, без непрозрачного фона |
| `PT_spark` | 128 RGBA | Чёткая четырёхлучевая белая искра |
| `PT_ring` | 128 RGBA | Ровное белое кольцо с мягким alpha краем |
| `PT_streak` | 128 RGBA | Узкая белая полоска/капля, прозрачные края |
| `PT_confetti` | 64 RGBA | Простой белый бумажный прямоугольник, цвет runtime |
| `PT_note` | 128-256 RGBA | Фантазийная зелёная купюра без текста |
| `PT_coin` | 128 RGBA | Монета из approved render, без чисел |
| `PT_dust` | 128 RGBA | Маленькое мягкое белое облако, низкий contrast |

Точные простые формы лучше сделать в редакторе, а не через AI. Для Grok concept: `Single isolated stylized particle shape: [shape], clean alpha silhouette, no scene, no text, no background glow rectangle, no sprite sheet.` Проверить настоящий alpha; финальный additive материал не исправляет грязный фон автоматически. Не генерировать сложный flipbook, пока общий набор не проверен на Android.

## Шесть trail-косметик

| Runtime ID | Пресет | Поведение target |
|---|---|---|
| `dust` | Светлая пыль у ступней | Небольшие короткие bursts; сейчас отдельного trail burst нет |
| `cash_trail` | Редкие купюры | За ногами, исчезают быстро, не выглядят collectible |
| `coin_trail` | Маленькие золотые монеты | Низкий след, без звука/начислений |
| `gold_trail` | Золотые искры | Короткие мягкие streaks |
| `neon_trail` | Фиолетово-голубая лента | Узкая полоса, без полноэкранного bloom |
| `rainbow_trail` | Цветные короткие полоски | Цикл нескольких цветов, не строб |

## Шесть money effects

| Runtime ID | Вид при реальном событии денег |
|---|---|
| `cash_green` | Зелёные небольшие купюры + светлая искра |
| `cash_gold` | Золотой ring + короткие sparks |
| `cash_neon` | Небольшой фиолетовый ring, не меняет знак delta |
| `cash_blue` | Голубые лёгкие streaks |
| `cash_rainbow` | 4-5 разноцветных confetti, не непрозрачное облако |
| `cash_spark` | Белые маленькие искры |

Floating money delta всегда TMP и фактическая сумма; выбор эффекта не превращает loss в зелёную награду. Дорогой эффект не меняет деньги.

## Три gate effects

| Runtime ID | Пресет |
|---|---|
| `gate_classic` | Короткое цветное расширяющееся кольцо |
| `gate_spark` | Несколько светлых искр на стойках |
| `gate_confetti` | Маленькая арка confetti после прохода |

Эффект запускается событием, не particle collision. В текущем проекте многие эффекты используют общий `Burst`; уникальные production presets ещё надо интегрировать.

## Общие игровые VFX

| Asset ID | Событие / target |
|---|---|
| `FX_cash_pickup` | 0.15-0.3 s, подлёт + 3-6 частиц |
| `FX_safe_pass` | 0.25-0.4 s, зелёный ring |
| `FX_risk_suspense` | Мягкое пульсирование стоек, длительность от состояния игры |
| `FX_risk_win` | Короткий gold/green burst, не закрывает следующую полосу |
| `FX_risk_loss` | Красный ring + быстрый fade, без травмы |
| `FX_tax` | Несколько уходящих вниз notes, отрицательная delta TMP |
| `FX_police` | Короткий red/blue beacon accent, без непрерывного строба |
| `FX_thief` | Маленький puff у столкновения, без violence |
| `FX_hit` | Компактные звёздочки/пыль, не экранный flash |
| `FX_jackpot` | Золотые искры, короткий money rain |
| `FX_investment_return` | Короткая зелёная восходящая группа частиц |
| `FX_shield_break` | Синее кольцо и несколько простых осколков |
| `FX_magnet` | Тонкий ограниченный pull accent, не новая механика |
| `FX_luck` | 2-3 искры около бонуса, HUD показывает настоящий шанс |
| `FX_double_cash` | Парные зелёные sparks, multiplier TMP |
| `FX_slow_motion` | Лёгкие голубые линии, без сильного blur |
| `FX_finish` | Конфетти за персонажем, дверь сейфа не перекрыта |
| `FX_milestone` | Золотой accent с усилением для $1M, без объявления конца игры |
| `FX_world_unlock` | Небольшой burst на UI значке |
| `FX_achievement` | Ring у badge |
| `FX_level_up` | Голубая звезда/краткий ring |

Начальные бюджеты: pickup до 12 одновременно живых частиц на событие; обычный hit/gate до 24; jackpot/finish до 80 на событие. Это tuning targets, не готовые ограничения движка. После серии событий нужен измеренный общий cap/pool, иначе даже маленькие системы перегрузят fill-rate. Для слабого устройства допускается убрать второй слой и тени частиц без изменения механики.

## UI-анимации: Unity, не MP4

| Asset / motion preset | Target |
|---|---|
| `UIA_press` | 0.08 s scale 1 -> 0.96, 0.12 s обратно, layout не меняется |
| `UIA_panel` | 0.18-0.25 s fade + небольшой сдвиг |
| `UIA_tab` | 0.12-0.18 s переход маркера |
| `UIA_currency` | 0.18 s icon bump; число берётся из состояния, не копится анимацией |
| `UIA_progress` | 0.25-0.4 s fill, без изменения фактического XP |
| `UIA_reward` | 0.25 s появление, короткий hold, без двойного claim |
| `UIA_unlock` | 0.3 s lock исчезает, check появляется |
| `UIA_milestone` | 0.3 s entrance, hold по game state, безопасный exit |
| `UIA_toast` | 0.18 s in/out; очередь ограничена |
| `UIA_wait` | Ненавязчивый цикл, пауза/отмена по lifecycle |

Переходы прерываемые, input-block только на реальный modal, повторный tap не порождает вторую покупку/награду. Локализация и SafeArea не анимируются масштабированием всего Canvas. Временно отключить VFX и анимации должно быть возможно без потери UI-функций.

