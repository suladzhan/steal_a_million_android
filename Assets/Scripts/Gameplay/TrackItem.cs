using TMPro;
using UnityEngine;
using StealAMillion.Core;

namespace StealAMillion
{
    public enum TrackItemKind { Cash, Gate, Obstacle, Power, Coin, Key, Finish }
    public sealed class TrackItem : MonoBehaviour
    {
        public TrackItemKind kind;
        public string id,group,gateId,obstacle="barrier";
        public PowerKind power;
        public float scale=1,moving,speed=1,baseX;
        public bool collected;
        private float baseY,clock;
        private float lastProbability=-1;
        private BigMoney lastMoney;
        private TextMeshPro title,value,odds;
        private Transform doorLeft,doorRight;
        private Collider trigger;
        public GateData Gate {get;private set;}
        public void Initialize()
        {
            baseX=transform.localPosition.x;baseY=transform.localPosition.y;
            trigger=GetComponent<Collider>();
            if(obstacle=="closing"){doorLeft=transform.Find("ObstacleVisual/DoorL")??transform.Find("DoorL");doorRight=transform.Find("ObstacleVisual/DoorR")??transform.Find("DoorR");}
            if(kind==TrackItemKind.Gate)
            {
                var source=GameManager.Instance.Gates.Find(g=>g.id==gateId);
                Gate=JsonUtility.FromJson<GateData>(JsonUtility.ToJson(source));
                int level=GameManager.Instance.Session.Run.level;
                if(Gate.kind==GateKind.Risk)foreach(var tier in GameManager.Instance.Session.Economy.riskTiers)if(level>=tier.level){Gate.multiplier=tier.multiplier;Gate.probability=tier.probability;}
                if(Gate.kind==GateKind.Jackpot)Gate.multiplier=level<10?2:level<30?4:10;
                if(Gate.kind==GateKind.RiskChain){int index;var parts=id.Split(':');int.TryParse(parts.Length>1?parts[1]:"0",out index);index=(index/3)%3;Gate.probability=index==0?.7f:index==1?.5f:.3f;Gate.multiplier=index==2?3:2;Gate.loss=1;}
                title=transform.Find("Title").GetComponent<TextMeshPro>();value=transform.Find("Value").GetComponent<TextMeshPro>();odds=transform.Find("Odds").GetComponent<TextMeshPro>();
                string tint=Gate.kind==GateKind.Jackpot?"#E8AB16":GameManager.IsRisk(Gate.kind)?"#FF4D5A":Gate.kind==GateKind.Safe||Gate.kind==GateKind.Multiplier||Gate.kind==GateKind.CashOut?"#34D17B":"#3E78FF";
                foreach(var renderer in GetComponentsInChildren<MeshRenderer>())if(renderer.name=="Banner"||renderer.name=="Pillar")renderer.sharedMaterial=RunnerArt.Material(tint);
                RefreshLabel();
            }
        }
        public void RefreshLabel()
        {
            if(Gate==null||kind!=TrackItemKind.Gate)return;
            var game=GameManager.Instance;var economy=game.Session.Economy;lastMoney=game.Session.Money;
            title.text=game.Loc.Text(Gate.title,Mathf.RoundToInt(economy.luckBonus*100));
            lastProbability=game.Session.Probability(Gate);
            bool risk=GameManager.IsRisk(Gate.kind);
            value.text=risk?"x"+Gate.multiplier.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture):"+"+game.Session.SafeReward(Gate).Format();
            odds.text=risk?game.Loc.Text("UI_CHANCE",Mathf.RoundToInt(game.Session.Probability(Gate)*100)):"";
            var lossLabel=transform.Find("Loss");if(lossLabel!=null)lossLabel.GetComponent<TextMeshPro>().text=risk&&Gate.kind!=GateKind.RiskChain?game.Loc.Text("UI_LOSS",Mathf.RoundToInt(Gate.loss*100)):"";
            if(Gate.kind==GateKind.Investment||Gate.kind==GateKind.Business){bool business=Gate.kind==GateKind.Business;value.text="-"+game.Session.Cash.Times(business?economy.businessUnits:economy.investmentUnits).Format();odds.text="x"+(business?economy.businessReturn:economy.investmentReturn).ToString("0.##",System.Globalization.CultureInfo.InvariantCulture);}
            if(Gate.kind==GateKind.Market){value.text=game.Loc.Text("UI_MARKET_ODDS");value.rectTransform.sizeDelta=new Vector2(3.1f,1.4f);value.transform.localPosition=new Vector3(0,3.3f,-.15f);value.fontSizeMax=3.7f;value.fontSizeMin=2.5f;odds.text="";if(lossLabel!=null)lossLabel.gameObject.SetActive(false);}
            if(Gate.kind==GateKind.CashOut)value.text=game.Loc.Text("UI_CASH_OUT",game.Session.Run.chain.Format());
            if(Gate.kind==GateKind.Shield||Gate.kind==GateKind.Luck||Gate.kind==GateKind.KeyGate||Gate.kind==GateKind.Mystery){value.text=Gate.kind==GateKind.Mystery?"?":"+1";}
            if(Gate.kind==GateKind.Insurance)value.text="-"+game.Session.Cash.Times(economy.insuranceUnits).Format();
            if(Gate.kind==GateKind.Tax||Gate.kind==GateKind.SubtractMoney)value.text="-"+Mathf.RoundToInt(Gate.loss*100)+"%";
            if(Gate.kind==GateKind.Multiplier)value.text="x1.2";
        }
        private void Update()
        {
            if(collected)return;clock+=Time.deltaTime;
            if(Gate!=null&&GameManager.Instance!=null&&Mathf.Abs(lastProbability-GameManager.Instance.Session.Probability(Gate))>.0001f)RefreshLabel();
            if(Gate!=null&&(Gate.kind==GateKind.Safe||Gate.kind==GateKind.AddMoney)&&!lastMoney.Equals(GameManager.Instance.Session.Money))RefreshLabel();
            if(kind==TrackItemKind.Cash||kind==TrackItemKind.Coin||kind==TrackItemKind.Key||kind==TrackItemKind.Power)
            {
                transform.localPosition=new Vector3(baseX,baseY+Mathf.Sin(clock*2.3f+baseX)*.09f,transform.localPosition.z);
                transform.localRotation=Quaternion.Euler(0,Mathf.Sin(clock)*18,0);
            }
            if(moving>0)transform.localPosition=new Vector3(Mathf.Clamp(baseX+Mathf.Sin(clock*speed+transform.position.z)*moving,-3.3f,3.3f),baseY,transform.localPosition.z);
            if(kind==TrackItemKind.Obstacle)
            {
                if(obstacle=="spinner")transform.localRotation=Quaternion.Euler(0,clock*55,0);
                if(obstacle=="sign")transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(clock*1.5f)*16);
                if(obstacle=="closing")
                {
                    float open=(1+Mathf.Sin(clock*1.1f))*.5f;var left=doorLeft;var right=doorRight;
                    if(left!=null&&right!=null){left.localPosition=new Vector3(-.45f-open*.75f,1.2f,0);right.localPosition=new Vector3(.45f+open*.75f,1.2f,0);if(trigger!=null)trigger.enabled=open<.7f;}
                }
            }
            var game=GameManager.Instance;
            if(game==null||!game.IsRunning)return;
            if(kind==TrackItemKind.Cash&&game.Session.Run.magnet>0&&Vector3.Distance(transform.position,game.World.player.transform.position)<3.6f)Hit();
        }
        private void OnTriggerEnter(Collider other){if(other.GetComponent<RunnerController>()!=null)Hit();}
        public void Hit(){if(collected||GameManager.Instance==null)return;GameManager.Instance.Touch(this);}
        public void FlyToPlayer(){StartCoroutine(Fly());}
        private System.Collections.IEnumerator Fly()
        {
            Vector3 start=transform.position,scale=transform.localScale;
            for(float t=0;t<.18f;t+=Time.deltaTime)
            {
                var game=GameManager.Instance;if(game==null)yield break;
                transform.position=Vector3.Lerp(start,game.World.player.transform.position+Vector3.up,t/.18f);
                transform.localScale=scale*(1-t/.18f);yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}
