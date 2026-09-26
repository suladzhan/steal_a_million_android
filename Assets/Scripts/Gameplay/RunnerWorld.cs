using System;
using System.Collections.Generic;
using StealAMillion.Core;
using UnityEngine;

namespace StealAMillion
{
    public sealed class RunnerWorld : MonoBehaviour
    {
        public RunnerController player;
        public CharacterVisual character;
        public RunnerCamera followCamera;
        public Transform trackRoot;
        public GameObject cashPrefab,safePrefab,riskPrefab,trackPrefab,splitPrefab,vaultPrefab;
        private readonly List<TrackItem> items=new List<TrackItem>();
        private readonly List<Vector2> splits=new List<Vector2>();
        private readonly Queue<Transform> chunks=new Queue<Transform>();
        public LevelData Level {get;private set;}
        public float FinishZ {get;private set;}
        private float builtEnd;
        private int builtChunk;
        private TMPro.TextMeshPro overhead;
        private GameObject previewProp;
        public void ClearPreview(){if(previewProp!=null){previewProp.SetActive(false);Destroy(previewProp);}}
        public void PreviewOutfit(string color){character.PreviewCloth(color);}
        public void PreviewProp(RunnerCosmetic item)
        {
            ClearPreview();previewProp=Instantiate(item.slot==CosmeticSlot.Vault?vaultPrefab:safePrefab,trackRoot);previewProp.transform.position=player.transform.position+new Vector3(1.3f,0,0);previewProp.transform.localScale=Vector3.one*.4f;
            if(item.slot!=CosmeticSlot.Vault)GateArt.Apply(previewProp);
            if(item.slot!=CosmeticSlot.Vault||!FinishArt.Apply(previewProp,item.id))
                foreach(var renderer in previewProp.GetComponentsInChildren<MeshRenderer>())if(renderer.name=="VaultBody"||renderer.name=="Door"||renderer.name=="Banner")renderer.sharedMaterial=RunnerArt.Material(item.color);
            foreach(var col in previewProp.GetComponentsInChildren<Collider>())col.enabled=false;
        }
        public void Build(LevelData level,bool preview)
        {
            foreach(Transform child in trackRoot){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            items.Clear();splits.Clear();chunks.Clear();Level=level;builtChunk=0;
            var game=GameManager.Instance;
            WorldData world=game.Worlds[Mathf.Clamp(game.Progress.world,0,game.Worlds.Count-1)];
            RenderSettings.skybox=null;RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.48f,.53f,.60f);RenderSettings.fog=true;RenderSettings.fogColor=RunnerArt.Color("#7DD8FF");
            RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=65;RenderSettings.fogEndDistance=155;
            Camera.main.backgroundColor=RunnerArt.Color("#7DD8FF");
            if(game.Session.Run.endless&&!preview)
            {
                builtChunk=Mathf.Max(0,game.Session.Run.chunk-1);
                for(int i=0;i<3;i++)BuildChunk(LevelGenerator.Generate(level.number+Math.Min(100,builtChunk),level.seed,level.world,builtChunk),builtChunk++*288,false,world);
            }
            else BuildChunk(level,0,true,world);
            player.Place(preview?0:game.Session.Run.x,preview?0:game.Session.Run.z);
            player.Active=!preview;player.Bounds(-3.6f,3.6f);
            character.ResetPose();character.Running=!preview;character.Preview=preview;
            ApplyCosmetics();followCamera.preview=preview;followCamera.finish=false;followCamera.shop=false;followCamera.Snap();
            if(overhead==null){overhead=RunnerArt.Text(player.transform,"Bankroll","",new Vector3(0,3.1f,0),5.8f,"#16202A",new Vector2(4,.8f));}
            overhead.gameObject.SetActive(false);
        }
        public void ApplyCosmetics(string previewId=null)
        {
            var game=GameManager.Instance;
            var skin=game.Cosmetics.Find(x=>x.id==(previewId??game.Progress.character))??game.Cosmetics[0];
            character.Apply(skin,game.Progress);
            if(game.Progress.outfit!="outfit_classic")
            {
                var outfit=game.Cosmetics.Find(x=>x.id==game.Progress.outfit);
                if(outfit!=null)foreach(var r in character.GetComponentsInChildren<Renderer>())if(r.name=="Shirt"||r.name=="Sleeve")r.sharedMaterial=RunnerArt.Material(outfit.color);
            }
        }
        private void BuildChunk(LevelData level,float start,bool finish,WorldData world)
        {
            if(!finish)
            {
                var segments=new string[16];
                for(int i=0;i<segments.Length;i++)segments[i]=i<level.segments.Length-1?level.segments[i]:"Cash";
                level=new LevelData{number=level.number,seed=level.seed,world=level.world,speed=level.speed,segments=segments};
            }
            var chunk=RunnerArt.Group(trackRoot,"Route "+level.number+" Seed "+level.seed,new Vector3(0,0,start));chunks.Enqueue(chunk);
            var random=new System.Random(unchecked(level.seed+(int)start));
            for(int i=0;i<level.segments.Length;i++)
            {
                string kind=level.segments[i];float z=i*18;
                var road=Instantiate(kind=="Split"?splitPrefab:trackPrefab,chunk);road.name=kind+" "+i;road.transform.localPosition=new Vector3(0,0,z);
                if(kind=="Split")splits.Add(new Vector2(start+z,start+z+18));
                string prefix=((int)start)+":"+i+":";
                float lane=i%3==0?-2:i%3==1?0:2;
                if(kind=="Cash"||kind=="Bonus"||kind=="Split"||kind=="Keys")
                {
                    for(int c=0;c<4;c++)Cash(chunk,new Vector3(kind=="Split"?(c%2==0?-2.6f:2.6f):lane,.65f,z+3+c*3.2f),prefix+"cash"+c,kind=="Bonus"?2:1);
                    if(kind=="Keys")Special(chunk,new Vector3(2.6f,.7f,z+10),prefix+"key",TrackItemKind.Key);
                    if(kind=="Bonus")Special(chunk,new Vector3(-lane,.7f,z+8),prefix+"coin",TrackItemKind.Coin);
                }
                if(kind=="Choice")
                {
                    string left=(level.number==4||level.number>=10&&i%4==0)?"multiplier":level.number>=16&&i%5==0?"luck":level.number>=7&&i%7==0?"shield":"safe";
                    Choice(chunk,z+10,prefix,left,level.number>=14&&i%3==0?"doubleornothing":"risk");
                }
                if(kind=="Jackpot")Choice(chunk,z+10,prefix,"safe","jackpot");
                if(kind=="Chain")Choice(chunk,z+10,prefix,"cashout","riskchain");
                if(kind=="Investment")Choice(chunk,z+7,prefix,"safe",i%2==0?"business":"investment");
                if(kind=="Market")Choice(chunk,z+10,prefix,"safe",level.number%2==0?"market":"mystery");
                if(kind=="Power")
                {
                    Special(chunk,new Vector3(-2,.8f,z+7),prefix+"power",TrackItemKind.Power,(PowerKind)(i%5));
                    for(int c=0;c<3;c++)Cash(chunk,new Vector3(2,.65f,z+4+c*4),prefix+"cash"+c,1);
                    if(level.number>=12&&i%2==0)Gate(chunk,new Vector3(2,0,z+13),prefix+"shield",prefix+"shield",level.number%3==0?"insurance":"shield");
                }
                if(kind=="Tax"||kind=="Police"||kind=="Moving"||kind=="Thief")
                {
                    string type=kind=="Tax"?"tax":kind=="Police"?"police":kind=="Thief"?"thief":new[]{"barrier","spinner","safe","traffic","wall","sign","closing"}[(level.number+i)%7];
                    Obstacle(chunk,new Vector3(lane,0,z+8),prefix+"obstacle",type,kind=="Moving"||kind=="Thief");
                    for(int c=0;c<3;c++)Cash(chunk,new Vector3(lane<=0?2.8f:-2.8f,.65f,z+3+c*4),prefix+"cash"+c,1);
                }
                if(kind=="Finish"&&finish)
                {
                    FinishZ=start+z+6;
                    var finishItem=Gate(chunk,new Vector3(0,0,z+6),prefix+"finish",prefix+"finish","safe");
                    finishItem.kind=TrackItemKind.Finish;finishItem.transform.localScale=new Vector3(2.3f,1.2f,1);
                    finishItem.transform.Find("Title").GetComponent<TMPro.TextMeshPro>().text=GameManager.Instance.Loc.Text("UI_FINISH_GATE");
                    finishItem.transform.Find("Value").GetComponent<TMPro.TextMeshPro>().text="";
                    var vault=Instantiate(vaultPrefab,chunk);vault.transform.localPosition=new Vector3(1.6f,0,z+11);vault.transform.localScale=Vector3.one*.65f;
                    var style=GameManager.Instance.Cosmetics.Find(c=>c.id==GameManager.Instance.Progress.vault);
                    if(!FinishArt.Apply(vault,style==null?"vault_classic":style.id)&&style!=null)foreach(var r in vault.GetComponentsInChildren<MeshRenderer>())if(r.name=="VaultBody"||r.name=="Door")r.sharedMaterial=RunnerArt.Material(style.color);
                }
                Environment(chunk,z,world,random,i);
            }
            builtEnd=start+level.segments.Length*18;
            if(!finish)builtEnd=start+288;
        }
        private void Cash(Transform root,Vector3 position,string id,float scale)
        {
            var go=Instantiate(cashPrefab,root);go.transform.localPosition=position;
            FinishArt.Apply(go,"PROP_cash_stack");
            var item=go.GetComponent<TrackItem>();item.kind=TrackItemKind.Cash;item.id=id;item.scale=scale;Register(item);
        }
        private TrackItem Gate(Transform root,Vector3 position,string id,string group,string gateId)
        {
            bool safe=gateId=="safe"||gateId=="cashout"||gateId=="shield"||gateId=="insurance";
            var go=Instantiate(safe?safePrefab:riskPrefab,root);go.transform.localPosition=position;
            GateArt.Apply(go,gateId);
            var item=go.GetComponent<TrackItem>();item.kind=TrackItemKind.Gate;item.id=id;item.group=group;item.gateId=gateId;Register(item);
            if(gateId=="jackpot")foreach(var r in go.GetComponentsInChildren<MeshRenderer>())if(r.name=="Pillar"||r.name=="Banner")r.sharedMaterial=RunnerArt.Material("#FFC928");
            if(gateId=="mystery")foreach(var r in go.GetComponentsInChildren<MeshRenderer>())if(r.name=="Pillar"||r.name=="Banner")r.sharedMaterial=RunnerArt.Material("#A85CFF");
            return item;
        }
        private void Choice(Transform root,float z,string prefix,string left,string right)
        {
            Gate(root,new Vector3(-2,0,z),prefix+"left",prefix+"choice",left);
            Gate(root,new Vector3(2,0,z),prefix+"right",prefix+"choice",right);
        }
        private void Register(TrackItem item)
        {
            item.Initialize();items.Add(item);
            if(GameManager.Instance.Session.WasConsumed(string.IsNullOrEmpty(item.group)?item.id:item.group))
            {item.collected=true;item.gameObject.SetActive(false);}
        }
        private void Special(Transform root,Vector3 pos,string id,TrackItemKind kind,PowerKind power=PowerKind.Shield)
        {
            var go=RunnerArt.Group(root,kind+" "+id,pos).gameObject;
            string color=kind==TrackItemKind.Power?"#3E78FF":"#FFC928";
            string artId=kind==TrackItemKind.Power?PickupArt.PowerId(power):kind==TrackItemKind.Key?"PROP_key":"PROP_coin";
            if(!PickupArt.Attach(go.transform,artId)){
            var shape=RunnerArt.Part(go.transform,"Token",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.75f,.13f,.75f),color);shape.transform.localRotation=Quaternion.Euler(90,0,0);
            if(kind==TrackItemKind.Key){RunnerArt.Part(go.transform,"Shaft",PrimitiveType.Cube,new Vector3(0,-.4f,0),new Vector3(.18f,.8f,.17f),color);RunnerArt.Part(go.transform,"Tooth",PrimitiveType.Cube,new Vector3(.15f,-.7f,0),new Vector3(.3f,.15f,.17f),color);}
            }
            var col=go.AddComponent<SphereCollider>();col.isTrigger=true;col.radius=.65f;
            var item=go.AddComponent<TrackItem>();item.id=id;item.kind=kind;item.power=power;Register(item);
            if(kind==TrackItemKind.Power)RunnerArt.Text(go.transform,"PowerLabel",GameManager.Instance.Loc.Text("POWER_"+power,Mathf.RoundToInt(GameManager.Instance.Session.Economy.luckBonus*100)),new Vector3(0,.9f,-.15f),2.7f,"#16202A",new Vector2(2.5f,.45f));
        }
        private void Obstacle(Transform root,Vector3 pos,string id,string type,bool moving)
        {
            var go=RunnerArt.Group(root,type+" "+id,pos).gameObject;
            string color=type=="police"?"#3E78FF":type=="thief"?"#41364D":"#FF4D5A";
            if(type=="thief")go.transform.localScale=Vector3.one*.85f;
            if(!DecorationArt.Obstacle(go,type)){
            if(type=="thief")
            {
                RunnerArt.Character(go.transform);go.transform.localScale=Vector3.one*.85f;
                foreach(var r in go.GetComponentsInChildren<Renderer>())if(r.name=="Shirt"||r.name=="Sleeve")r.sharedMaterial=RunnerArt.Material(color);
                RunnerArt.Part(go.transform,"Mask",PrimitiveType.Cube,new Vector3(0,1.8f,-.25f),new Vector3(.48f,.12f,.05f),"#16202A");
            }
            else if(type=="safe")
            {
                RunnerArt.Part(go.transform,"RollingSafe",PrimitiveType.Cube,new Vector3(0,.7f,0),new Vector3(1.25f,1.3f,1.25f),"#70899F");
                var lockFace=RunnerArt.Part(go.transform,"Lock",PrimitiveType.Cylinder,new Vector3(0,.7f,-.65f),new Vector3(.55f,.05f,.55f),"#FFC928");lockFace.transform.localRotation=Quaternion.Euler(90,0,0);
            }
            else if(type=="traffic")
            {
                RunnerArt.Part(go.transform,"Car",PrimitiveType.Cube,new Vector3(0,.6f,0),new Vector3(1.55f,.6f,2.5f),"#FF7769");
                RunnerArt.Part(go.transform,"Cabin",PrimitiveType.Cube,new Vector3(0,1.1f,-.1f),new Vector3(1.2f,.65f,1.3f),"#D5F3FF");
                foreach(int side in new[]{-1,1})foreach(int end in new[]{-1,1}){var wheel=RunnerArt.Part(go.transform,"Wheel",PrimitiveType.Cylinder,new Vector3(side*.76f,.3f,end*.8f),new Vector3(.45f,.13f,.45f),"#253A70");wheel.transform.localRotation=Quaternion.Euler(0,0,90);}
            }
            else if(type=="spinner")
            {
                RunnerArt.Part(go.transform,"Hub",PrimitiveType.Cylinder,new Vector3(0,.6f,0),new Vector3(.4f,.6f,.4f),"#FFC928");
                RunnerArt.Part(go.transform,"RotatingArm",PrimitiveType.Cube,new Vector3(0,1.1f,0),new Vector3(2.4f,.4f,.35f),color);
            }
            else if(type=="closing")
            {
                foreach(int side in new[]{-1,1})RunnerArt.Part(go.transform,side<0?"DoorL":"DoorR",PrimitiveType.Cube,new Vector3(side*.45f,1.2f,0),new Vector3(.8f,2.4f,.3f),color);
                RunnerArt.Part(go.transform,"Frame",PrimitiveType.Cube,new Vector3(0,2.5f,0),new Vector3(2.2f,.18f,.4f),"#566575");
            }
            else if(type=="wall")
            {
                RunnerArt.Part(go.transform,"TaxWall",PrimitiveType.Cube,new Vector3(0,1.3f,0),new Vector3(1.75f,2.6f,.35f),color);
                for(int line=0;line<4;line++)RunnerArt.Part(go.transform,"Receipt",PrimitiveType.Cube,new Vector3(0,.6f+line*.4f,-.19f),new Vector3(line%2==0?1.1f:.7f,.13f,.02f),"#FFFFFF");
            }
            else if(type=="sign")
            {
                RunnerArt.Part(go.transform,"Post",PrimitiveType.Cylinder,new Vector3(0,1.4f,0),new Vector3(.12f,1.4f,.12f),"#566575");
                var sign=RunnerArt.Part(go.transform,"Sign",PrimitiveType.Cube,new Vector3(0,1.7f,0),new Vector3(1.15f,1.15f,.2f),"#FFC928");sign.transform.localRotation=Quaternion.Euler(0,0,45);
            }
            else
            {
                RunnerArt.Part(go.transform,"Barrier",PrimitiveType.Cube,new Vector3(0,.85f,0),new Vector3(1.7f,1.1f,.45f),color);
                for(int i=0;i<3;i++){var stripe=RunnerArt.Part(go.transform,"Stripe",PrimitiveType.Cube,new Vector3(-.58f+i*.58f,.8f,-.24f),new Vector3(.2f,.85f,.025f),"#FFFFFF");stripe.transform.localRotation=Quaternion.Euler(0,0,-24);}
                foreach(int side in new[]{-1,1})RunnerArt.Part(go.transform,"Foot",PrimitiveType.Cube,new Vector3(side*.6f,.15f,0),new Vector3(.22f,.3f,.7f),"#566575");
                if(type=="police")foreach(int side in new[]{-1,1})RunnerArt.Part(go.transform,"Beacon",PrimitiveType.Sphere,new Vector3(side*.5f,1.5f,0),new Vector3(.25f,.25f,.25f),side<0?"#FF4D5A":"#3E78FF");
            }
            }
            RunnerArt.Text(go.transform,"Label",GameManager.Instance.Loc.Text("OBSTACLE_"+type),new Vector3(0,2.25f,0),2.8f,color,new Vector2(2.6f,.5f));
            var col=go.AddComponent<BoxCollider>();col.isTrigger=true;col.center=new Vector3(0,.9f,0);col.size=new Vector3(type=="spinner"?2.4f:1.65f,1.8f,type=="traffic"?2.5f:.75f);
            var item=go.AddComponent<TrackItem>();item.id=id;item.kind=TrackItemKind.Obstacle;item.obstacle=type;item.moving=moving?1.8f:0;item.speed=.65f+RunnerProgress.Difficulty(Level.number);Register(item);
        }
        private void Environment(Transform root,float z,WorldData world,System.Random random,int index)
        {
            bool ocean=world.id=="island"||world.id=="bay";
            RunnerArt.Part(root,"Island",PrimitiveType.Cube,new Vector3(0,ocean?-2.8f:-1.7f,z+9),new Vector3(58,2,18.1f),world.ground);
            if(world.id=="gold")foreach(int side in new[]{-1,1})RunnerArt.Part(root,"GoldRail",PrimitiveType.Cube,new Vector3(side*4.35f,.2f,z+9),new Vector3(.16f,.4f,18),"#FFC928");
            if(world.id=="future")foreach(int side in new[]{-1,1})RunnerArt.Part(root,"NeonRail",PrimitiveType.Cube,new Vector3(side*4.35f,.18f,z+9),new Vector3(.12f,.3f,18),world.accent);
            foreach(int side in new[]{-1,1})
            {
                float x=side*(10+(float)random.NextDouble()*5),height=world.level<20?3+random.Next(4):6+random.Next(12);
                var b=RunnerArt.Group(root,"Building",new Vector3(x,0,z+7));
                if(!DecorationArt.Building(b,world.id)){
                RunnerArt.Part(b,"Building",PrimitiveType.Cube,new Vector3(0,height/2,0),new Vector3(4,height,5),world.building);
                RunnerArt.Part(b,"Roof",PrimitiveType.Cube,new Vector3(0,height+.1f,0),new Vector3(4.3f,.25f,5.3f),world.accent);
                for(int floor=1;floor<height;floor+=2)RunnerArt.Part(b,"Window",PrimitiveType.Cube,new Vector3(-side*2.02f,floor,0),new Vector3(.04f,.85f,3.4f),"#D5F3FF");
                }else b.localScale=Vector3.one*(.86f+(float)random.NextDouble()*.28f);
                if(!DecorationArt.Tree(root,new Vector3(side*6.7f,0,z+13),world.level>=30&&world.id!="future"&&world.id!="gold")){
                RunnerArt.Part(root,"TreeTrunk",PrimitiveType.Cylinder,new Vector3(side*6.2f,.8f,z+13),new Vector3(.25f,.8f,.25f),"#B89788");
                if(world.level>=30&&world.id!="future"&&world.id!="gold")
                    for(int leaf=0;leaf<4;leaf++){var palm=RunnerArt.Part(root,"PalmLeaf",PrimitiveType.Capsule,new Vector3(side*6.2f,2.35f,z+13),new Vector3(.35f,1.4f,.18f),"#35C996");palm.transform.localRotation=Quaternion.Euler(65,leaf*90,0);}
                else RunnerArt.Part(root,"TreeCanopy",PrimitiveType.Sphere,new Vector3(side*6.2f,2.3f,z+13),new Vector3(1.7f,2,1.7f),"#55BC81");
                }
                if(ocean&&index%3==0)
                {
                    var yacht=RunnerArt.Group(root,"Yacht",new Vector3(side*21,-.7f,z+9));
                    RunnerArt.Part(yacht,"Hull",PrimitiveType.Capsule,Vector3.zero,new Vector3(2,.55f,5),"#FFFFFF");
                    RunnerArt.Part(yacht,"Deck",PrimitiveType.Cube,new Vector3(0,.5f,0),new Vector3(1.6f,.45f,3),"#D5F3FF");
                    RunnerArt.Part(yacht,"Roof",PrimitiveType.Cube,new Vector3(0,.9f,-.3f),new Vector3(1.5f,.25f,1.8f),"#FFFFFF");
                }
                if(index%3==0)RunnerArt.Part(root,"Cloud",PrimitiveType.Sphere,new Vector3(side*18,13,z+18),new Vector3(8,2,3.5f),"#FFFFFF");
            }
        }
        public void RefreshGates(){foreach(var item in items)if(item!=null&&item.kind==TrackItemKind.Gate&&!item.collected)item.RefreshLabel();}
        public void CelebrateFinish()
        {
            foreach(var item in items)if(item!=null&&item.kind==TrackItemKind.Finish)item.gameObject.SetActive(false);
            if(overhead!=null)overhead.gameObject.SetActive(false);
            foreach(var vault in trackRoot.GetComponentsInChildren<VaultVisual>())vault.Open();
            character.Preview=true;character.Cheer(4);followCamera.finish=true;
        }
        public void ConsumeGroup(TrackItem selected)
        {
            foreach(var item in items)if(item!=null&&(item==selected||(!string.IsNullOrEmpty(selected.group)&&item.group==selected.group)))
            {item.collected=true;var col=item.GetComponent<Collider>();if(col!=null)col.enabled=false;if(item.kind==TrackItemKind.Cash||item.kind==TrackItemKind.Coin||item.kind==TrackItemKind.Key||item.kind==TrackItemKind.Power)item.FlyToPlayer();}
        }
        private void Update()
        {
            var game=GameManager.Instance;if(game==null||player==null)return;
            bool running=game.IsRunning;player.Active=running||game.IsSuspense;
            player.ForwardSpeed=Level==null?7:Level.speed;
            if(game.IsSuspense)player.ForwardSpeed*=.08f;
            if(game.Session.Run.slowMotion>0)player.ForwardSpeed*=.65f;
            character.Running=running||game.IsSuspense;character.Lean=player.HorizontalVelocity;
            if(followCamera.finish&&!followCamera.preview)
            {
                // Bring every finishing lane into the same portrait victory composition.
                var position=player.transform.position;
                position.x=Mathf.MoveTowards(position.x,-1.15f,Time.deltaTime*6);
                player.transform.position=position;character.Lean=0;
            }
            if(overhead!=null){overhead.text=game.Session.Money.Format();overhead.transform.rotation=followCamera.transform.rotation;}
            if(!running)return;
            float z=player.transform.position.z;float min=-3.6f,max=3.6f;
            foreach(var split in splits)if(z>=split.x-2&&z<split.y+1){if(player.transform.position.x<=0)max=-1.3f;else min=1.3f;break;}
            player.Bounds(min,max);
            for(int i=items.Count-1;i>=0;i--)
            {
                var item=items[i];if(item==null){items.RemoveAt(i);continue;}
                if(item.kind==TrackItemKind.Obstacle&&!item.collected&&z>item.transform.position.z+2)
                {item.collected=true;if(!game.Session.Run.practice)game.Progress.stats.avoided++;game.Session.Run.score+=15;}
            }
            if(game.Session.Run.endless&&z>builtEnd-150)
            {
                BuildChunk(LevelGenerator.Generate(Level.number+Math.Min(100,builtChunk),Level.seed,Level.world,builtChunk),builtChunk*288,false,game.Worlds[game.Progress.world]);builtChunk++;
                while(chunks.Count>3){var old=chunks.Dequeue();if(old!=null)Destroy(old.gameObject);}
                game.Session.Run.chunk=Mathf.FloorToInt(z/288);
                int oldest=Mathf.Max(0,game.Session.Run.chunk-1)*288;
                game.Session.ForgetBefore(oldest);
                splits.RemoveAll(split=>split.y<oldest);
            }
            else if(!game.Session.Run.endless&&z>FinishZ+4)game.Finish();
        }
    }
}
