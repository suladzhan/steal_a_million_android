using System;
using System.Linq;
using StealAMillion.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StealAMillion
{
    public sealed class UIManager : MonoBehaviour
    {
        private GameManager game;
        private RectTransform safe,root,modal,toast;
        private TextMeshProUGUI money,target,level,tutorial,power,notice,suspenseText;
        private Image wealthBar,levelBar,spinner;
        private Button rewardedButton;
        private float toastTime;
        private float moneyAnimation;
        private BigMoney moneyFrom,moneyTo;
        private bool hud;
        private CosmeticSlot slot;
        private int missionTab;
        private GameObject swipeGuide;
        private static readonly Color Ink=RunnerArt.Color("#16202A"),Blue=RunnerArt.Color("#3E78FF"),Green=RunnerArt.Color("#34D17B"),Gold=RunnerArt.Color("#FFC928"),Gray=RunnerArt.Color("#82909D");
        public bool HasModal {get{return modal!=null;}}
        public void AnimateMoney(BigMoney before,BigMoney after){moneyFrom=before;moneyTo=after;moneyAnimation=.35f;}
        public void Initialize(GameManager manager)
        {
            game=manager;var canvas=GetComponent<Canvas>();if(canvas==null)canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=10;
            var scaler=GetComponent<CanvasScaler>();if(scaler==null)scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(540,960);scaler.matchWidthOrHeight=1;
            if(GetComponent<GraphicRaycaster>()==null)gameObject.AddComponent<GraphicRaycaster>();
            safe=UIFactory.Rect(transform,"SafeArea",0,0,1,1);safe.gameObject.AddComponent<SafeArea>();
            var stage=UIFactory.Rect(safe,"Stage",0,0,1,1);stage.anchorMin=stage.anchorMax=new Vector2(.5f,.5f);stage.gameObject.AddComponent<StageLayout>();safe=stage;
            notice=UIFactory.Label(safe,"",17,Color.red,.03f,.005f,.94f,.035f);
        }
        private string T(string key,params object[] args){return game.Loc.Text(key,args);}
        private TextMeshProUGUI Label(Transform parent,string value,float size,Color color,float x,float y,float w,float h)
        {
            var text=UIFactory.Label(parent,value,size,color,x,y,w,h);text.font=Resources.Load<TMP_FontAsset>("Runner/Fonts/Primary")??TMP_Settings.defaultFontAsset;text.fontSizeMin=Mathf.Min(12,size*.6f);text.overflowMode=TextOverflowModes.Overflow;return text;
        }
        private Button Button(Transform parent,string key,float x,float y,float w,float h,Action action,Color? color=null,params object[] args)
        {
            var button=UIFactory.Button(parent,T(key,args),color??Blue,Color.white,x,y,w,h,action);button.name=key;
            var text=button.GetComponentInChildren<TMP_Text>();if(text!=null){text.font=Resources.Load<TMP_FontAsset>("Runner/Fonts/Primary")??TMP_Settings.defaultFontAsset;text.fontSizeMax=22;text.fontSizeMin=12;}
            string icon=key=="UI_CONTINUE"||key=="UI_NEXT"?"play":key=="UI_MENU"?"home":key=="UI_DOUBLE_COINS"||key=="UI_SECOND_CHANCE"?"rewarded":key=="UI_CLAIM"?"claim":key.StartsWith("UI_")?key.Substring(3).ToLowerInvariant():null;
            var sprite=icon==null?null:Resources.Load<Sprite>("Runner/UI/UI_"+icon+"_on_color")??Resources.Load<Sprite>("Runner/UI/UI_"+icon);
            if((sprite!=null||key=="UI_SETTINGS")&&text!=null){
                if(key=="UI_SETTINGS")UIFactory.Art(button.transform,ArtKind.Settings,Color.white,game.Loc.IsRtl?.79f:.045f,.2f,.16f,.6f);
                else {var image=UIFactory.Rect(button.transform,"ActionIcon",game.Loc.IsRtl?.79f:.045f,.2f,.16f,.6f).gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;}
                text.rectTransform.anchorMin=new Vector2(game.Loc.IsRtl?.04f:.23f,.05f);text.rectTransform.anchorMax=new Vector2(game.Loc.IsRtl?.77f:.96f,.95f);text.rectTransform.offsetMin=text.rectTransform.offsetMax=Vector2.zero;
            }
            return button;
        }
        private void Clear(bool opaque=false)
        {
            hud=false;rewardedButton=null;CloseModal();if(root!=null){root.gameObject.SetActive(false);Destroy(root.gameObject);}
            root=UIFactory.Rect(safe,"View",0,0,1,1);root.gameObject.AddComponent<PanelReveal>();if(opaque){UIFactory.Panel(root,"Page",RunnerArt.Color("#EEF4FA"),0,0,1,1);UIFactory.Panel(root,"Header band",Color.white,0,.89f,1,.11f);}
            money=target=level=tutorial=power=null;wealthBar=levelBar=null;swipeGuide=null;
        }
        private void Header(string key)
        {
            Label(root,T(key),30,Ink,.16f,.91f,.68f,.06f);
            var back=Button(root,"",game.Loc.IsRtl?.865f:.035f,.915f,.1f,.055f,game.MainMenu,Color.white);back.name="Back";
            var icon=UIFactory.Art(back.transform,ArtKind.Back,Ink,.15f,.15f,.7f,.7f);if(game.Loc.IsRtl)icon.transform.localScale=new Vector3(-1,1,1);
        }
        public void MainMenu()
        {
            Clear();var logo=Resources.Load<Sprite>("Runner/UI/Logo-v001");
            if(logo!=null){var image=UIFactory.Rect(root,"GameLogo",.10f,.855f,.80f,.12f).gameObject.AddComponent<Image>();image.sprite=logo;image.preserveAspect=true;image.raycastTarget=false;}
            else Label(root,T("UI_TITLE"),50,Ink,.08f,.79f,.84f,.14f);
            UIFactory.Panel(root,"Player chip",new Color(1,1,1,.94f),.035f,.805f,.46f,.04f,true);
            UIFactory.Panel(root,"Coin chip",new Color(1,.97f,.84f,.97f),.505f,.805f,.46f,.04f,true);
            Label(root,T("UI_PLAYER_LEVEL",RunnerProgress.PlayerLevel(game.Progress.xp)),16,Ink,.05f,.808f,.43f,.033f);
            Label(root,T("UI_COINS",game.Progress.coins),16,Ink,.525f,.808f,.42f,.033f);
            UIFactory.Panel(root,"Wealth card",RunnerArt.Color("#173A59"),.24f,.717f,.52f,.075f,true);
            Label(root,game.Progress.money.Format(),36,Color.white,.255f,.727f,.49f,.055f);
            Label(root,T("RANK_"+RunnerProgress.Rank(game.Progress.highest)),16,Ink,.1f,.675f,.8f,.035f);
            UIFactory.Panel(root,"Menu deck",new Color(.97f,.985f,1,.97f),.025f,.01f,.95f,.355f,true);
            Label(root,T(game.Worlds[game.Progress.world].title)+"  /  "+T("UI_LEVEL",game.Progress.level),16,Ink,.07f,.322f,.86f,.035f);
            bool resume=game.Session.Run.active&&game.Session.Run.state!=RunnerState.Finished;
            Button(root,resume?"UI_CONTINUE":"UI_PLAY",.085f,.23f,.83f,.08f,()=>{if(resume)game.Continue();else game.Play();},Green);
            Button(root,"UI_SHOP",.06f,.153f,.43f,.06f,game.OpenShop);
            Button(root,"UI_MISSIONS",.51f,.153f,.43f,.06f,MissionScreen);
            Button(root,"UI_PROFILE",.06f,.078f,.43f,.06f,Profile,new Color(.19f,.40f,.58f));
            Button(root,"UI_SETTINGS",.51f,.078f,.43f,.06f,game.OpenSettings,new Color(.19f,.40f,.58f));
            Button(root,"UI_WORLDS",.22f,.019f,.56f,.045f,WorldMap,new Color(.19f,.40f,.58f));
        }
        public void Playing()
        {
            Clear();hud=true;
            UIFactory.Panel(root,"HUD backing",new Color(1,1,1,.94f),.025f,.867f,.95f,.118f,true);
            level=Label(root,T("UI_LEVEL",game.Session.Run.level),15,Ink,.06f,.944f,.31f,.028f);
            money=Label(root,game.Session.Money.Format(),32,Ink,.22f,.91f,.56f,.051f);
            target=Label(root,"",13,Gray,.12f,.878f,.76f,.027f);
            var bar=UIFactory.Panel(root,"Wealth track",new Color(.86f,.90f,.93f),.15f,.872f,.70f,.006f,true);
            wealthBar=UIFactory.Panel(bar.transform,"Wealth progress",Green,0,0,0,1,true);
            var progress=UIFactory.Panel(root,"Level track",new Color(1,1,1,.7f),.1f,.847f,.8f,.006f);
            levelBar=UIFactory.Panel(progress.transform,"Level progress",Blue,0,0,0,1);
            var pause=Button(root,"",.84f,.922f,.105f,.05f,game.Pause,RunnerArt.Color("#EAF2FA"));pause.name="Pause";UIFactory.Art(pause.transform,ArtKind.Pause,Ink,.22f,.18f,.56f,.64f);
            tutorial=Label(root,"",22,Ink,.08f,.15f,.84f,.065f);
            if(!game.Progress.tutorialMove||!game.Progress.tutorialGate){var hint=UIFactory.Panel(root,"Swipe guide",new Color(1,1,1,.92f),.26f,.095f,.48f,.045f,true);swipeGuide=hint.gameObject;UIFactory.Art(hint.transform,ArtKind.Back,Blue,.12f,.2f,.18f,.6f);var arrow=UIFactory.Art(hint.transform,ArtKind.Back,Blue,.70f,.2f,.18f,.6f);arrow.transform.localScale=new Vector3(-1,1,1);UIFactory.Panel(hint.transform,"Touch",Blue,.45f,.22f,.10f,.56f,true);}
            power=Label(root,"",18,Blue,.08f,.065f,.84f,.065f);
            if(game.Session.Run.endless)Button(root,"UI_EXIT_ENDLESS",.23f,.015f,.54f,.045f,game.Finish);
            if(game.Session.Run.practice)Label(root,T("UI_PRACTICE"),17,Blue,.3f,.76f,.4f,.035f);
        }
        public void Suspense(PendingRisk result)
        {
            CloseToast();toast=UIFactory.Rect(safe,"Risk suspense",.14f,.66f,.72f,.12f);
            UIFactory.Panel(toast,"Backdrop",RunnerArt.Color("#FF4D5A"),0,0,1,1,true);
            suspenseText=Label(toast,T("UI_CHANCE",Mathf.RoundToInt(result.probability*100)),28,RunnerArt.Color("#FFE76A"),.05f,.40f,.9f,.48f);
            spinner=UIFactory.Panel(toast,"Suspense meter",Gold,.06f,.14f,.2f,.09f,true);toastTime=0;
        }
        public void SuspenseTick(float t){if(spinner!=null){spinner.rectTransform.anchorMin=new Vector2(.06f+Mathf.PingPong(t*3,.7f),.14f);spinner.rectTransform.anchorMax=spinner.rectTransform.anchorMin+new Vector2(.18f,.09f);}}
        public void RiskResult(bool won){ShowToast(T(won?"UI_RESULT_WIN":"UI_RESULT_LOSS"),won?Green:RunnerArt.Color("#FF4D5A"),1.2f);}
        public void Milestone(int exponent)
        {
            string key=exponent==3?"UI_FIRST_THOUSAND":exponent==5?"UI_SIX_FIGURES":exponent==6?"UI_MILLIONAIRE":exponent==9?"UI_BILLIONAIRE":exponent==12?"UI_TRILLIONAIRE":"UI_MILESTONE";
            ShowToast(T(key)+"\n"+T("UI_MILESTONE_VALUE",BigMoney.Power10(exponent).Format()),Gold,2.8f);
        }
        private void ShowToast(string text,Color color,float duration)
        {
            CloseToast();toast=UIFactory.Rect(safe,"Announcement",.08f,.64f,.84f,.13f);UIFactory.Panel(toast,"Backdrop",color,0,0,1,1,true);
            Label(toast,text,25,color==Gold?Ink:Color.white,.04f,.08f,.92f,.84f);toastTime=duration;
        }
        private void CloseToast(){if(toast!=null){toast.gameObject.SetActive(false);Destroy(toast.gameObject);}toast=null;spinner=null;suspenseText=null;}
        public void Terminal(bool won)
        {
            Clear();CloseToast();UIFactory.Panel(root,"Result summary",new Color(1,1,1,.95f),.04f,.692f,.92f,.265f,true);Label(root,T(won?"UI_FINISH":"UI_BROKE"),36,won?Ink:RunnerArt.Color("#FF4055"),.08f,.875f,.84f,.065f);
            Label(root,game.Session.Money.Format(),40,Ink,.12f,.802f,.76f,.064f);
            var run=game.Session.Run;
            if(won){int stars=run.hits==0?3:run.hits<3?2:1;for(int i=0;i<3;i++)UIFactory.Art(root,ArtKind.Star,i<stars?Gold:RunnerArt.Color("#D6E0EA"),.35f+i*.105f,.755f,.09f,.042f);}
            else Label(root,T("UI_LEVEL",run.level),20,Ink,.15f,.755f,.7f,.04f);
            Label(root,T("UI_SCORE",run.score)+(won?"  /  "+T("UI_COINS",run.practice?0:run.coins):""),18,Ink,.08f,.704f,.84f,.035f);
            UIFactory.Panel(root,"Result actions",new Color(1,1,1,.96f),.055f,.025f,.89f,.31f,true);
            if(won)
            {
                Button(root,"UI_NEXT",.13f,.25f,.74f,.08f,()=>game.Play(),Green);
                if(!run.doubled&&!run.practice&&game.Config.mockAds)rewardedButton=Button(root,"UI_DOUBLE_COINS",.13f,.15f,.74f,.075f,game.DoubleCoins);
            }
            else
            {
                if(!run.secondChance&&game.Config.mockAds)rewardedButton=Button(root,"UI_SECOND_CHANCE",.10f,.25f,.80f,.08f,game.SecondChance,Green);
                Button(root,"UI_RETRY",.13f,.15f,.74f,.075f,()=>game.Play(run.level));
            }
            Button(root,"UI_MENU",.13f,.055f,.74f,.075f,game.MainMenu,new Color(.26f,.38f,.50f));
        }
        private void Modal(string title)
        {
            CloseModal();modal=UIFactory.Rect(safe,"Modal",0,0,1,1);var backdrop=UIFactory.Panel(modal,"Dim",new Color(.07f,.15f,.22f,.72f),0,0,1,1);backdrop.raycastTarget=true;
            Label(modal,T(title),34,Color.white,.09f,.66f,.82f,.09f);
        }
        public void PauseMenu()
        {
            Modal("UI_PAUSE");Button(modal,"UI_RESUME",.16f,.48f,.68f,.08f,game.Resume,Green);Button(modal,"UI_MENU",.16f,.36f,.68f,.08f,game.MainMenu);
            Label(modal,T("UI_SEED",game.Session.Run.seed),16,Color.white,.16f,.27f,.68f,.04f);
        }
        public void Confirm(string title,string body,Action action)
        {
            Modal(title);Label(modal,T(body),20,Color.white,.12f,.55f,.76f,.085f);
            Button(modal,"UI_CONFIRM",.16f,.40f,.68f,.08f,()=>{CloseModal();action();},RunnerArt.Color("#FF4D5A"));Button(modal,"UI_CANCEL",.16f,.29f,.68f,.08f,CloseModal);
        }
        public void CloseModal(){if(modal!=null){modal.gameObject.SetActive(false);Destroy(modal.gameObject);}modal=null;}
        public void AdWaiting(){Clear();Modal("UI_AD_WAIT");}
        public void Notice(string text){if(notice!=null){notice.text=text;notice.transform.SetAsLastSibling();}}
        private RectTransform Scroll(float bottom,float top)
        {
            var viewport=UIFactory.Rect(root,"Scroll",.04f,bottom,.92f,top-bottom);var image=viewport.gameObject.AddComponent<Image>();image.material=UIFactory.SurfaceMaterial;image.color=new Color(1,1,1,.98f);viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=28;
            var content=UIFactory.Rect(viewport,"Content",0,1,1,0);content.pivot=new Vector2(.5f,1);scroll.content=content;scroll.viewport=viewport;
            var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=10;layout.padding=new RectOffset(7,7,10,12);layout.childControlHeight=false;layout.childControlWidth=true;layout.childForceExpandHeight=false;
            var fitter=content.gameObject.AddComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;return content;
        }
        private RectTransform Row(Transform parent,string name,float height=100)
        {
            var row=UIFactory.Rect(parent,name,0,0,1,0);row.sizeDelta=new Vector2(0,height);var element=row.gameObject.AddComponent<LayoutElement>();element.preferredHeight=height;
            UIFactory.Panel(row,"Card",RunnerArt.Color("#F2F6FB"),0,0,1,1,true);UIFactory.Panel(row,"Accent",new Color(.24f,.47f,.85f,.35f),0,.13f,.009f,.74f,true);return row;
        }
        public void Shop()
        {
            Clear();Header("UI_SHOP");Label(root,T("UI_COINS",game.Progress.coins),18,Ink,.69f,.85f,.28f,.04f);
            var slots=(CosmeticSlot[])Enum.GetValues(typeof(CosmeticSlot));
            for(int i=0;i<slots.Length;i++){var value=slots[i];var tab=Button(root,"SLOT_"+value,.03f+(game.Loc.IsRtl?2-i%3:i%3)*.317f,.48f-(i/3)*.06f,.306f,.055f,()=>{slot=value;Shop();},slot==value?Blue:Gray);tab.GetComponentInChildren<TMP_Text>().fontSizeMax=15;}
            var content=Scroll(.025f,.35f);
            foreach(var item in game.Cosmetics.Where(c=>c.slot==slot))
            {
                var captured=item;var row=Row(content,item.id,134);bool owned=game.Progress.owned.Contains(item.id),unlocked=RunnerProgress.Unlocked(game.Progress,item);
                UIFactory.Panel(row,"Color",RunnerArt.Color(item.color),.015f,.22f,.12f,.58f,true);
                var preview=Button(row,item.title,.16f,.55f,.8f,.35f,()=>game.Preview(captured),Color.white);
                preview.GetComponentInChildren<TMP_Text>().color=Ink;
                string equipped=Equipped(slot);string key=owned?(equipped==item.id?"UI_EQUIPPED":"UI_EQUIP"):unlocked?"UI_BUY":"UI_LOCKED";
                var buy=Button(row,key,.55f,.10f,.41f,.32f,()=>game.Purchase(captured.id),owned?Green:Blue,item.cost);buy.interactable=(owned||unlocked)&&equipped!=item.id&&(owned||game.Progress.coins>=item.cost);
                if(!unlocked&&!owned)Label(row,T("UI_REQUIREMENT",item.levels,item.playerLevel,item.wealthExponent==0?"$0":BigMoney.Power10(item.wealthExponent).Format()),12,Gray,.16f,.04f,.37f,.47f);
                MirrorChildren(row);
            }
            Label(root,T("UI_COLLECTION",game.Progress.owned.Count(id=>game.Cosmetics.Any(c=>c.slot==slot&&c.id==id)),game.Cosmetics.Count(c=>c.slot==slot)),15,Ink,.1f,.555f,.8f,.04f);
        }
        private string Equipped(CosmeticSlot value)
        {
            switch(value){case CosmeticSlot.Character:return game.Progress.character;case CosmeticSlot.Outfit:return game.Progress.outfit;case CosmeticSlot.Trail:return game.Progress.trail;case CosmeticSlot.MoneyEffect:return game.Progress.effect;case CosmeticSlot.Victory:return game.Progress.victory;case CosmeticSlot.Vault:return game.Progress.vault;default:return game.Progress.gate;}
        }
        public void Settings()
        {
            Clear(true);Header("UI_SETTINGS");var content=Scroll(.09f,.87f);
            ToggleRow(content,"UI_SOUND",game.SaveData.soundEnabled,v=>game.SaveData.soundEnabled=v);
            ToggleRow(content,"UI_MUSIC",game.SaveData.musicEnabled,v=>game.SaveData.musicEnabled=v);
            ToggleRow(content,"UI_HAPTICS",game.SaveData.vibrationEnabled,v=>game.SaveData.vibrationEnabled=v);
            var lang=Row(content,"Language",100);Label(lang,T("UI_LANGUAGE"),22,Ink,.025f,.53f,.95f,.4f);
            Button(lang,game.Progress.language=="auto"?"UI_AUTO":"LANG_"+game.Progress.language.Replace('-','_'),.08f,.04f,.84f,.42f,Languages);
            Button(root,"UI_RESET",.13f,.02f,.74f,.06f,()=>Confirm("UI_RESET_CONFIRM","UI_RESET_BODY",game.ResetProgress),RunnerArt.Color("#FF4D5A"));
            var privacy=Row(content,"Privacy",80);Button(privacy,"UI_PRIVACY",.03f,.16f,.94f,.62f,Privacy,new Color(.19f,.40f,.58f));
            var version=Row(content,"Version",54);Label(version,Application.version,16,Gray,.05f,.2f,.9f,.6f);
        }
        public void Privacy(){Clear(true);Header("UI_PRIVACY");Label(root,T("UI_PRIVACY_BODY"),22,Ink,.08f,.39f,.84f,.43f);
            var config=ReleaseSettings.Current;if(config!=null&&!string.IsNullOrEmpty(config.privacyUrl))Button(root,"UI_PRIVACY_OPEN",.10f,.22f,.80f,.065f,()=>Application.OpenURL(config.privacyUrl));}
        private void ToggleRow(Transform parent,string key,bool value,Action<bool> changed)
        {
            var row=Row(parent,key,88);Label(row,T(key),23,Ink,.03f,.18f,.70f,.64f);
            var box=UIFactory.Panel(row,key,Blue,.8f,.27f,.12f,.46f,true);box.raycastTarget=true;var check=UIFactory.Art(box.transform,ArtKind.Check,Color.white,.14f,.14f,.72f,.72f);
            var toggle=box.gameObject.AddComponent<Toggle>();toggle.targetGraphic=box;toggle.graphic=check;toggle.isOn=value;toggle.onValueChanged.AddListener(v=>{changed(v);game.SettingsChanged();});
            MirrorChildren(row);
        }
        private void MirrorChildren(Transform parent)
        {
            if(!game.Loc.IsRtl)return;
            foreach(RectTransform rect in parent){var min=rect.anchorMin;var max=rect.anchorMax;rect.anchorMin=new Vector2(1-max.x,min.y);rect.anchorMax=new Vector2(1-min.x,max.y);}
        }
        private void Languages()
        {
            Clear(true);Header("UI_LANGUAGE");var list=Scroll(.03f,.87f);
            foreach(string code in LocalizationManager.Codes){string selected=code;var row=Row(list,code,62);Button(row,code=="auto"?"UI_AUTO":"LANG_"+code.Replace('-','_'),.03f,.08f,.94f,.84f,()=>game.Language(selected),game.Progress.language==code?Green:Blue);}
        }
        public void MissionScreen()
        {
            Clear(true);Header("UI_MISSIONS");string[] tabs={"UI_MISSIONS","UI_DAILY","UI_ACHIEVEMENTS"};
            for(int i=0;i<3;i++){int index=i;Button(root,tabs[i],.04f+i*.31f,.835f,.30f,.05f,()=>{missionTab=index;MissionScreen();},i==missionTab?Blue:Gray);}
            var list=Scroll(.025f,.81f);
            if(missionTab==1)
            {
                RunnerProgress.DailyRefresh(game.Progress,DateTime.UtcNow);string[] metrics={"completed","cash","wins"};int[] goals={3,100,2};
                for(int i=0;i<3;i++){int index=i;var row=Row(list,"Daily "+i,130);int count=Math.Max(0,game.Progress.stats.Get(metrics[i])-game.Progress.dailyBaseline.Get(metrics[i]));bool claimed=game.Progress.dailyClaims.Contains("daily_"+i);
                    Label(row,T("MISSION_"+metrics[i],goals[i]),21,Ink,.03f,.55f,.94f,.36f);Label(row,T("UI_PROGRESS",count,goals[i]),17,Gray,.03f,.1f,.43f,.35f);
                    Button(row,claimed?"UI_CLAIMED":"UI_CLAIM",.52f,.09f,.45f,.35f,()=>game.ClaimDaily(index),Green).interactable=!claimed&&count>=goals[i];}
                return;
            }
            foreach(var mission in game.Missions.Where(m=>m.achievement==(missionTab==2)).OrderBy(m=>game.Progress.claims.Contains(m.id)))
            {
                var item=mission;var row=Row(list,item.id,142);int count=RunnerProgress.ChallengeValue(game.Progress,item);bool claimed=game.Progress.claims.Contains(item.id);
                Label(row,T(item.title,item.metric=="wealth"?(object)BigMoney.Power10(item.target).Format():item.target),20,Ink,.03f,.56f,.94f,.4f);
                Label(row,T("UI_PROGRESS",Math.Min(count,item.target),item.target)+"\n"+T("UI_COINS",item.coins),16,Gray,.03f,.08f,.44f,.46f);
                Button(row,claimed?"UI_CLAIMED":"UI_CLAIM",.52f,.12f,.45f,.32f,()=>game.Claim(item),Green).interactable=!claimed&&count>=item.target;
            }
        }
        public void Profile()
        {
            Clear(true);Header("UI_PROFILE");var list=Scroll(.025f,.875f);var s=game.Progress;var stats=s.stats;
            Stat(list,"UI_PLAYER_LEVEL",RunnerProgress.PlayerLevel(s.xp).ToString());Stat(list,"UI_RANK",T("RANK_"+RunnerProgress.Rank(s.highest)));
            Stat(list,"UI_WEALTH",s.money.Format());Stat(list,"UI_HIGHEST",s.highest.Format());Stat(list,"UI_TOTAL_EARNED",s.lifetime.Format());Stat(list,"UI_BIGGEST",s.biggestReward.Format());
            Stat(list,"UI_COMPLETED",stats.completed.ToString());Stat(list,"UI_RISKS",stats.risks.ToString());Stat(list,"UI_WINS",stats.wins.ToString());Stat(list,"UI_WIN_RATE",(stats.risks==0?0:100f*stats.wins/stats.risks).ToString("0.#")+"%");
            Stat(list,"UI_SAFE",stats.safe.ToString());Stat(list,"UI_CASH",stats.cash.ToString());Stat(list,"UI_COINS_EARNED",stats.coinsEarned.ToString());Stat(list,"UI_SKINS",s.owned.Count(id=>game.Cosmetics.Any(c=>c.id==id&&c.slot==CosmeticSlot.Character)).ToString());
            Stat(list,"UI_ACHIEVEMENTS",s.claims.Count(id=>game.Missions.Any(m=>m.id==id&&m.achievement)).ToString());Stat(list,"UI_ENDLESS_BEST",T("UI_DISTANCE",Mathf.FloorToInt(stats.bestEndless)));
        }
        private void Stat(Transform parent,string key,string value){var row=Row(parent,key,82);Label(row,T(key,value),17,Gray,.03f,.54f,.94f,.4f);Label(row,value,24,Ink,.03f,.08f,.94f,.44f);}
        public void WorldMap()
        {
            Clear(true);Header("UI_WORLDS");var list=Scroll(.02f,.86f);
            var mode=Row(list,"Modes",140);bool endless=game.Progress.highestLevel>=game.Config.endlessUnlockLevel;
            Button(mode,endless?"UI_ENDLESS":"UI_ENDLESS_LOCK",.03f,.53f,.94f,.4f,()=>game.Play(0,true),Blue,game.Config.endlessUnlockLevel).interactable=endless;
            Button(mode,"UI_BONUS",.03f,.06f,.6f,.38f,()=>game.Play(0,false,true),Green).interactable=game.Progress.keys>=3;Label(mode,T("UI_KEYS",game.Progress.keys),18,Ink,.67f,.06f,.3f,.38f);
            for(int i=0;i<game.Worlds.Count;i++)
            {
                int index=i;var world=game.Worlds[i];var row=Row(list,world.id,115);bool unlocked=game.Progress.worlds.Contains(i);
                Button(row,world.title,.03f,.47f,.94f,.43f,()=>game.SelectWorld(index),RunnerArt.Color(world.accent)).interactable=unlocked;
                Label(row,unlocked?T("UI_LEVEL",world.level):i<4?T("UI_WORLD_LOCK",world.level,BigMoney.Power10(world.exponent).Format()):T("UI_MILESTONE_VALUE",BigMoney.Power10(world.exponent).Format()),17,Gray,.03f,.06f,.94f,.35f);
            }
            for(int n=1;n<=Math.Min(game.Progress.highestLevel,500);n++)
            {
                int number=n;var row=Row(list,"Level "+n,72);Button(row,"UI_LEVEL",.03f,.06f,.94f,.86f,()=>game.Play(number),Blue,n);
            }
        }
        private void Update()
        {
            if(game==null)return;if(toastTime>0){toastTime-=Time.deltaTime;if(toastTime<=0)CloseToast();}
            if(rewardedButton!=null)rewardedButton.interactable=game.RewardAvailable;
            if(!hud)return;var run=game.Session.Run;
            if(swipeGuide!=null){bool show=!game.Progress.tutorialMove||!game.Progress.tutorialGate;swipeGuide.SetActive(show);if(show)swipeGuide.transform.localScale=Vector3.one*(1+.025f*Mathf.Sin(Time.unscaledTime*4));}
            moneyAnimation=Mathf.Max(0,moneyAnimation-Time.deltaTime);
            float fraction=1-Mathf.Pow(moneyAnimation/.35f,3);
            var displayed=moneyAnimation<=0?game.Session.Money:moneyTo>=moneyFrom?moneyFrom+(moneyTo-moneyFrom).Times(fraction):moneyFrom-(moneyFrom-moneyTo).Times(fraction);
            money.text=displayed.Format();int exponent=RunnerProgress.NextMilestone(game.Progress.highest);var next=BigMoney.Power10(exponent);
            target.text=T("UI_TARGET",game.Session.Money.Format(),next.Format());wealthBar.rectTransform.anchorMax=new Vector2(Mathf.Clamp01((float)game.Session.Money.Ratio(next)),1);
            levelBar.rectTransform.anchorMax=new Vector2(run.endless?Mathf.Repeat(run.distance,288)/288:Mathf.Clamp01(game.World.player.transform.position.z/Mathf.Max(1,game.World.FinishZ)),1);
            tutorial.text=!game.Progress.tutorialMove?T("UI_SWIPE"):!game.Progress.tutorialGate?T("UI_CHOOSE"):run.combo>=5?T("UI_COMBO",run.combo):"";
            power.text=run.shields>0?T("POWER_Shield")+" "+run.shields:run.magnet>0?T("POWER_Magnet"):run.luck>0?T("POWER_Luck",Mathf.RoundToInt(game.Session.Economy.luckBonus*100)):run.doubleCash>0?T("POWER_DoubleCash"):run.slowMotion>0?T("POWER_SlowMotion"):"";
            if(run.endless)level.text=T("UI_DISTANCE",Mathf.FloorToInt(run.distance));
            if(!run.chain.IsZero)power.text=T("UI_CHAIN",run.chain.Format());
        }
    }
}
