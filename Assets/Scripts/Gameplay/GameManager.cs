using System;
using System.Collections;
using StealAMillion.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace StealAMillion
{
    public sealed class GameManager : MonoBehaviour
    {
        private enum View { Menu, Game, Settings, Shop }
        private static GameManager instance;
        public GameSession Session { get; private set; }
        public GameConfig Config { get; private set; }
        public ShopManager Shop { get; private set; }
        public IRewardedAds Ads { get; private set; }
        public UIManager UI { get; private set; }
        private DecisionCatalog catalog;
        private ShopCatalog shopCatalog;
        private SaveManager saves;
        private AudioManager audioManager;
        private HapticsManager haptics;
        private readonly IAnalytics analytics = new AnalyticsManager();
        private Coroutine flow;
        private View view;
        private bool paused;
        private bool background;
        private bool focusLost;
        private bool skipResult;
        private bool adBusy;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private int forcedOutcome;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic() { instance = null; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SAM_TEST_SAVE_DIRECTORY"))) return;
#endif
            if (instance == null) new GameObject("Steal A Million").AddComponent<GameManager>();
        }

        private void Awake()
        {
            if (instance != null) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            Config = Resources.Load<GameConfig>("GameConfig");
            if (Config == null) Config = ScriptableObject.CreateInstance<GameConfig>();
            bool fallback = false;
            try
            {
                var asset = Resources.Load<TextAsset>("Data/decisions");
                catalog = asset == null ? null : JsonUtility.FromJson<DecisionCatalog>(asset.text);
                if (catalog == null || catalog.Validate().Count > 0) throw new InvalidOperationException("Invalid decision catalog.");
            }
            catch (Exception e)
            {
                Debug.LogError("Decision catalog unavailable: " + e.Message);
                fallback = true;
                catalog = FallbackCatalog();
            }
            try
            {
                var asset = Resources.Load<TextAsset>("Data/shop");
                shopCatalog = asset == null ? new ShopCatalog() : JsonUtility.FromJson<ShopCatalog>(asset.text);
            }
            catch (Exception) { shopCatalog = new ShopCatalog(); }
            saves = new SaveManager();
            InitializeSession(saves.Load());
            gameObject.AddComponent<AudioListener>();
            audioManager = gameObject.AddComponent<AudioManager>();
            audioManager.Initialize(Session.Save);
            Ads = new MockRewardedAds(Config.mockAdsEnabled);
            var events = new GameObject("Input", typeof(EventSystem), typeof(StandaloneInputModule));
            events.transform.SetParent(transform, false);
            UI = gameObject.AddComponent<UIManager>();
            UI.Initialize(this);
            MainMenu();
            if (fallback) UI.Notice("Decision data missing. Recovery mode is active.");
        }

        private void InitializeSession(SaveData data)
        {
            Session = new GameSession(catalog, data, Config.startingMoney, Config.targetMoney);
            Shop = new ShopManager(Session.Save, shopCatalog);
            haptics = new HapticsManager(Session.Save);
        }

        public Color CosmeticColor
        {
            get
            {
                var item = Array.Find(Shop.Items, value => value != null && value.id == Session.Save.equippedCosmetic);
                return item == null ? UIFactory.Green : UIFactory.Hex(item.color);
            }
        }

        public void Click() { audioManager.Play(SoundCue.Click); haptics.Pulse(); }

        public void MainMenu()
        {
            StopFlow();
            paused = false;
            ApplyPause();
            Save();
            view = View.Menu;
            LoadScene("MainMenu");
            UI.MainMenu();
        }

        public void Continue()
        {
            if (!Session.CanContinue) { NewRun(); return; }
            view = View.Game;
            LoadScene("Game");
            RenderRun();
        }

        public void NewRun()
        {
            StopFlow();
            paused = false;
            ApplyPause();
            Session.NewRun();
            Save();
            analytics.Track("game_started");
            view = View.Game;
            LoadScene("Game");
            UI.Playing();
        }

        private void RenderRun()
        {
            if (Session.Save.state == RunState.ShowingResult)
                flow = StartCoroutine(Resolve(Session.Save.pendingResult, false));
            else if (Session.Save.state == RunState.Playing) UI.Playing();
            else UI.Terminal(Session.Save.state == RunState.Victory);
        }

        public void Choose(bool risk)
        {
            if (paused || background || focusLost || view != View.Game) return;
            float sample = UnityEngine.Random.value;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (forcedOutcome != 0)
            {
                sample = forcedOutcome > 0 ? 0 : 1;
                forcedOutcome = 0;
            }
#endif
            var result = Session.Choose(risk, sample);
            if (result == null) return;
            Save();
            analytics.Track("decision_made", result.decisionId);
            analytics.Track(risk ? "risk_selected" : "safe_selected", result.decisionId);
            audioManager.Play(risk ? SoundCue.Risk : SoundCue.Safe);
            flow = StartCoroutine(Resolve(result, risk));
        }

        private IEnumerator Resolve(ResultData result, bool suspense)
        {
            skipResult = false;
            if (suspense)
            {
                UI.Suspense(result);
                for (float elapsed = 0; elapsed < Config.suspenseSeconds; elapsed += Time.deltaTime)
                {
                    UI.SuspenseTick(elapsed);
                    yield return null;
                }
            }
            bool positive = result.after >= result.before;
            bool big = result.after - result.before >= 25000;
            UI.Result(result);
            audioManager.Play(positive ? big ? SoundCue.Jackpot : SoundCue.Win : SoundCue.Loss);
            haptics.Pulse(big || !positive);
            if (result.choseRisk) analytics.Track(result.riskWon ? "risk_won" : "risk_lost", result.decisionId);
            UI.PlayFeedback(positive, big);
            if (big) UI.Burst();
            yield return UI.CountMoney(result.before, result.after, Config.countSeconds, () => skipResult);
            for (float elapsed = 0; elapsed < Config.resultSeconds && !skipResult; elapsed += Time.deltaTime) yield return null;
            int level = Session.Save.currentLevel;
            Session.CompleteResult();
            Save();
            flow = null;
            if (Session.Save.state == RunState.Playing)
            {
                if (level != Session.Save.currentLevel)
                {
                    audioManager.Play(SoundCue.Level);
                    analytics.Track("level_completed", level.ToString());
                }
                UI.Playing();
            }
            else
            {
                bool victory = Session.Save.state == RunState.Victory;
                UI.Terminal(victory);
                audioManager.Play(victory ? SoundCue.Victory : SoundCue.GameOver);
                analytics.Track(victory ? "victory" : "game_over");
                haptics.Pulse(true);
                if (victory) UI.Burst();
            }
        }

        public void SkipResult() { skipResult = true; }

        public void Pause()
        {
            if (view != View.Game || paused || adBusy) return;
            paused = true;
            ApplyPause();
            Save();
            UI.PauseMenu();
        }

        public void Resume()
        {
            paused = false;
            UI.CloseModal();
            ApplyPause();
        }

        public void OpenSettings() { view = View.Settings; UI.Settings(); }
        public void OpenShop() { view = View.Shop; analytics.Track("shop_opened"); UI.Shop(); }
        public void SettingsChanged() { audioManager.Refresh(); Save(); }

        public void Purchase(string id)
        {
            bool owned = Session.Save.unlockedCosmetics.Contains(id);
            if (!Shop.BuyOrEquip(id)) return;
            Save();
            if (!owned) analytics.Track("item_purchased", id);
            UI.Shop();
        }

        public void ResetProgress()
        {
            StopFlow();
            if (!saves.Reset()) { UI.Notice("Unable to reset progress. Check device storage."); return; }
            InitializeSession(new SaveData());
            audioManager.SetSettings(Session.Save);
            MainMenu();
        }

        public void SecondChance()
        {
            if (adBusy || Session.Save.state != RunState.GameOver || Session.Save.secondChanceUsed || !Ads.IsRewardedAdReady()) return;
            adBusy = true;
            flow = StartCoroutine(Rewarded());
        }

        private IEnumerator Rewarded()
        {
            UI.AdWaiting();
            analytics.Track("rewarded_ad_started");
            bool rewarded = false;
            yield return Ads.ShowRewardedAd(value => rewarded = value);
            adBusy = false;
            flow = null;
            UI.CloseModal();
            if (rewarded && Session.SecondChance(Config.secondChanceMoney))
            {
                Save();
                analytics.Track("rewarded_ad_completed");
                UI.Playing();
            }
            else UI.Terminal(false);
        }

        private void Save()
        {
            if (saves == null || Session == null) return;
            bool success = saves.Save(Session.Save);
            if (UI != null) UI.Notice(success ? "" : "Progress not saved. Check device storage.");
        }

        private void StopFlow()
        {
            if (flow != null) StopCoroutine(flow);
            flow = null;
        }

        private void ApplyPause()
        {
            Time.timeScale = paused || background || focusLost ? 0 : 1;
            AudioListener.pause = paused || background || focusLost;
            if (UI != null) UI.BlockInput(background || focusLost);
        }

        private void OnApplicationPause(bool value) { background = value; if (value) Save(); ApplyPause(); }
        private void OnApplicationFocus(bool value) { focusLost = !value; if (!value) Save(); ApplyPause(); }
        private void OnApplicationQuit() { Save(); }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || adBusy || background || focusLost) return;
            if (paused) { Resume(); return; }
            if (UI.HasModal) { UI.CloseModal(); return; }
            if (view == View.Game) Pause();
            else if (view == View.Menu) UI.Confirm("LEAVE THE VAULT?", "Your progress is saved.", Application.Quit);
            else MainMenu();
        }

        private static void LoadScene(string name)
        {
            if (SceneManager.GetActiveScene().name != name && Application.CanStreamedLevelBeLoaded(name)) SceneManager.LoadScene(name);
        }

        private static DecisionCatalog FallbackCatalog()
        {
            return new DecisionCatalog { decisions = new[] { new DecisionData {
                id = "recovery", title = "The recovery vault", description = "A fresh opportunity.", level = 1, difficulty = 1,
                riskProbability = .8f, safeReward = new Reward { amount = 100 },
                riskWinReward = new Reward { amount = 250 }, riskLossReward = new Reward { mode = RewardMode.Set, amount = 0 }
            } } };
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (Config == null || !Config.developmentControls || Session == null) return;
            GUILayout.BeginArea(new Rect(8, 8, 220, 280), GUI.skin.box);
            GUILayout.Label("DEBUG / " + Session.Save.state);
            GUILayout.Label("Level " + Session.Save.currentLevel + " / Decision " + (Session.Save.decisionIndex + 1));
            GUILayout.Label("Probability " + Session.Decision.riskProbability);
            if (GUILayout.Button("Add $100,000")) { StopFlow(); Session.DebugSetMoney(Session.Money.Current + 100000); Save(); Continue(); }
            if (GUILayout.Button("Skip level")) { StopFlow(); Session.DebugNextLevel(); Save(); Continue(); }
            if (GUILayout.Button("Next risk wins")) forcedOutcome = 1;
            if (GUILayout.Button("Next risk loses")) forcedOutcome = -1;
            if (GUILayout.Button("Unlock cosmetics"))
            {
                foreach (var item in Shop.Items) if (!Session.Save.unlockedCosmetics.Contains(item.id)) Session.Save.unlockedCosmetics.Add(item.id);
                Save();
            }
            if (GUILayout.Button("Reset save")) UI.Confirm("RESET ALL PROGRESS?", "This cannot be undone.", ResetProgress);
            GUILayout.EndArea();
        }
#endif
    }
}
