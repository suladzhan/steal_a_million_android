using System;
using System.IO;
using System.Linq;
using StealAMillion.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace StealAMillion.Editor
{
    public static class RunnerSetup
    {
        private const string Root="Assets/Resources/Runner/";
        [MenuItem("Steal A Million/Build Runner Assets")]
        public static void Configure()
        {
            Directory.CreateDirectory(Root+"Materials");Directory.CreateDirectory(Root+"Fonts");AssetDatabase.Refresh();
            if(AssetDatabase.LoadAssetAtPath<RunnerConfig>(Root+"Balance.asset")==null)AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RunnerConfig>(),Root+"Balance.asset");
            EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<RunnerConfig>(Root+"Balance.asset"));
            if(AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/UI.mat")==null)
                AssetDatabase.CreateAsset(new Material(Shader.Find("UI/Default")),Root+"Materials/UI.mat");
            if(AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/Particles.mat")==null)
            {
                var particles=new Material(Shader.Find("Particles/Standard Unlit"));particles.SetFloat("_Mode",0);
                AssetDatabase.CreateAsset(particles,Root+"Materials/Particles.mat");
            }
            var cash=Prefab("Cash",go=>{RunnerArt.Cash(go.transform);go.AddComponent<TrackItem>().kind=TrackItemKind.Cash;});
            var safe=Prefab("SafeGate",go=>{RunnerArt.Gate(go.transform,"#34D17B");go.AddComponent<TrackItem>().kind=TrackItemKind.Gate;});
            var risk=Prefab("RiskGate",go=>{RunnerArt.Gate(go.transform,"#FF4D5A");go.AddComponent<TrackItem>().kind=TrackItemKind.Gate;});
            var track=Prefab("StraightSegment",go=>RunnerArt.Track(go.transform));
            var split=Prefab("SplitSegment",go=>RunnerArt.Track(go.transform,true));
            var vault=Prefab("FinishVault",go=>RunnerArt.Vault(go.transform));
            var runner=Prefab("Player",go=>{
                RunnerArt.Character(go.transform);var capsule=go.AddComponent<CharacterController>();capsule.height=1.9f;capsule.radius=.28f;capsule.center=new Vector3(0,.95f,0);capsule.stepOffset=.15f;
                go.AddComponent<RunnerController>();go.AddComponent<CharacterVisual>();
            });
            var stage=Prefab("Stage",go=>{
                var world=go.AddComponent<RunnerWorld>();world.cashPrefab=cash;world.safePrefab=safe;world.riskPrefab=risk;world.trackPrefab=track;world.splitPrefab=split;world.vaultPrefab=vault;
                world.trackRoot=RunnerArt.Group(go.transform,"Generated Track",Vector3.zero);
                var player=(GameObject)PrefabUtility.InstantiatePrefab(runner,go.transform);world.player=player.GetComponent<RunnerController>();world.character=player.GetComponent<CharacterVisual>();
                var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(RunnerCamera));camera.tag="MainCamera";camera.transform.SetParent(go.transform,false);
                var cam=camera.GetComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=RunnerArt.Color("#7DD8FF");cam.farClipPlane=180;cam.nearClipPlane=.15f;
                world.followCamera=camera.GetComponent<RunnerCamera>();world.followCamera.target=player.transform;world.followCamera.preview=true;world.followCamera.Snap();
                var light=new GameObject("Sun",typeof(Light));light.transform.SetParent(go.transform,false);light.transform.rotation=Quaternion.Euler(42,-35,0);
                var sun=light.GetComponent<Light>();sun.type=LightType.Directional;sun.intensity=.78f;sun.color=new Color(1,.98f,.92f);sun.shadows=LightShadows.Soft;sun.shadowStrength=.35f;
            });
            foreach(string path in ProjectSetup.Scenes)
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                PrefabUtility.InstantiatePrefab(stage,scene);
                var canvas=new GameObject("Runner Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(UIManager));
                canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
                new GameObject("GameManager",typeof(GameManager));
                RenderSettings.skybox=null;RenderSettings.ambientLight=new Color(.77f,.84f,.92f);RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
                EditorSceneManager.SaveScene(scene,path);
            }
            EditorBuildSettings.scenes=ProjectSetup.Scenes.Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();
            PlayerSettings.bundleVersion=ReleaseReadiness.Version;PlayerSettings.Android.bundleVersionCode=ReleaseReadiness.Code;
            QualitySettings.shadows=ShadowQuality.HardOnly;QualitySettings.shadowDistance=30;QualitySettings.antiAliasing=2;
            Validate();AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Debug.Log("V2 runner scenes, player, colliders, camera, canvas, shared materials and modular prefabs are saved.");
        }
        private static GameObject Prefab(string name,Action<GameObject> build)
        {
            var go=new GameObject(name);build(go);
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                var mat=renderer.sharedMaterial;if(mat==null||AssetDatabase.Contains(mat))continue;
                if(mat.name.StartsWith("M")&&mat.name.Length==7)
                {
                    string path=Root+"Materials/"+mat.name+".mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(existing!=null)renderer.sharedMaterial=existing;else{mat.enableInstancing=true;AssetDatabase.CreateAsset(mat,path);}
                }
            }
            var prefab=PrefabUtility.SaveAsPrefabAsset(go,Root+name+".prefab");UnityEngine.Object.DestroyImmediate(go);return prefab;
        }
        [MenuItem("Steal A Million/Validate Runner Content")]
        public static void Validate()
        {
            var config=Resources.Load<RunnerConfig>("Runner/Balance");
            if(config==null||config.economy==null||config.economy.pickupGrowth<1||config.economy.pickupGrowth>2||config.economy.growthStartsAt<1||config.economy.startingMoney<1)
                throw new InvalidOperationException("Invalid runner economy configuration");
            var tiers=config.economy.riskTiers;
            if(tiers==null||tiers.Length==0||tiers[0].level!=1||tiers.Any(t=>t.probability<0||t.probability>1||t.multiplier<1))
                throw new InvalidOperationException("Invalid risk tiers");
            if(Resources.Load<Material>("Runner/Materials/Particles")==null)throw new InvalidOperationException("Build Runner Assets before packaging: missing particle material.");
            var ui=Resources.Load<Material>("Runner/Materials/UI");
            if(ui==null||ui.shader==null||ui.shader.name!="UI/Default")throw new InvalidOperationException("Build Runner Assets before packaging: missing UI shader reference.");
            var catalog=JsonUtility.FromJson<LevelCatalog>(Resources.Load<TextAsset>("RunnerData/levels").text);
            foreach(var level in catalog.levels){var errors=LevelGenerator.Validate(level);if(errors.Count>0)throw new InvalidOperationException("Level "+level.number+": "+string.Join(", ",errors));}
            for(int level=21;level<=500;level++)if(LevelGenerator.Validate(LevelGenerator.Generate(level,713+level*1949,0)).Count>0)throw new InvalidOperationException("Generated level invalid: "+level);
            Debug.Log("Validated "+catalog.levels.Length+" curated routes and generation through level 500.");
        }
    }
}
