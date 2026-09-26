using System;
using System.Collections;
using System.Collections.Generic;
using StealAMillion.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StealAMillion
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance {get;private set;}
        public RunnerConfig Config {get;private set;}
        public SaveData SaveData {get;private set;}
        public RunnerSave Progress {get{return SaveData.runner;}}
        public RunnerSession Session {get;private set;}
        public LocalizationManager Loc {get;private set;}
        public RunnerWorld World {get;private set;}
        public UIManager UI {get;private set;}
        public List<GateData> Gates {get;private set;}
        public List<WorldData> Worlds {get;private set;}
        public List<RunnerCosmetic> Cosmetics {get;private set;}
        public List<ChallengeData> Missions {get;private set;}
        public LevelCatalog Levels {get;private set;}
        public bool IsRunning {get{return !menu&&!paused&&!background&&!focusLost&&!adBusy&&Session.Run.state==RunnerState.Running;}}
        public bool IsSuspense {get{return !menu&&!paused&&!background&&!focusLost&&Session.Run.state==RunnerState.Suspense;}}
        public bool Paused {get{return paused;}}
        public bool RewardAvailable {get{return !adBusy&&!IsRunning&&!IsSuspense&&celebrationTime<=0&&milestones.Count==0;}}
        private bool menu=true,paused,background,focusLost,adBusy;
        private SaveManager saves;
        private AudioManager audioManager;
        private HapticsManager haptics;
        private RunnerEffects effects;
        private IRewardedAds ads;
        private readonly IAnalytics analytics=new AnalyticsManager();
        private readonly System.Random random=new System.Random();
        private Coroutine suspense;
        private float saveTimer,trailTimer;
        private readonly Queue<int> milestones=new Queue<int>();
        private float celebrationTime;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public int ForcedRisk;
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatic(){Instance=null;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
#if UNITY_EDITOR
            if(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SAM_TEST_SAVE_DIRECTORY")))return;
#endif
            if(Instance==null)new GameObject("GameManager").AddComponent<GameManager>();
        }
        private void Awake()
        {
            if(Instance!=null){Destroy(gameObject);return;}Instance=this;
            Config=Resources.Load<RunnerConfig>("Runner/Balance");
            if(Config==null)throw new InvalidOperationException("Run Steal A Million > Build Runner Assets before playing.");
            Application.targetFrameRate=Config.targetFrameRate;Screen.orientation=ScreenOrientation.Portrait;
            saves=new SaveManager();SaveData=saves.Load();RunnerProgress.Migrate(SaveData);BindSession();
            Gates=new List<GateData>(Read<GateCatalog>("gates").gates);Worlds=new List<WorldData>(Read<WorldCatalog>("worlds").worlds);
            Cosmetics=new List<RunnerCosmetic>(Read<RunnerShopCatalog>("shop").items);Missions=new List<ChallengeData>(Read<ChallengeCatalog>("missions").missions);Levels=Read<LevelCatalog>("levels");
            RunnerProgress.DailyRefresh(Progress,DateTime.UtcNow);UnlockWorlds();
            audioManager=gameObject.AddComponent<AudioManager>();audioManager.Initialize(SaveData);haptics=new HapticsManager(SaveData);ads=new MockRewardedAds(Config.mockAds);
            World=FindFirstObjectByType<RunnerWorld>();
            if(World==null)World=Instantiate(Resources.Load<GameObject>("Runner/Stage")).GetComponent<RunnerWorld>();
            if(FindFirstObjectByType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            UI=FindFirstObjectByType<UIManager>();if(UI==null)UI=new GameObject("Runner Canvas",typeof(RectTransform),typeof(Canvas)).AddComponent<UIManager>();UI.Initialize(this);
            effects=gameObject.AddComponent<RunnerEffects>();gameObject.AddComponent<RunnerPerformanceProbe>();Save();MainMenu();analytics.Track("game_started");
        }
        private void BindSession(){Session=new RunnerSession(Progress,Config.economy);Loc=new LocalizationManager(Progress);Session.MoneyChanged+=MoneyChanged;Session.Milestone+=e=>milestones.Enqueue(e);}
        private T Read<T>(string path){var text=Resources.Load<TextAsset>("RunnerData/"+path);if(text==null)throw new InvalidOperationException("Missing RunnerData/"+path);return JsonUtility.FromJson<T>(text.text);}
        public LevelData Level(int number){return Array.Find(Levels.levels,l=>l.number==number)??LevelGenerator.Generate(number,unchecked(713+number*1949),Progress.world);}
        public void MainMenu()
        {
            if(adBusy)return;StopSuspense();StopAllCoroutines();effects.Clear();Snapshot();menu=true;paused=false;ApplyPause();World.Build(Level(Progress.level),true);UI.MainMenu();Save();
        }
        public void Play(int number=0,bool endless=false,bool bonus=false)
        {
            if(adBusy)return;if(endless&&Progress.highestLevel<Config.endlessUnlockLevel)return;
            if(bonus&&Progress.keys<3)return;if(bonus)Progress.keys-=3;
            int level=number<=0?Progress.level:number;if(level>Progress.highestLevel&&!endless&&!bonus)return;
            StopSuspense();StopAllCoroutines();effects.Clear();Session.Start(level,Level(level).seed,endless,bonus);StartScene(false);
            analytics.Track(endless?"endless_started":bonus?"bonus_started":"level_started",level.ToString());
        }
        public void Continue(){if(!Session.Run.active||Session.Run.state==RunnerState.Finished){Play();return;}StartScene(true);}
        private void StartScene(bool resume)
        {
            menu=false;paused=false;ApplyPause();var level=Session.Run.bonus?LevelGenerator.Generate(Session.Run.level,Session.Run.seed,Progress.world,0,true):Level(Session.Run.level);
            World.Build(level,false);UI.Playing();Save();
            if(resume&&Session.Run.state==RunnerState.Suspense)suspense=StartCoroutine(ResolveRisk());
            if(Session.Run.state==RunnerState.Broke)UI.Terminal(false);
        }
        public void Touch(TrackItem item)
        {
            if(item.kind==TrackItemKind.Gate&&!string.IsNullOrEmpty(item.group)&&((World.player.transform.position.x<=0)!=(item.transform.position.x<=0)))return;
            if(!IsRunning||!Session.Consume(string.IsNullOrEmpty(item.group)?item.id:item.group))return;World.ConsumeGroup(item);
            switch(item.kind)
            {
                case TrackItemKind.Cash:Session.Collect(item.scale);Feedback(SoundCue.Cash,"#35D06F");analytics.Track("cash_collected");break;
                case TrackItemKind.Gate:EnterGate(item.Gate);break;
                case TrackItemKind.Obstacle:
                    bool hit=Session.Hit(item.obstacle=="tax"?Session.Economy.taxLoss:Session.Economy.obstacleLoss);
                    if(hit){World.character.Stumble();World.followCamera.Impulse(false);Feedback(item.obstacle=="police"?SoundCue.Police:SoundCue.Loss,"#FF4D5A");}
                    else effects.Floating(Loc.Text("UI_SHIELDED"),RunnerArt.Color("#3E78FF"),World.player.transform.position);
                    analytics.Track(item.obstacle=="tax"?"tax_hit":item.obstacle=="police"?"police_hit":"obstacle_hit",item.obstacle);break;
                case TrackItemKind.Power:Session.Power(item.power,Config.powerSeconds);World.RefreshGates();Feedback(SoundCue.Safe,"#3E78FF");analytics.Track("powerup_used",item.power.ToString());break;
                case TrackItemKind.Coin:Session.Run.coins++;Feedback(SoundCue.Cash,"#FFC928");break;
                case TrackItemKind.Key:if(!Session.Run.practice){Progress.keys++;Progress.stats.keys++;}Session.Run.keys++;Feedback(SoundCue.Level,"#FFC928");break;
                case TrackItemKind.Finish:Finish();break;
            }
            if(Session.Run.state==RunnerState.Broke)UI.Terminal(false);if(item.kind!=TrackItemKind.Cash)Save();
        }
        public static bool IsRisk(GateKind kind){return kind==GateKind.Risk||kind==GateKind.Jackpot||kind==GateKind.RiskChain||kind==GateKind.DoubleOrNothing||kind==GateKind.Market;}
        private void EnterGate(GateData gate)
        {
            Progress.tutorialGate=true;
            if(IsRisk(gate.kind))
            {
                double sample=random.NextDouble();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if(ForcedRisk!=0){sample=ForcedRisk>0?0:.999999999;ForcedRisk=0;}
#endif
                Session.EnterRisk(gate,sample);Save();suspense=StartCoroutine(ResolveRisk());analytics.Track("risk_selected",gate.id);return;
            }
            switch(gate.kind)
            {
                case GateKind.Safe:case GateKind.AddMoney:Session.Safe(gate);analytics.Track("safe_selected");break;
                case GateKind.Multiplier:Session.SetMoney(Session.Run.money.Times(1.2));break;
                case GateKind.Tax:case GateKind.SubtractMoney:Session.Hit(gate.loss);break;
                case GateKind.Investment:case GateKind.Business:Session.Invest(Session.Run.distance,gate.kind==GateKind.Business);break;
                case GateKind.Shield:Session.Power(PowerKind.Shield,Config.powerSeconds);break;
                case GateKind.Luck:Session.Power(PowerKind.Luck,Config.powerSeconds);World.RefreshGates();break;
                case GateKind.Insurance:var cost=Session.Cash.Times(Session.Economy.insuranceUnits);if(Session.Run.money>=cost){Session.SetMoney(Session.Run.money-cost);Session.Power(PowerKind.Shield,Config.powerSeconds);}break;
                case GateKind.CashOut:Session.SetMoney(Session.Run.money+Session.Run.chain);Session.Run.chain=BigMoney.Zero;break;
                case GateKind.KeyGate:if(!Session.Run.practice){Progress.keys++;Progress.stats.keys++;}break;
                case GateKind.Bonus:Session.Collect(5);break;
                case GateKind.Mystery:int prize=random.Next(4);if(prize==0)Session.Power(PowerKind.Magnet,Config.powerSeconds);else if(prize==1)Session.Run.coins+=5;else Session.Collect(prize==2?3:1);break;
            }
            Feedback(SoundCue.Safe,"#34D17B",true);
            if(Progress.gate!="gate_classic")effects.Burst(World.player.transform.position+Vector3.up,RunnerArt.Color(Progress.gate=="gate_spark"?"#FFC928":"#3E78FF"),true);
            World.RefreshGates();
        }
        private IEnumerator ResolveRisk()
        {
            UI.Suspense(Session.Run.pending);audioManager.Duck(true);audioManager.Play(SoundCue.Risk);
            for(float t=0;t<Config.suspenseSeconds;t+=Time.deltaTime){UI.SuspenseTick(t/Config.suspenseSeconds);yield return null;}
            var result=Session.ResolveRisk();Save();audioManager.Duck(false);suspense=null;if(result==null)yield break;
            UI.RiskResult(result.won);World.RefreshGates();
            if(result.won){World.character.Cheer();World.followCamera.Impulse(true);}else{World.character.Stumble();World.followCamera.Impulse(false);}
            Feedback(result.won?result.kind==GateKind.Jackpot?SoundCue.Jackpot:SoundCue.Win:SoundCue.Loss,result.won?"#FFC928":"#FF4D5A",true);
            analytics.Track(result.won?"risk_won":"risk_lost");if(Session.Run.state==RunnerState.Broke)UI.Terminal(false);
        }
        private void MoneyChanged(BigMoney before,BigMoney after)
        {
            if(effects==null||World==null)return;
            if(after.Equals(before))return;
            if(UI!=null)UI.AnimateMoney(before,after);
            var cosmetic=Cosmetics.Find(c=>c.id==Progress.effect);string color=after>=before?(cosmetic==null?"#159447":cosmetic.color):"#FF4055";
            effects.Floating((after>=before?"+":"-")+(after>=before?after-before:before-after).Format(),RunnerArt.Color(color),World.player.transform.position);UnlockWorlds();
        }
        private void Feedback(SoundCue cue,string color,bool big=false){audioManager.Play(cue);haptics.Pulse(big);effects.Cue(cue,World.player.transform.position,RunnerArt.Color(color),big);}
        public void Finish()
        {
            if(!Session.Finish())return;World.player.Active=false;World.CelebrateFinish();
            Feedback(SoundCue.Victory,"#FFC928",true);StartCoroutine(VictoryEffects());UnlockWorlds();Save();UI.Terminal(true);analytics.Track("level_completed",Session.Run.level.ToString());
        }
        private void UnlockWorlds()
        {
            if(Worlds==null)return;
            for(int i=0;i<Worlds.Count;i++){var w=Worlds[i];if((i<4&&Progress.highestLevel>=w.level)||(w.exponent>0&&Progress.highest>=BigMoney.Power10(w.exponent)))if(!Progress.worlds.Contains(i)){Progress.worlds.Add(i);Progress.world=i;analytics.Track("world_unlocked",w.id);}}
            Progress.world=Mathf.Clamp(Progress.world,0,Worlds.Count-1);
        }
        public void SelectWorld(int index){if(Progress.worlds.Contains(index)){Progress.world=index;Save();MainMenu();}}
        public void Pause(){if(menu||adBusy)return;paused=true;Snapshot();ApplyPause();UI.PauseMenu();}
        public void Resume(){paused=false;ApplyPause();UI.CloseModal();}
        public void OpenShop(){World.followCamera.shop=true;World.followCamera.Snap();World.ApplyCosmetics();UI.Shop();}
        public void OpenSettings(){UI.Settings();}
        public void SettingsChanged(){audioManager.Refresh();Save();}
        public void Language(string code){Loc.Select(code);Save();UI.Settings();analytics.Track("language_changed",code);}
        public void Purchase(string id){var item=Cosmetics.Find(c=>c.id==id);if(item!=null&&RunnerProgress.Buy(Progress,item)){Save();World.ApplyCosmetics();Feedback(SoundCue.Win,item.color);UI.Shop();analytics.Track("skin_equipped",id);}}
        public void Preview(RunnerCosmetic item)
        {
            if(item.slot==CosmeticSlot.Character){World.ClearPreview();World.ApplyCosmetics(item.id);}
            else if(item.slot==CosmeticSlot.Victory){World.character.PreviewVictory(item.id);StartCoroutine(VictoryEffects(item.id));}
            else if(item.slot==CosmeticSlot.Vault||item.slot==CosmeticSlot.Gate)World.PreviewProp(item);
            else if(item.slot==CosmeticSlot.Outfit)World.PreviewOutfit(item.color);
            else effects.Burst(World.player.transform.position,RunnerArt.Color(item.color),true);
        }
        private IEnumerator VictoryEffects(string preview=null)
        {
            string style=preview??Progress.victory;
            for(int i=0;i<8;i++)
            {
                Vector3 position=World.player.transform.position;
                if(style=="money_rain")position+=new Vector3(Mathf.Sin(i*2)*2,3,Mathf.Cos(i*2));
                else if(style=="tornado")position+=new Vector3(Mathf.Sin(i)*2,i*.3f,Mathf.Cos(i)*2);
                else position+=new Vector3(i%2==0?-1.7f:1.7f,0,0);
                effects.Burst(position,RunnerArt.Color(style=="gold_pose"?"#FFC928":i%3==0?"#35D06F":i%3==1?"#FFC928":"#FFFFFF"));
                yield return new WaitForSeconds(.18f);
            }
        }
        public void Claim(ChallengeData mission){if(RunnerProgress.Claim(Progress,mission)){Save();UI.MissionScreen();Feedback(SoundCue.Level,"#FFC928");analytics.Track(mission.achievement?"achievement_unlocked":"mission_completed",mission.id);}}
        public void ClaimDaily(int index)
        {
            string[] metric={"completed","cash","wins"};int[] goal={3,100,2};string id="daily_"+index;RunnerProgress.DailyRefresh(Progress,DateTime.UtcNow);
            if(index<0||index>=3||Progress.dailyClaims.Contains(id)||Progress.stats.Get(metric[index])-Progress.dailyBaseline.Get(metric[index])<goal[index])return;
            Progress.dailyClaims.Add(id);RunnerProgress.AddCoins(Progress,40);RunnerProgress.AddXp(Progress,60);Save();UI.MissionScreen();
        }
        public void SecondChance(){if(RewardAvailable&&Session.Run.state==RunnerState.Broke&&!Session.Run.secondChance&&ads.IsRewardedAdReady())StartCoroutine(Reward(false));}
        public void DoubleCoins(){if(RewardAvailable&&Session.Run.state==RunnerState.Finished&&!Session.Run.doubled&&!Session.Run.practice&&ads.IsRewardedAdReady())StartCoroutine(Reward(true));}
        private IEnumerator Reward(bool doubleCoins)
        {
            adBusy=true;UI.AdWaiting();analytics.Track("rewarded_started");bool reward=false;yield return ads.ShowRewardedAd(value=>reward=value);
            if(reward){if(doubleCoins){Session.Run.doubled=true;RunnerProgress.AddCoins(Progress,Session.Run.coins);}else Session.SecondChance();analytics.Track("rewarded_completed");}
            adBusy=false;Save();if(doubleCoins)UI.Terminal(true);else UI.Playing();
        }
        public void ResetProgress()
        {
            StopSuspense();if(!saves.Reset()){UI.Notice(Loc.Text("UI_SAVE_ERROR"));return;}
            SaveData=new SaveData{version=2,runner=new RunnerSave()};BindSession();audioManager.SetSettings(SaveData);haptics=new HapticsManager(SaveData);milestones.Clear();MainMenu();
        }
        private void Snapshot(){if(World!=null&&Session.Run.active&&!menu){Session.Run.x=World.player.transform.position.x;Session.Run.z=World.player.transform.position.z;}Save();}
        public void Save(){if(saves!=null&&!saves.Save(SaveData)&&UI!=null)UI.Notice(Loc.Text("UI_SAVE_ERROR"));}
        private void StopSuspense(){if(suspense!=null)StopCoroutine(suspense);suspense=null;if(audioManager!=null)audioManager.Duck(false);}
        private void ApplyPause(){Time.timeScale=paused||background||focusLost?0:1;AudioListener.pause=paused||background||focusLost;}
        private void OnApplicationPause(bool value){background=value;if(value)Snapshot();ApplyPause();}
        private void OnApplicationFocus(bool value){focusLost=!value;if(!value)Snapshot();ApplyPause();}
        private void OnApplicationQuit(){Snapshot();}
        private void OnDestroy(){if(Instance==this){Instance=null;Time.timeScale=1;AudioListener.pause=false;}}
        private void Update()
        {
            if(Session==null)return;if(Input.GetKeyDown(KeyCode.Escape)&&!adBusy){if(paused)Resume();else if(!menu)Pause();else MainMenu();}
            if(IsRunning)
            {
                Session.Tick(Time.deltaTime,World.player.transform.position.z);saveTimer+=Time.deltaTime;if(saveTimer>=2){Snapshot();saveTimer=0;}
                trailTimer+=Time.deltaTime;if(trailTimer>.5f&&Progress.trail!="dust"){trailTimer=0;var trail=Cosmetics.Find(c=>c.id==Progress.trail);effects.Burst(World.player.transform.position-Vector3.forward,RunnerArt.Color(trail==null?"#35D06F":trail.color));}
            }
            if(celebrationTime>0)celebrationTime-=Time.deltaTime;
            else if(!menu&&!IsSuspense&&milestones.Count>0){int e=milestones.Dequeue();UI.Milestone(e);World.character.Cheer(2);Feedback(SoundCue.Jackpot,"#FFC928",true);celebrationTime=3;analytics.Track("wealth_milestone",e.ToString());World.ApplyCosmetics();}
        }
    }
}
