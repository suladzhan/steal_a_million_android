#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using StealAMillion.Core;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StealAMillion.Tests
{
    public sealed class VisualFlowTests
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        [UnityTest, Timeout(180000)]
        public IEnumerator MajorPresentationAndTexturedArtContracts()
        {
            Resize(540,1200);yield return null;yield return null;
            var game=new GameObject("Major update QA").AddComponent<GameManager>();game.SendMessage("OnApplicationFocus",true);game.ResetProgress();game.Loc.Select("en");game.MainMenu();yield return null;
            Assert.That(ReleaseSettings.Current,Is.Not.Null);Assert.That(Resources.Load<AudioClip>("Runner/Audio/music-v002").length,Is.EqualTo(19.2f).Within(.01f));
            foreach(var world in game.Worlds){var prefab=Resources.Load<GameObject>("Runner/Major/Worlds/"+world.id);Assert.That(prefab,Is.Not.Null,world.id);Assert.That(prefab.GetComponentsInChildren<Collider>().Length,Is.Zero);var mesh=prefab.transform.Find("Model").GetComponentsInChildren<Renderer>();Assert.That(mesh.Length,Is.GreaterThan(0));foreach(var r in mesh)foreach(var material in r.sharedMaterials)Assert.That(material.mainTexture,Is.Not.Null,world.id+" base colour");}
            foreach(string id in new[]{"traffic","barrier","police","thief"})Assert.That(Resources.Load<GameObject>("Runner/Major/Obstacles/"+id).GetComponentsInChildren<Collider>().Length,Is.Zero);
            var female=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Runner/Imported/CHAR_female_Visual"));
            var femaleAnimator=female.GetComponentInChildren<Animator>();Assert.That(femaleAnimator.avatar.isValid&&femaleAnimator.avatar.isHuman,Is.True);
            femaleAnimator.cullingMode=AnimatorCullingMode.AlwaysAnimate;femaleAnimator.Rebind();yield return null;femaleAnimator.Play("ANIM_run",0,0);femaleAnimator.Update(0);var before=femaleAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).position;yield return new WaitForSeconds(.25f);
            Assert.That(Vector3.Distance(before,femaleAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).position),Is.GreaterThan(.01f));
            // Body facing is defined by the pelvis; animated toe joints are not a reliable facing axis.
            var hipRight=femaleAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position-femaleAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position;
            var femaleForward=Vector3.Cross(Vector3.ProjectOnPlane(hipRight,Vector3.up),Vector3.up).normalized;
            foreach(var renderer in female.GetComponentsInChildren<SkinnedMeshRenderer>())Assert.That(renderer.bounds.size.y,Is.InRange(1.3f,2.5f));
            UnityEngine.Object.Destroy(female);yield return null;
            game.World.ApplyCosmetics("businesswoman");yield return new WaitForSeconds(.3f);Assert.That(game.World.character.UsesImported,Is.True);Assert.That(game.World.character.GetComponentInChildren<ImportedRunnerVisual>().IsFemale,Is.True);Capture("major-female",540,1200);game.World.ApplyCosmetics("runner");yield return null;
            Assert.That(Vector3.Dot(femaleForward,Vector3.forward),Is.GreaterThan(.5f),"Female must face along the track");
            game.Progress.character="businesswoman";game.Play(1);yield return new WaitForSeconds(.35f);var gameplayFemaleAnimator=game.World.character.GetComponentInChildren<Animator>();var gameplayRight=gameplayFemaleAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position-gameplayFemaleAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position;Assert.That(Vector3.Dot(Vector3.Cross(Vector3.ProjectOnPlane(gameplayRight,Vector3.up),Vector3.up).normalized,Vector3.forward),Is.GreaterThan(.98f),"Female gameplay facing");Capture("major-female-run",540,1200);game.Progress.character="runner";game.MainMenu();yield return null;
            Capture("major-menu",540,1200);
            game.Play(1);yield return new WaitForSeconds(.3f);Assert.That(game.UI.transform.Find("SafeArea/Stage/View/Swipe guide"),Is.Not.Null);Capture("major-hud",540,1200);
            game.Progress.tutorialMove=game.Progress.tutorialGate=true;yield return null;Assert.That(game.UI.transform.Find("SafeArea/Stage/View/Swipe guide").gameObject.activeSelf,Is.False);
            game.MainMenu();game.Loc.Select("ru");game.UI.Privacy();yield return null;Capture("major-privacy",540,1200);
            game.Play(1);game.World.player.Place(0,game.World.FinishZ-1.5f);yield return new WaitForSeconds(1.8f);Assert.That(game.Session.Run.state,Is.EqualTo(RunnerState.Finished));Assert.That(game.UI.GetComponentsInChildren<GameArt>().Count(a=>a.kind==ArtKind.Star),Is.EqualTo(3));Capture("major-result",540,1200);
        }
        [UnityTest, Timeout(180000)]
        public IEnumerator PlayabilityDecorationAndEffectBudgets()
        {
            var game=new GameObject("Playability QA").AddComponent<GameManager>();game.SendMessage("OnApplicationFocus",true);game.ResetProgress();game.MainMenu();yield return null;
            var obstacle=typeof(RunnerWorld).GetMethod("Obstacle",InstanceFlags);
            foreach(string id in new[]{"tax","police","thief","barrier","spinner","safe","traffic","wall","sign","closing"}){
                var prefab=Resources.Load<GameObject>("Runner/Decor/Obstacles/"+id);Assert.That(prefab,Is.Not.Null,id);
                Assert.That(prefab.GetComponentsInChildren<Collider>().Length,Is.Zero,id);
                Assert.That(prefab.GetComponentsInChildren<Renderer>().Length,Is.LessThanOrEqualTo(12),id);
                obstacle.Invoke(game.World,new object[]{game.World.trackRoot,new Vector3(0,0,500),"qa-"+id,id,true});
                var root=game.World.trackRoot.Find(id+" qa-"+id);Assert.That(root.Find("ObstacleVisual"),Is.Not.Null);
                Assert.That(root.GetComponent<BoxCollider>().isTrigger,Is.True);
                if(id=="closing"){Assert.That(root.Find("ObstacleVisual/DoorL"),Is.Not.Null);Assert.That(root.Find("ObstacleVisual/DoorR"),Is.Not.Null);}
            }
            foreach(var world in game.Worlds){var prefab=Resources.Load<GameObject>("Runner/Decor/Worlds/"+world.id);Assert.That(prefab,Is.Not.Null,world.id);Assert.That(prefab.GetComponentsInChildren<Collider>().Length,Is.Zero);Assert.That(prefab.GetComponentsInChildren<Renderer>().Length,Is.LessThanOrEqualTo(12));}
            var building=new GameObject("Visibility QA");building.transform.SetParent(game.World.trackRoot);building.transform.position=new Vector3(0,0,500);
            Assert.That(DecorationArt.Building(building.transform,game.Worlds[0].id),Is.True);yield return new WaitForSeconds(.3f);
            var visibility=building.GetComponentInChildren<DecorationVisibility>();Assert.That(visibility.Hidden,Is.True);
            building.transform.position=game.World.player.transform.position;yield return new WaitForSeconds(.3f);Assert.That(visibility.Hidden,Is.False);
            var effects=game.GetComponent<RunnerEffects>();int systems=effects.GetComponentsInChildren<ParticleSystem>().Length;
            foreach(SoundCue cue in Enum.GetValues(typeof(SoundCue)))effects.Cue(cue,Vector3.zero,Color.yellow,true);
            Assert.That(effects.GetComponentsInChildren<ParticleSystem>().Length,Is.EqualTo(systems));
            foreach(var p in effects.GetComponentsInChildren<ParticleSystem>()){Assert.That(p.main.maxParticles,Is.LessThanOrEqualTo(64));Assert.That(p.GetComponent<ParticleSystemRenderer>().sharedMaterial,Is.Not.Null);}
            game.UI.gameObject.AddComponent<CanvasGroup>().alpha=0;game.World.followCamera.enabled=false;
            int column=0;foreach(string id in new[]{"tax","police","thief","barrier","spinner","safe","traffic","wall","sign","closing"}){
                var sample=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Runner/Decor/Obstacles/"+id),game.World.trackRoot);
                sample.transform.position=new Vector3((column%5-2)*3.2f,0,column/5*5);column++;
                foreach(var t in sample.GetComponentsInChildren<Transform>())t.gameObject.layer=31;
            }
            Resize(960,540);yield return null;yield return null;Camera.main.cullingMask=1<<31;Camera.main.transform.position=new Vector3(0,9,-16);Camera.main.transform.LookAt(new Vector3(0,.6f,2));Capture("v2-obstacle-styles",960,540);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            var game=GameManager.Instance;
            if(game!=null){UnityEngine.Object.Destroy(game.World.gameObject);UnityEngine.Object.Destroy(game.UI.gameObject);UnityEngine.Object.Destroy(game.gameObject);}
            Time.timeScale=1;yield return null;
        }
        [UnityTest, Timeout(180000)]
        public IEnumerator ImportedInterfaceAndGateContracts()
        {
            Resize(540,1200);yield return null;yield return null;
            var game=new GameObject("Interface art QA").AddComponent<GameManager>();game.SendMessage("OnApplicationFocus",true);game.ResetProgress();game.MainMenu();yield return null;
            var logo=game.UI.GetComponentsInChildren<Image>().First(i=>i.name=="GameLogo");
            Assert.That(logo.sprite,Is.Not.Null);Assert.That(logo.raycastTarget,Is.False);
            var play=game.UI.GetComponentsInChildren<Button>().First(b=>b.name=="UI_PLAY"||b.name=="UI_CONTINUE");
            Assert.That(play.GetComponent<Image>().sprite.name,Does.StartWith("ButtonFace-v001"));
            Assert.That(play.transform.Find("ActionIcon"),Is.Not.Null);
            var gate=typeof(RunnerWorld).GetMethod("Gate",InstanceFlags);
            foreach(var data in game.Gates){
                var item=(TrackItem)gate.Invoke(game.World,new object[]{game.World.trackRoot,new Vector3(0,0,500),"art-"+data.id,"art-"+data.id,data.id});
                Assert.That(item.transform.Find("ImportedFrame"),Is.Not.Null,data.id);
                var crest=item.transform.Find("ImportedFrame/Crest");Assert.That(crest,Is.Not.Null,data.id+" crest mapping");
                Assert.That(crest.GetComponentInChildren<Renderer>().bounds.min.y,Is.GreaterThan(4.6f),"Crest must clear dynamic labels");
                Assert.That(item.GetComponentsInChildren<Collider>().Length,Is.EqualTo(1),data.id);
                Assert.That(item.GetComponent<BoxCollider>().size,Is.EqualTo(new Vector3(3.85f,2.4f,.8f)));
                Assert.That(item.transform.Find("Title").GetComponent<TMP_Text>().text,Is.Not.Empty);
                var banner=item.GetComponentsInChildren<MeshRenderer>().First(r=>r.name=="Banner"&&r.enabled);
                Color expected=data.id=="jackpot"?RunnerArt.Color("#FFC928"):data.id=="mystery"?RunnerArt.Color("#A85CFF"):GameManager.IsRisk(data.kind)?RunnerArt.Color("#FF4D5A"):data.kind==GateKind.Safe||data.kind==GateKind.Multiplier||data.kind==GateKind.CashOut?RunnerArt.Color("#34D17B"):RunnerArt.Color("#3E78FF");
                Assert.That(banner.sharedMaterial.color,Is.EqualTo(expected),data.id+" semantic color");
            }
            Capture("v2-interface-menu",540,1200);
            game.UI.gameObject.AddComponent<CanvasGroup>().alpha=0;game.World.followCamera.enabled=false;
            int column=0;
            foreach(string style in new[]{"safe","risk","jackpot","investment"}){
                var sample=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Runner/Gates/"+style),game.World.trackRoot);
                sample.transform.position=new Vector3((column++-1.5f)*4,0,0);
                foreach(var t in sample.GetComponentsInChildren<Transform>())t.gameObject.layer=31;
            }
            Resize(960,540);yield return null;yield return null;
            Camera.main.cullingMask=1<<31;Camera.main.transform.position=new Vector3(0,4,-18);Camera.main.transform.LookAt(new Vector3(0,2.8f,0));
            Capture("v2-gate-styles",960,540);
        }
        [UnityTest, Timeout(180000)]
        public IEnumerator ImportedVaultsOpenAndCashKeepsTrigger()
        {
            var game=new GameObject("Finish art QA").AddComponent<GameManager>();game.SendMessage("OnApplicationFocus",true);game.ResetProgress();
            foreach(string id in new[]{"vault_classic","vault_gold","vault_diamond","vault_neon"})
            {
                game.Progress.vault=id;game.Play(1);yield return null;
                var vault=game.World.trackRoot.GetComponentInChildren<VaultVisual>();
                Assert.That(vault.transform.Find(id),Is.Not.Null);
                Assert.That(vault.hinge.IsChildOf(vault.transform.Find(id)),Is.True);
                var closed=vault.hinge.localRotation;vault.Open();yield return new WaitForSeconds(1.6f);
                Assert.That(Quaternion.Angle(closed,vault.hinge.localRotation),Is.EqualTo(105).Within(1));
                var expected=Quaternion.AngleAxis(105,vault.hinge.parent.InverseTransformDirection(vault.transform.up))*closed;
                Assert.That(Quaternion.Angle(expected,vault.hinge.localRotation),Is.LessThan(1),"Door must open in the corrected direction");
                var item=game.World.trackRoot.GetComponentsInChildren<TrackItem>().First(t=>t.kind==TrackItemKind.Cash);
                Assert.That(item.transform.Find("PROP_cash_stack"),Is.Not.Null);
                Assert.That(item.GetComponentsInChildren<Collider>().Length,Is.EqualTo(1));
                Assert.That(item.GetComponent<BoxCollider>().size,Is.EqualTo(new Vector3(.95f,1.4f,.9f)));
                Assert.That(item.GetComponent<BoxCollider>().isTrigger,Is.True);
                game.World.PreviewProp(game.Cosmetics.First(c=>c.id==id));yield return null;
                Assert.That(game.World.trackRoot.GetComponentsInChildren<VaultVisual>().Any(v=>v.transform.Find(id)!=null),Is.True);
            }
        }
        [UnityTest, Timeout(180000)]
        public IEnumerator PickupModelsKeepTriggersAndDynamicLabels()
        {
            Assert.That(Environment.GetEnvironmentVariable("SAM_TEST_SAVE_DIRECTORY"),Is.Not.Null.And.Not.Empty);
            var game=new GameObject("Pickup art QA").AddComponent<GameManager>();game.SendMessage("OnApplicationFocus",true);game.ResetProgress();game.MainMenu();
            var special=typeof(RunnerWorld).GetMethod("Special",InstanceFlags);
            foreach(PowerKind power in Enum.GetValues(typeof(PowerKind)))
            {
                string id="art-power-"+power;
                special.Invoke(game.World,new object[]{game.World.trackRoot,new Vector3(((int)power-2)*1.2f,.8f,2),id,TrackItemKind.Power,power});
                var item=game.World.trackRoot.GetComponentsInChildren<TrackItem>().First(t=>t.id==id);
                Assert.That(item.transform.Find(PickupArt.PowerId(power)),Is.Not.Null);
                Assert.That(item.GetComponent<SphereCollider>().radius,Is.EqualTo(.65f));
                Assert.That(item.GetComponent<SphereCollider>().isTrigger,Is.True);
                Assert.That(item.GetComponentsInChildren<Collider>().Length,Is.EqualTo(1));
                Assert.That(item.transform.Find("Token"),Is.Null);
                Assert.That(item.transform.Find("PowerLabel").GetComponent<TMP_Text>().text,Is.Not.Empty);
            }
            yield return null;
            game.UI.gameObject.AddComponent<CanvasGroup>().alpha=0;game.World.followCamera.enabled=false;
            Camera.main.transform.position=new Vector3(0,3,-5);Camera.main.transform.LookAt(new Vector3(0,.8f,2));
            Capture("v2-pickup-models",960,540);
        }
        [UnityTest, Timeout(180000)]
        public IEnumerator ImportedRunnerPreservesMovementAndCosmeticFallback()
        {
            Assert.That(Environment.GetEnvironmentVariable("SAM_TEST_SAVE_DIRECTORY"),Is.Not.Null.And.Not.Empty);
            var game=new GameObject("Imported runner QA").AddComponent<GameManager>();game.SendMessage("OnApplicationFocus",true);game.ResetProgress();
            game.MainMenu();yield return null;yield return null;
            Assert.That(game.World.character.UsesImported,Is.True);
            var animator=game.World.character.GetComponentInChildren<Animator>();
            Assert.That(animator.avatar.isHuman,Is.True);Assert.That(animator.applyRootMotion,Is.False);
            var position=game.World.player.transform.position;
            game.World.character.PreviewVictory("jump");yield return new WaitForSeconds(.5f);
            Assert.That(game.World.player.transform.position,Is.EqualTo(position));
            Capture("v2-imported-jump",540,960);
            game.World.ApplyCosmetics("street");yield return null;
            Assert.That(game.World.character.UsesImported,Is.True,"Street cosmetic must retain animated character");
            Assert.That(game.World.character.GetComponentInChildren<ImportedRunnerVisual>().transform.Find("Hood"),Is.Not.Null,"Street hood must remain visible");
            Assert.That(game.World.character.transform.Find("Body").gameObject.activeSelf,Is.False);
            game.World.ApplyCosmetics("runner");yield return null;
            Assert.That(game.World.character.UsesImported,Is.True);
            game.World.PreviewOutfit("#FF7769");yield return null;
            Assert.That(game.World.character.UsesImported,Is.True,"Outfit preview must retain animated character");
            game.World.ApplyCosmetics();game.Play(1);yield return new WaitForSeconds(.3f);
            Assert.That(game.World.character.UsesImported,Is.True);
            Assert.That(game.World.player.transform.position.z,Is.GreaterThan(0));
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("ANIM_run"),Is.True);
            Capture("v2-imported-run",540,960);
            game.World.character.PreviewVictory("backflip");yield return null;
            Assert.That(game.World.character.UsesImported,Is.True,"Victory preview must retain animated character");
        }
        [UnityTest, Timeout(180000)]
        public IEnumerator FinishArtFitsBothEdgeLanesOnTallPhones()
        {
            Assert.That(Environment.GetEnvironmentVariable("SAM_TEST_SAVE_DIRECTORY"),Is.Not.Null.And.Not.Empty);
            var game=new GameObject("Finish framing QA").AddComponent<GameManager>();game.SendMessage("OnApplicationFocus",true);game.ResetProgress();game.Loc.Select("ru");
            foreach(int height in new[]{960,1200})
            {
                Resize(540,height);yield return null;yield return null;
                foreach(float lane in new[]{-3.6f,3.6f})
                {
                    game.Play(1);yield return null;
                    game.World.player.Place(lane,game.World.FinishZ-1.5f);
                    yield return new WaitForSeconds(1.8f);
                    Assert.That(game.Session.Run.state,Is.EqualTo(RunnerState.Finished));
                    var vault=game.World.trackRoot.GetComponentInChildren<VaultVisual>();
                    Assert.That(vault,Is.Not.Null);
                    var renderers=game.World.character.GetComponentsInChildren<Renderer>().Concat(vault.GetComponentsInChildren<Renderer>());
                    foreach(var renderer in renderers)
                    {
                        var bounds=renderer.bounds;
                        for(int corner=0;corner<8;corner++)
                        {
                            var point=bounds.center+Vector3.Scale(bounds.extents,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1));
                            var projected=Camera.main.WorldToViewportPoint(point);
                            Assert.That(projected.z,Is.GreaterThan(0),renderer.name);
                            Assert.That(projected.x,Is.InRange(.025f,.975f),renderer.name+" horizontal framing");
                            Assert.That(projected.y,Is.InRange(.34f,.67f),renderer.name+" result UI overlap");
                        }
                    }
                    Capture("v2-finish-edge-"+height+"-"+(lane<0?"left":"right"),540,height);
                }
            }
        }
        [UnityTest, Timeout(180000)]
        public IEnumerator ResumeRiskSettingsAndRewardedFlow()
        {
            Assert.That(Environment.GetEnvironmentVariable("SAM_TEST_SAVE_DIRECTORY"),Is.Not.Null.And.Not.Empty);
            var game=new GameObject("Persistence QA").AddComponent<GameManager>();game.SendMessage("OnApplicationFocus",true);game.ResetProgress();game.Loc.Select("en");
            Resize(540,960);game.Play();yield return null;
            var risk=UnityEngine.Object.FindObjectsByType<TrackItem>(FindObjectsSortMode.None).First(x=>x.kind==TrackItemKind.Gate&&x.gateId=="risk");
            game.ForcedRisk=-1;game.World.player.Place(2,risk.transform.position.z-1.5f);yield return new WaitForSeconds(.4f);
            Assert.That(game.Session.Run.state,Is.EqualTo(RunnerState.Suspense));var committed=game.Session.Run.pending.after;
            game.SendMessage("OnApplicationPause",true);float z=game.World.player.transform.position.z;
            yield return new WaitForSecondsRealtime(.15f);Assert.That(game.World.player.transform.position.z,Is.EqualTo(z));
            game.SendMessage("OnApplicationPause",false);game.MainMenu();game.Continue();yield return new WaitForSeconds(1.4f);
            Assert.That(game.Session.Run.pending,Is.Null);Assert.That(game.Session.Money.Equals(committed),Is.True);Assert.That(game.Progress.stats.risks,Is.EqualTo(1));
            game.Session.Hit(1);game.UI.Terminal(false);game.SecondChance();yield return new WaitForSeconds(1.8f);
            Assert.That(game.Session.Run.secondChance,Is.True);Assert.That(game.Session.Run.state,Is.EqualTo(RunnerState.Running));
            game.MainMenu();RunnerProgress.AddCoins(game.Progress,1000);game.Progress.highestLevel=3;game.Purchase("street");
            game.SaveData.soundEnabled=false;game.Language("ru");game.SettingsChanged();
            var restored=new SaveManager().Load();Assert.That(restored.runner.character,Is.EqualTo("street"));Assert.That(restored.runner.language,Is.EqualTo("ru"));Assert.That(restored.soundEnabled,Is.False);
            game.Progress.level=3;game.Play();game.Session.SetMoney(new BigMoney(1000));game.Finish();int coins=game.Progress.coins;
            game.DoubleCoins();yield return new WaitForSeconds(.2f);Assert.That(game.Progress.coins,Is.EqualTo(coins),"No ad during milestone");
            yield return new WaitForSeconds(3.2f);game.DoubleCoins();yield return new WaitForSeconds(1.7f);
            Assert.That(game.Session.Run.doubled,Is.True);int rewarded=game.Progress.coins;game.DoubleCoins();yield return null;Assert.That(game.Progress.coins,Is.EqualTo(rewarded));
        }
        [UnityTest, Timeout(180000)]
        public IEnumerator RoutesWorldsAndEndlessRemainPlayable()
        {
            Assert.That(Environment.GetEnvironmentVariable("SAM_TEST_SAVE_DIRECTORY"),Is.Not.Null.And.Not.Empty);
            var game=new GameObject("Route QA").AddComponent<GameManager>();game.SendMessage("OnApplicationFocus",true);game.ResetProgress();game.Loc.Select("en");
            Resize(540,960);yield return null;
            for(int number=1;number<=20;number++)
            {
                game.Progress.level=game.Progress.highestLevel=number;game.Play(number);yield return null;
                Assert.That(game.World.player.GetComponent<CharacterController>(),Is.Not.Null);
                Assert.That(game.World.trackRoot.GetComponentsInChildren<Collider>().Length,Is.GreaterThan(20));
                var gates=UnityEngine.Object.FindObjectsByType<TrackItem>(FindObjectsSortMode.None).Where(x=>x.kind==TrackItemKind.Gate).ToArray();
                Assert.That(gates.All(g=>g.Gate!=null&&g.GetComponent<Collider>().isTrigger),Is.True);
                game.World.player.Place(0,game.World.FinishZ-1.5f);yield return new WaitForSeconds(.4f);
                Assert.That(game.Session.Run.state,Is.EqualTo(RunnerState.Finished),"Physical finish level "+number);
            }
            for(int world=0;world<game.Worlds.Count;world++)
            {
                game.Progress.world=world;game.MainMenu();yield return null;yield return null;
                Capture("v2-world-"+game.Worlds[world].id,540,960);
            }
            game.Progress.level=game.Progress.highestLevel=30;game.Play(30);yield return null;
            var obstacle=UnityEngine.Object.FindObjectsByType<TrackItem>(FindObjectsSortMode.None).First(x=>x.kind==TrackItemKind.Obstacle&&x.moving==0);
            var before=game.Session.Money;game.World.player.Place(obstacle.transform.position.x,obstacle.transform.position.z-2);
            yield return new WaitForSeconds(.5f);Assert.That(game.Session.Run.hits,Is.EqualTo(1));Assert.That(game.Session.Money<before,Is.True);
            game.Progress.keys=3;game.Play(0,false,true);yield return null;
            Assert.That(game.Progress.keys,Is.Zero);Assert.That(UnityEngine.Object.FindObjectsByType<TrackItem>(FindObjectsSortMode.None).Any(x=>x.kind==TrackItemKind.Obstacle),Is.False);
            game.Play(0,true);yield return null;
            for(int chunk=1;chunk<=8;chunk++)
            {
                game.World.player.Place(0,chunk*288+150);yield return null;yield return null;yield return null;
                Assert.That(game.World.trackRoot.childCount,Is.LessThanOrEqualTo(3));
                Assert.That(game.World.trackRoot.GetComponentsInChildren<TrackItem>().Any(x=>x.transform.position.z>game.World.player.transform.position.z+150),Is.True);
            }
            float distance=game.World.player.transform.position.z;game.MainMenu();game.Continue();yield return null;
            Assert.That(game.World.player.transform.position.z,Is.GreaterThanOrEqualTo(distance));
            Assert.That(game.Progress.stats.bestEndless,Is.GreaterThan(2000));
            game.Finish();Assert.That(game.Session.Run.state,Is.EqualTo(RunnerState.Finished));
        }
        [UnityTest, Timeout(180000)]
        public IEnumerator PortraitRunnerAndPhysicalChoices()
        {
            Assert.That(Environment.GetEnvironmentVariable("SAM_TEST_SAVE_DIRECTORY"), Is.Not.Null.And.Not.Empty);
            var testRunner = GameObject.Find("Code-based tests runner");
            if (testRunner != null) UnityEngine.Object.DontDestroyOnLoad(testRunner);
            var game = new GameObject("Runner Test").AddComponent<GameManager>();
            game.SendMessage("OnApplicationFocus", true);
            game.ResetProgress();
            game.Loc.Select("en");
            Resize(540, 960);
            yield return null;
            game.MainMenu();
            yield return new WaitForSeconds(.3f);
            Capture("v2-menu-960",540,960);
            game.Play();
            yield return new WaitForSeconds(.5f);
            var player=game.World.player;
            Assert.That(player.transform.position.z, Is.GreaterThan(1));
            float oldX=player.transform.position.x;
            player.Drag(.15f);
            yield return new WaitForSeconds(.3f);
            Assert.That(player.transform.position.x, Is.GreaterThan(oldX+.5f));
            var cash=UnityEngine.Object.FindObjectsByType<TrackItem>(FindObjectsSortMode.None).First(x=>x.kind==TrackItemKind.Cash&&!x.collected);
            BigMoney before=game.Session.Money;
            player.Place(cash.transform.position.x,cash.transform.position.z-2);
            yield return new WaitForSeconds(.5f);
            Assert.That(game.Session.Money>before,Is.True,"PhysX pickup trigger");
            var gate=UnityEngine.Object.FindObjectsByType<TrackItem>(FindObjectsSortMode.None).Where(x=>x.kind==TrackItemKind.Gate&&x.gateId=="risk").OrderBy(x=>x.transform.position.z).First();
            player.Place(2,gate.transform.position.z-23);
            game.World.followCamera.Snap();
            yield return null;
            Capture("v2-runner-960",540,960);
            game.Pause();
            float pausedZ=player.transform.position.z;
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(player.transform.position.z,Is.EqualTo(pausedZ));
            game.Resume();
            game.ForcedRisk=1;
            player.Place(2,gate.transform.position.z-2);
            yield return new WaitForSeconds(.6f);
            Assert.That(game.Session.Run.state,Is.EqualTo(RunnerState.Suspense));
            Capture("v2-suspense",540,960);
            yield return new WaitForSeconds(1.4f);
            Assert.That(game.Session.Run.state,Is.EqualTo(RunnerState.Running));
            Assert.That(game.Progress.stats.wins,Is.EqualTo(1));
            player.Place(0,game.World.FinishZ-2);
            yield return new WaitForSeconds(.6f);
            Assert.That(game.Session.Run.state,Is.EqualTo(RunnerState.Finished));
            Capture("v2-finish",540,960);
            foreach(int height in new[]{960,1170,1200})
            {
                Resize(540,height);yield return null;yield return null;
                game.MainMenu();yield return null;Capture("v2-menu-"+height,540,height);
                game.OpenShop();yield return null;Capture("v2-shop-"+height,540,height);
                game.OpenSettings();yield return null;Capture("v2-settings-"+height,540,height);
                game.UI.MissionScreen();yield return null;Capture("v2-missions-"+height,540,height);
                game.UI.Profile();yield return null;Capture("v2-profile-"+height,540,height);
            }
            foreach(string code in new[]{"en","ru","tr","de","ar","zh"})
            {
                game.Loc.Select(code);game.MainMenu();yield return null;yield return null;Capture("v2-locale-"+code,540,1200);
                game.OpenSettings();yield return null;Capture("v2-settings-"+code,540,1200);
                game.OpenShop();yield return null;Capture("v2-shop-"+code,540,1200);
                game.Play();yield return null;
                var firstGate=UnityEngine.Object.FindObjectsByType<TrackItem>(FindObjectsSortMode.None).Where(x=>x.kind==TrackItemKind.Gate).OrderBy(x=>x.transform.position.z).First();
                game.World.player.Place(0,firstGate.transform.position.z-18);game.World.followCamera.Snap();yield return null;
                Capture("v2-gate-"+code,540,1200);
            }
            game.Loc.Select("en");
            game.MainMenu();
        }

        private static void Resize(int width, int height)
        {
            var assembly = typeof(UnityEditor.Editor).Assembly;
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes = singleton.GetProperty("instance", StaticFlags).GetValue(null);
            var groupType = assembly.GetType("UnityEditor.GameViewSizeGroupType");
            string targetGroup = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? "Android" : "Standalone";
            var group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { Enum.Parse(groupType, targetGroup) });
            var sizeType = assembly.GetType("UnityEditor.GameViewSize");
            var modeType = assembly.GetType("UnityEditor.GameViewSizeType");
            object size = Activator.CreateInstance(sizeType, InstanceFlags, null,
                new[] { Enum.Parse(modeType, "FixedResolution"), (object)width, height, "SAM QA " + height }, null);
            int index = (int)group.GetType().GetMethod("GetBuiltinCount").Invoke(group, null)
                + (int)group.GetType().GetMethod("GetCustomCount").Invoke(group, null);
            group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
            var gameViewType = assembly.GetType("UnityEditor.GameView");
            var view = EditorWindow.GetWindow(gameViewType);
            gameViewType.GetProperty("selectedSizeIndex", InstanceFlags).SetValue(view, index);
            view.Repaint();
            var playModeType = assembly.GetType("UnityEditor.PlayModeView");
            playModeType.GetProperty("targetSize", InstanceFlags).SetValue(view, new Vector2(width, height));
        }

        private static void Capture(string name, int width, int height)
        {
            foreach(var reveal in UnityEngine.Object.FindObjectsByType<PanelReveal>(FindObjectsSortMode.None))reveal.Complete();
            Canvas.ForceUpdateCanvases();
            foreach (var art in UnityEngine.Object.FindObjectsByType<GameArt>(FindObjectsSortMode.None))
            {
                Assert.That(art.GetComponent<CanvasRenderer>(), Is.Not.Null, "Missing renderer: " + art.kind);
                Assert.That(art.canvasRenderer.materialCount, Is.GreaterThan(0), "Missing material: " + art.kind);
            }
            var truncations = new System.Collections.Generic.List<string>();
            foreach (var text in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                text.ForceMeshUpdate();
                if (text.isTextTruncated) truncations.Add(text.text + " rect=" + text.rectTransform.rect + " font=" + text.fontSize);
                Assert.That(text.rectTransform.rect.width, Is.GreaterThan(0));
            }
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.Create();
            var camera = Camera.main;
            var canvas = UnityEngine.Object.FindFirstObjectByType<UIManager>().GetComponent<Canvas>();
            var oldMode = canvas.renderMode;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
            camera.targetTexture = target;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var capture = new Texture2D(width, height, TextureFormat.RGB24, false);
            capture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            capture.Apply();
            RenderTexture.active = previous;
            camera.targetTexture = null;
            canvas.renderMode = oldMode;
            string directory = Path.GetFullPath("TestResults/Screenshots");
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory, name + ".png"), capture.EncodeToPNG());
            int bright = capture.GetPixels32().Count(pixel => pixel.r > 100 || pixel.g > 100 || pixel.b > 100);
            UnityEngine.Object.DestroyImmediate(capture);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            Assert.That(bright, Is.GreaterThan(width * height / 100), "Rendered screen is blank: " + name);
            Assert.That(truncations, Is.Empty, "Truncated text in " + name + " screen=" + Screen.width + "x" + Screen.height + ": " + string.Join("; ", truncations));
        }
    }
}
#endif
