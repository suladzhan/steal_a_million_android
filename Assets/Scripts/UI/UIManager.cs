using System;
using System.Collections;
using StealAMillion.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StealAMillion
{
    public sealed class UIManager : MonoBehaviour
    {
        private GameManager game;
        private RectTransform stage;
        private RectTransform page;
        private RectTransform modal;
        private TextMeshProUGUI moneyLabel;
        private TextMeshProUGUI suspenseLabel;
        private TextMeshProUGUI notice;
        private Image flash;
        private CanvasGroup input;
        private readonly RectTransform[] particles = new RectTransform[54];
        private readonly Vector2[] velocities = new Vector2[54];
        private Coroutine celebration;
        private Coroutine feedback;

        public void Initialize(GameManager owner)
        {
            game = owner;
            var canvasObject = new GameObject("Game Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540, 960);
            scaler.matchWidthOrHeight = 1;
            UIFactory.Panel(canvas.transform, "Background", UIFactory.Background, 0, 0, 1, 1);
            var safe = UIFactory.Rect(canvas.transform, "Safe Area", 0, 0, 1, 1);
            safe.gameObject.AddComponent<SafeArea>();
            stage = UIFactory.Rect(safe, "Portrait Stage", .5f, .5f, 0, 0);
            stage.gameObject.AddComponent<StageLayout>();
            input = stage.gameObject.AddComponent<CanvasGroup>();
            flash = UIFactory.Panel(canvas.transform, "Feedback Flash", Color.clear, 0, 0, 1, 1);
            var particleRoot = UIFactory.Rect(canvas.transform, "Confetti", 0, 0, 1, 1);
            for (int i = 0; i < particles.Length; i++)
            {
                var p = UIFactory.Panel(particleRoot, "Confetti " + i, i % 3 == 0 ? UIFactory.Gold : i % 3 == 1 ? UIFactory.Green : UIFactory.White, .5f, .5f, 0, 0);
                particles[i] = p.rectTransform;
                particles[i].sizeDelta = new Vector2(6 + i % 4, 11 + i % 3);
                particles[i].gameObject.SetActive(false);
            }
            notice = UIFactory.Label(canvas.transform, "", 16, UIFactory.Red, .05f, .005f, .90f, .035f);
        }

        public void BlockInput(bool blocked) { input.interactable = !blocked; input.blocksRaycasts = !blocked; }
        public void Notice(string message) { if (notice != null) notice.text = message; }

        private void Clear()
        {
            if (feedback != null) StopCoroutine(feedback);
            feedback = null;
            flash.color = Color.clear;
            CloseModal();
            if (page != null) { page.gameObject.SetActive(false); Destroy(page.gameObject); }
            page = UIFactory.Rect(stage, "Page", 0, 0, 1, 1);
            moneyLabel = null;
            suspenseLabel = null;
        }

        private Action Click(Action action)
        {
            return () => { game.Click(); action(); };
        }

        private Button Command(string text, float y, Action action, Color color)
        {
            return UIFactory.Button(page, text, color, color == UIFactory.Surface ? UIFactory.White : UIFactory.Background,
                .02f, y, .96f, .078f, Click(action));
        }

        private void IconButton(Transform parent, ArtKind icon, float x, float y, Action action)
        {
            var button = UIFactory.Button(parent, "", UIFactory.Surface, UIFactory.White, x, y, .15f, .08f, Click(action));
            button.name = icon.ToString();
            UIFactory.Art(button.transform, icon, UIFactory.White, .2f, .17f, .6f, .66f);
        }

        private void Header(string title, Action back)
        {
            UIFactory.Label(page, title, 22, UIFactory.White, .02f, .92f, .8f, .06f, TextAlignmentOptions.Left);
            IconButton(page, ArtKind.Back, .84f, .92f, back);
        }

        public void MainMenu()
        {
            Clear();
            var save = game.Session.Save;
            UIFactory.Art(page, ArtKind.Coin, UIFactory.Gold, .01f, .925f, .065f, .043f);
            UIFactory.Label(page, save.coins.ToString("N0"), 21, UIFactory.Gold, .09f, .92f, .32f, .05f, TextAlignmentOptions.Left);
            UIFactory.Label(page, "BEST " + MoneyManager.Format(save.highestMoney), 16, UIFactory.Muted, .40f, .92f, .58f, .05f, TextAlignmentOptions.Right);
            UIFactory.Label(page, "STEAL A\nMILLION", 66, UIFactory.White, .02f, .715f, .96f, .185f);
            UIFactory.Art(page, ArtKind.Vault, game.CosmeticColor, .16f, .42f, .68f, .29f);
            UIFactory.Label(page, MoneyManager.Format(save.hasRun ? save.currentMoney : game.Config.startingMoney), 52, UIFactory.Gold, .02f, .335f, .96f, .08f);
            string subtitle = save.hasRun ? "LEVEL " + save.currentLevel + " / " + game.Session.TotalLevels : "ONE MILLION. YOUR NEXT MOVE.";
            UIFactory.Label(page, subtitle, 17, UIFactory.Muted, .02f, .29f, .96f, .04f);
            Command(save.hasRun ? "CONTINUE" : "PLAY", .185f, game.Continue, UIFactory.Green);
            if (save.hasRun) Command("NEW GAME", .09f, () => Confirm("START A NEW RUN?", "Your current run will be replaced.", game.NewRun), UIFactory.Surface);
            UIFactory.Button(page, "SHOP", UIFactory.Surface, UIFactory.White, .02f, .006f, .72f, .08f, Click(game.OpenShop));
            IconButton(page, ArtKind.Settings, .84f, .006f, game.OpenSettings);
        }

        private void GameHeader(long amount)
        {
            UIFactory.Label(page, "STEAL A MILLION", 20, UIFactory.White, .02f, .925f, .8f, .055f, TextAlignmentOptions.Left);
            IconButton(page, ArtKind.Pause, .84f, .92f, game.Pause);
            UIFactory.Label(page, "LEVEL " + game.Session.Save.currentLevel + " / " + game.Session.TotalLevels,
                18, game.CosmeticColor, .02f, .858f, .96f, .035f);
            UIFactory.Label(page, "CURRENT MONEY", 17, UIFactory.Muted, .02f, .80f, .96f, .04f);
            moneyLabel = UIFactory.Label(page, MoneyManager.Format(amount), 76, UIFactory.White, 0, .685f, 1, .12f);
            UIFactory.Panel(page, "Progress track", UIFactory.Surface, .035f, .661f, .93f, .009f, true);
            float progress = Mathf.Clamp01((float)amount / game.Session.Target);
            if (progress > 0) UIFactory.Panel(page, "Progress", UIFactory.Gold, .035f, .661f, .93f * progress, .009f, true);
            UIFactory.Label(page, "TARGET " + MoneyManager.Format(game.Session.Target), 16, UIFactory.Muted, .02f, .615f, .96f, .037f);
        }

        public void Playing()
        {
            Clear();
            var session = game.Session;
            var d = session.Decision;
            GameHeader(session.Money.Current);
            UIFactory.Label(page, "DECISION " + (session.Save.decisionIndex + 1).ToString("00"), 15, UIFactory.Muted, .02f, .568f, .96f, .03f);
            UIFactory.Label(page, d.title.ToUpperInvariant(), 29, UIFactory.White, .02f, .516f, .96f, .045f);
            var description = UIFactory.Label(page, d.description, 18, UIFactory.Muted, .025f, .467f, .95f, .047f);
            description.fontStyle = FontStyles.Normal;
            var safe = UIFactory.Button(page, "", UIFactory.Green, UIFactory.Background, .02f, .29f, .96f, .16f, Click(() => game.Choose(false)));
            UIFactory.Art(safe.transform, ArtKind.Shield, UIFactory.Background, .055f, .58f, .08f, .22f);
            UIFactory.Label(safe.transform, d.safeReward.mode == RewardMode.Multiply ? "KEEP" : "SAFE", 20, UIFactory.Background, .15f, .56f, .7f, .26f, TextAlignmentOptions.Left);
            UIFactory.Label(safe.transform, d.safeReward.Describe(), 38, UIFactory.Background, .05f, .13f, .90f, .4f);
            UIFactory.Label(page, "OR", 16, UIFactory.Muted, .4f, .25f, .2f, .032f);
            var risk = UIFactory.Button(page, "", UIFactory.Surface, UIFactory.Gold, .02f, .055f, .96f, .183f, Click(() => game.Choose(true)));
            UIFactory.Art(risk.transform, ArtKind.Bolt, UIFactory.Gold, .055f, .68f, .08f, .2f);
            UIFactory.Label(risk.transform, "RISK", 20, UIFactory.Gold, .15f, .66f, .7f, .23f, TextAlignmentOptions.Left);
            UIFactory.Label(risk.transform, Mathf.RoundToInt(d.riskProbability * 100) + "%  " + d.riskWinReward.Describe(), 29, UIFactory.White, .04f, .34f, .92f, .27f);
            UIFactory.Label(risk.transform, Mathf.RoundToInt((1 - d.riskProbability) * 100) + "%  " + d.riskLossReward.Describe(), 21, UIFactory.Red, .04f, .10f, .92f, .23f);
            UIFactory.Label(page, "FICTIONAL MONEY", 13, UIFactory.Muted, .1f, .004f, .8f, .028f);
        }

        public void Suspense(ResultData result)
        {
            Clear();
            GameHeader(result.before);
            UIFactory.Art(page, ArtKind.Vault, game.CosmeticColor, .28f, .30f, .44f, .24f);
            suspenseLabel = UIFactory.Label(page, "RISKING", 32, UIFactory.Gold, .02f, .205f, .96f, .07f);
            UIFactory.Label(page, Mathf.RoundToInt(game.Session.Decision.riskProbability * 100) + "% CHANCE", 20, UIFactory.Muted, .02f, .15f, .96f, .045f);
        }

        public void SuspenseTick(float elapsed)
        {
            if (suspenseLabel != null) suspenseLabel.text = "RISKING" + new string('.', (int)(elapsed * 5) % 4);
        }

        public void Result(ResultData result)
        {
            Clear();
            GameHeader(result.before);
            bool positive = result.after >= result.before;
            Color color = positive ? UIFactory.Green : UIFactory.Red;
            UIFactory.Art(page, positive ? ArtKind.Check : ArtKind.Cross, color, .39f, .445f, .22f, .12f);
            string title = !result.choseRisk ? "LOCKED IN" : result.riskWon ? "YOU WON!" : "RISK LOST";
            UIFactory.Label(page, title, 32, color, .02f, .355f, .96f, .07f);
            long delta = result.after - result.before;
            UIFactory.Label(page, delta == 0 ? "TOTAL KEPT" : (delta > 0 ? "+" : "-") + MoneyManager.Format(Math.Abs(delta)),
                47, UIFactory.White, .02f, .245f, .96f, .09f);
            Command("CONTINUE", .105f, game.SkipResult, UIFactory.Surface);
        }

        public void Terminal(bool victory)
        {
            Clear();
            var save = game.Session.Save;
            UIFactory.Label(page, victory ? "THE VAULT IS YOURS" : "END OF THE RUN", 18, UIFactory.Muted, .02f, .88f, .96f, .05f);
            UIFactory.Art(page, victory ? ArtKind.Vault : ArtKind.Cross, victory ? UIFactory.Gold : UIFactory.Red, .22f, .56f, .56f, .27f);
            UIFactory.Label(page, victory ? "YOU STOLE\nA MILLION!" : "GAME OVER", victory ? 48 : 46,
                victory ? UIFactory.Gold : UIFactory.White, .02f, .405f, .96f, .14f);
            UIFactory.Label(page, victory ? MoneyManager.Format(save.currentMoney) : "YOU REACHED " + MoneyManager.Format(save.runPeak),
                victory ? 44 : 27, UIFactory.White, .02f, .315f, .96f, .07f);
            if (victory) UIFactory.Label(page, "+250 COINS", 21, UIFactory.Gold, .02f, .27f, .96f, .04f);
            if (!victory && !save.secondChanceUsed && game.Ads.IsRewardedAdReady())
                Command("SECOND CHANCE  /  TEST AD", .218f, game.SecondChance, UIFactory.Gold);
            Command(victory ? "PLAY AGAIN" : "TRY AGAIN", .125f, game.NewRun, UIFactory.Green);
            Command("MAIN MENU", .03f, game.MainMenu, UIFactory.Surface);
        }

        public void Settings()
        {
            Clear();
            Header("SETTINGS", game.MainMenu);
            var save = game.Session.Save;
            ToggleRow("Sound", .70f, save.soundEnabled, value => save.soundEnabled = value);
            ToggleRow("Music", .56f, save.musicEnabled, value => save.musicEnabled = value);
            ToggleRow("Vibration", .42f, save.vibrationEnabled, value => save.vibrationEnabled = value);
            Command("RESET PROGRESS", .18f, () => Confirm("RESET ALL PROGRESS?", "Runs, coins and cosmetics will be erased.", game.ResetProgress), UIFactory.Surface);
        }

        private void ToggleRow(string name, float y, bool enabled, Action<bool> changed)
        {
            UIFactory.Label(page, name, 27, UIFactory.White, .02f, y, .6f, .08f, TextAlignmentOptions.Left);
            var background = UIFactory.Panel(page, name + " Toggle", enabled ? UIFactory.Green : UIFactory.Surface, .74f, y, .24f, .083f, true);
            background.raycastTarget = true;
            var toggle = background.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.isOn = enabled;
            var knob = UIFactory.Panel(background.transform, "Knob", UIFactory.White, enabled ? .53f : .06f, .15f, .40f, .70f, true);
            toggle.onValueChanged.AddListener(value =>
            {
                game.Click();
                changed(value);
                game.SettingsChanged();
                background.color = value ? UIFactory.Green : UIFactory.Surface;
                knob.rectTransform.anchorMin = new Vector2(value ? .53f : .06f, .15f);
                knob.rectTransform.anchorMax = new Vector2(value ? .93f : .46f, .85f);
            });
        }

        public void Shop()
        {
            Clear();
            Header("COSMETICS", game.MainMenu);
            UIFactory.Label(page, game.Session.Save.coins.ToString("N0") + " COINS", 27, UIFactory.Gold, .02f, .815f, .96f, .06f);
            var items = game.Shop.Items;
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                bool owned = game.Session.Save.unlockedCosmetics.Contains(item.id);
                bool equipped = game.Session.Save.equippedCosmetic == item.id;
                float y = .62f - i * .16f;
                var row = UIFactory.Panel(page, item.id, UIFactory.Surface, .02f, y, .96f, .14f, true);
                UIFactory.Art(row.transform, ArtKind.Vault, UIFactory.Hex(item.color), .015f, .1f, .23f, .8f);
                UIFactory.Label(row.transform, item.name, 22, UIFactory.White, .26f, .66f, .68f, .27f, TextAlignmentOptions.Left);
                var button = UIFactory.Button(row.transform, equipped ? "EQUIPPED" : owned ? "EQUIP" : item.price + " COINS", equipped ? UIFactory.Green : UIFactory.Background,
                    equipped ? UIFactory.Background : UIFactory.Gold, .27f, .08f, .67f, .56f, Click(() => game.Purchase(item.id)));
                button.interactable = !equipped && (owned || game.Session.Save.coins >= item.price);
            }
        }

        public void PauseMenu()
        {
            MakeModal("PAUSED", MoneyManager.Format(game.Session.Money.Current));
            UIFactory.Button(modal, "RESUME", UIFactory.Green, UIFactory.Background, .10f, .37f, .80f, .085f, Click(game.Resume));
            UIFactory.Button(modal, "MAIN MENU", UIFactory.Surface, UIFactory.White, .10f, .255f, .80f, .085f, Click(game.MainMenu));
        }

        public void Confirm(string title, string message, Action confirmed)
        {
            MakeModal(title, message);
            UIFactory.Button(modal, "CANCEL", UIFactory.Surface, UIFactory.White, .08f, .30f, .40f, .085f, Click(CloseModal));
            UIFactory.Button(modal, "CONFIRM", UIFactory.Red, UIFactory.Background, .52f, .30f, .40f, .085f, Click(() => { CloseModal(); confirmed(); }));
        }

        public void AdWaiting()
        {
            MakeModal("SECOND CHANCE", "TEST AD");
            UIFactory.Label(modal, "...", 45, UIFactory.Gold, .1f, .32f, .8f, .08f);
        }

        private void MakeModal(string title, string message)
        {
            CloseModal();
            var shade = UIFactory.Panel(stage, "Modal", new Color(.055f, .075f, .08f, .98f), 0, 0, 1, 1);
            shade.raycastTarget = true;
            modal = shade.rectTransform;
            UIFactory.Label(modal, title, 32, UIFactory.White, .05f, .56f, .90f, .10f);
            UIFactory.Label(modal, message, 21, UIFactory.Muted, .08f, .46f, .84f, .09f);
        }

        public bool HasModal { get { return modal != null; } }
        public void CloseModal()
        {
            if (modal == null) return;
            modal.gameObject.SetActive(false);
            Destroy(modal.gameObject);
            modal = null;
        }

        public IEnumerator CountMoney(long from, long to, float duration, Func<bool> skip)
        {
            float elapsed = 0;
            while (elapsed < duration && moneyLabel != null && !skip())
            {
                elapsed += Time.deltaTime;
                float t = 1 - Mathf.Pow(1 - Mathf.Clamp01(elapsed / duration), 3);
                moneyLabel.text = MoneyManager.Format(from + (long)((to - from) * (double)t));
                moneyLabel.rectTransform.localScale = Vector3.one * (1 + .055f * Mathf.Sin(t * Mathf.PI));
                yield return null;
            }
            if (moneyLabel != null) { moneyLabel.text = MoneyManager.Format(to); moneyLabel.rectTransform.localScale = Vector3.one; }
        }

        public void PlayFeedback(bool positive, bool big)
        {
            if (feedback != null) StopCoroutine(feedback);
            feedback = StartCoroutine(Feedback(positive, big));
        }

        private IEnumerator Feedback(bool positive, bool big)
        {
            Color color = positive ? UIFactory.Gold : UIFactory.Red;
            for (float t = 0; t < .35f; t += Time.deltaTime)
            {
                flash.color = new Color(color.r, color.g, color.b, (1 - t / .35f) * (big ? .12f : .06f));
                if (!positive && page != null)
                    page.anchoredPosition = new Vector2(Mathf.Sin(t * 85) * (1 - t / .35f) * 6, 0);
                yield return null;
            }
            flash.color = Color.clear;
            if (page != null) page.anchoredPosition = Vector2.zero;
            feedback = null;
        }

        public void Burst()
        {
            if (celebration != null) StopCoroutine(celebration);
            celebration = StartCoroutine(Confetti());
        }

        private IEnumerator Confetti()
        {
            var bounds = ((RectTransform)particles[0].parent).rect;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].gameObject.SetActive(true);
                particles[i].anchoredPosition = new Vector2(UnityEngine.Random.Range(-bounds.width * .4f, bounds.width * .4f), bounds.height * .55f);
                velocities[i] = new Vector2(UnityEngine.Random.Range(-75f, 75f), UnityEngine.Random.Range(-280f, -110f));
            }
            for (float t = 0; t < 3; t += Time.deltaTime)
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    velocities[i] += Vector2.down * (100 * Time.deltaTime);
                    particles[i].anchoredPosition += velocities[i] * Time.deltaTime;
                    particles[i].Rotate(0, 0, (i % 2 == 0 ? 130 : -110) * Time.deltaTime);
                }
                yield return null;
            }
            foreach (var particle in particles) particle.gameObject.SetActive(false);
            celebration = null;
        }
    }
}
