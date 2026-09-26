// Used only in the isolated .utmp/asset-review project. Never attaches to game code.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ProductionReview
{
    [Serializable] public class Entry { public string id; public int triangles, renderers; public Vector3 size; public bool scalePass, hierarchyPass; }
    [Serializable] public class Report { public string unity; public Entry[] models; public string[] effects; public string[] animations; public string status; }
    static readonly string Base = "Assets/Production";
    static readonly string Out = "Assets/Production/Prepared";
    static readonly List<string> effectIds = new List<string>();

    public static void Run()
    {
        Directory.CreateDirectory(Out);
        AssetDatabase.Refresh();
        foreach(string path in Directory.GetFiles(Base,"*.png",SearchOption.AllDirectories))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
            bool sprite=path.Contains("/UI/") || path.Contains("\\UI\\");
            importer.textureType=sprite?TextureImporterType.Sprite:TextureImporterType.Default;
            importer.alphaIsTransparency=!path.Contains("palette");
            importer.mipmapEnabled=!sprite && path.Contains("palette");
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=1024;
            if(sprite){importer.spritePixelsPerUnit=100;importer.spriteImportMode=SpriteImportMode.Single;
                if(path.Contains("panel") || path.Contains("button_shape"))importer.spriteBorder=new Vector4(9,9,9,9);
                if(path.Contains("pill_shape"))importer.spriteBorder=new Vector4(64,64,64,64);
            }
            importer.SaveAndReimport();
        }
        var material=new Material(Shader.Find("Standard"));
        material.name="SAM_palette_satin";
        material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Base+"/Shared/SAM_palette__v001.png");
        material.SetFloat("_Glossiness",.24f);
        material.enableInstancing=true;
        AssetDatabase.CreateAsset(material,Out+"/SAM_palette_satin.mat");
        var entries=new List<Entry>();
        foreach(string raw in Directory.GetFiles(Base,"*.fbx",SearchOption.AllDirectories))
        {
            string path=raw.Replace('\\','/');
            var importer=(ModelImporter)AssetImporter.GetAtPath(path);
            importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;importer.importCameras=false;importer.importLights=false;
            importer.addCollider=false;importer.isReadable=false;importer.importAnimation=false;
            importer.materialImportMode=ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var instance=UnityEngine.Object.Instantiate(model);instance.name=Path.GetFileName(path).Split(new[]{"__"},StringSplitOptions.None)[0];
            var renderers=instance.GetComponentsInChildren<MeshRenderer>();
            var bounds=renderers[0].bounds;
            foreach(var renderer in renderers){bounds.Encapsulate(renderer.bounds);renderer.sharedMaterial=material;}
            int triangles=instance.GetComponentsInChildren<MeshFilter>().Sum(filter=>filter.sharedMesh.triangles.Length/3);
            var entry=new Entry{id=instance.name,triangles=triangles,renderers=renderers.Length,size=bounds.size,scalePass=true,hierarchyPass=true};
            if(entry.id=="PROP_cash_stack")entry.scalePass=Near(bounds.size.x,.84f)&&Near(bounds.size.z,.444f);
            if(entry.id.StartsWith("TRK_"))entry.scalePass=Near(bounds.size.z,18)&&Near(bounds.center.z,9);
            if(entry.id=="GATE_frame"){
                entry.scalePass=Near(bounds.size.y,4.78f)&&Near(bounds.size.x,3.56f);
                entry.hierarchyPass=instance.GetComponentsInChildren<Transform>().Any(t=>t.name=="Anchor_Odds"&&Near(t.position.y,2.91f));
            }
            if(entry.id.StartsWith("vault_")){
                entry.scalePass=Near(bounds.size.x,4.8f)&&Near(bounds.size.y,4);
                var hinge=instance.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="DoorHinge");
                entry.hierarchyPass=hinge!=null && Vector3.Distance(hinge.position,new Vector3(-1.6f,2,-1.4f))<.025f;
                if(hinge!=null){
                    var before=hinge.position;var rotation=hinge.rotation;
                    var door=hinge.GetComponentInChildren<MeshRenderer>();
                    var doorCenter=door!=null?door.bounds.center:before;
                    hinge.rotation=Quaternion.AngleAxis(-105,Vector3.up)*rotation;
                    entry.hierarchyPass &= Vector3.Distance(before,hinge.position)<.001f
                        && door!=null && Vector3.Distance(doorCenter,door.bounds.center)>.5f;
                    hinge.rotation=rotation;
                }
            }
            PrefabUtility.SaveAsPrefabAsset(instance,Out+"/"+entry.id+".prefab");
            UnityEngine.Object.DestroyImmediate(instance);entries.Add(entry);
        }
        Effects();
        string[] animationIds=Animations();
        var report=new Report{unity=Application.unityVersion,models=entries.ToArray(),effects=effectIds.ToArray(),animations=animationIds,
            status=entries.All(e=>e.scalePass&&e.hierarchyPass)?"FBX scale/hierarchy pass; integration and Android QA pending":"FAILED FBX contract checks"};
        string output=Environment.GetEnvironmentVariable("SAM_ART_REPORT");
        if(string.IsNullOrEmpty(output))throw new Exception("SAM_ART_REPORT required");
        File.WriteAllText(output,JsonUtility.ToJson(report,true));
        AssetDatabase.SaveAssets();
        if(!entries.All(e=>e.scalePass&&e.hierarchyPass))throw new Exception("FBX contract failed. See "+output);
        string package=Environment.GetEnvironmentVariable("SAM_ART_PACKAGE");
        AssetDatabase.ExportPackage(new[]{Base},package,ExportPackageOptions.Recurse);
        Debug.Log("PRODUCTION_REVIEW_PASS: "+entries.Count+" models, "+effectIds.Count+" particle prefabs, "+animationIds.Length+" motion templates.");
    }
    static bool Near(float a,float b){return Mathf.Abs(a-b)<.025f;}
    static Color Hex(string value){Color color;ColorUtility.TryParseHtmlString(value,out color);return color;}

    static void Effect(string id,string particle,string color,int count,float duration,float speed,float size,bool loop=false)
    {
        string path=Base+"/Particles/PT_"+particle+"__v001.png";
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if(texture==null)throw new Exception("Missing particle texture: "+path);
        string materialPath=Out+"/PT_"+particle+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(material==null){material=new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));material.mainTexture=texture;AssetDatabase.CreateAsset(material,materialPath);}
        var go=new GameObject(id);var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.duration=Mathf.Max(duration,.2f);main.loop=loop;main.startLifetime=duration;main.startSpeed=speed;
        main.startSize=size;main.startColor=Hex(color);main.maxParticles=count;main.playOnAwake=false;
        main.simulationSpace=ParticleSystemSimulationSpace.World;main.gravityModifier=particle=="confetti"||particle=="note"?.4f:0;
        var emission=ps.emission;emission.enabled=true;emission.rateOverTime=loop?Mathf.Min(count/Mathf.Max(duration,.1f),12):0;
        if(!loop)emission.SetBursts(new[]{new ParticleSystem.Burst(0,(short)count)});
        var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.10f;
        var sizeLife=ps.sizeOverLifetime;sizeLife.enabled=true;sizeLife.size=new ParticleSystem.MinMaxCurve(1,particle=="ring"?AnimationCurve.EaseInOut(0,.1f,1,1):AnimationCurve.EaseInOut(0,1,1,.1f));
        if(id=="FX_police")main.startColor=new ParticleSystem.MinMaxGradient(Hex("#FF4D5A"),Hex("#3E78FF"));
        if(id.Contains("rainbow")||id=="FX_finish"||id=="GATE_gate_confetti"){
            var spectrum=new Gradient();spectrum.SetKeys(new[]{new GradientColorKey(Hex("#FF4D5A"),0),new GradientColorKey(Hex("#FFC928"),.25f),new GradientColorKey(Hex("#35D06F"),.5f),new GradientColorKey(Hex("#3E78FF"),.75f),new GradientColorKey(Hex("#A85CFF"),1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,1)});
            var randomColor=new ParticleSystem.MinMaxGradient(spectrum);randomColor.mode=ParticleSystemGradientMode.RandomColor;main.startColor=randomColor;
        }
        var fade=ps.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();
        gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(0,1)});fade.color=gradient;
        var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
        PrefabUtility.SaveAsPrefabAsset(go,Out+"/"+id+".prefab");UnityEngine.Object.DestroyImmediate(go);effectIds.Add(id);
    }
    static void Effects()
    {
        Effect("FX_cash_pickup","spark","#A9FFBE",6,.25f,.8f,.13f);
        Effect("FX_safe_pass","ring","#34D17B",1,.35f,0,.8f);
        Effect("FX_risk_suspense","soft","#FF4D5A",3,.6f,.03f,.13f,true);
        Effect("FX_risk_win","spark","#FFC928",16,.4f,1.2f,.15f);
        Effect("FX_risk_loss","ring","#FF4055",1,.3f,0,.9f);
        Effect("FX_tax","note","#FFFFFF",5,.5f,.6f,.15f);
        Effect("FX_police","soft","#3E78FF",2,.25f,.1f,.16f);
        Effect("FX_thief","dust","#FFFFFF",5,.3f,.5f,.2f);
        Effect("FX_hit","spark","#FFF5B1",8,.3f,.8f,.12f);
        Effect("FX_jackpot","note","#FFFFFF",32,.8f,1.4f,.18f);
        Effect("FX_investment_return","spark","#35D06F",8,.4f,1,.14f);
        Effect("FX_shield_break","ring","#3E78FF",4,.3f,.6f,.25f);
        Effect("FX_magnet","streak","#7DD8FF",4,.3f,.5f,.10f,true);
        Effect("FX_luck","spark","#35D06F",3,.35f,.25f,.10f,true);
        Effect("FX_double_cash","spark","#35D06F",2,.25f,.3f,.13f,true);
        Effect("FX_slow_motion","streak","#7DD8FF",4,.4f,.2f,.15f,true);
        Effect("FX_finish","confetti","#FFC928",48,1,1.3f,.10f);
        Effect("FX_milestone","spark","#FFC928",20,.7f,1,.14f);
        Effect("FX_world_unlock","spark","#7DD8FF",6,.4f,.5f,.13f);
        Effect("FX_achievement","ring","#FFC928",1,.4f,0,.6f);
        Effect("FX_level_up","spark","#3E78FF",6,.4f,.5f,.16f);
        string[] trails={"dust","cash_trail","coin_trail","gold_trail","neon_trail","rainbow_trail"};
        string[] textures={"dust","note","coin","spark","streak","confetti"};
        string[] colors={"#FFFFFF","#FFFFFF","#FFFFFF","#FFC928","#B67AFF","#FF7769"};
        for(int i=0;i<trails.Length;i++)Effect("TRAIL_"+trails[i],textures[i],colors[i],12,.35f,.1f,.10f,true);
        string[] money={"cash_green","cash_gold","cash_neon","cash_blue","cash_rainbow","cash_spark"};
        string[] mt={"note","ring","ring","streak","confetti","spark"};
        string[] mc={"#FFFFFF","#FFC928","#B67AFF","#7DD8FF","#FF7769","#FFFFFF"};
        for(int i=0;i<money.Length;i++)Effect("MONEY_"+money[i],mt[i],mc[i],i==1||i==2?1:5,.35f,.6f,.13f);
        Effect("GATE_gate_classic","ring","#FFFFFF",1,.35f,0,.8f);
        Effect("GATE_gate_spark","spark","#FFF5B1",8,.35f,.6f,.13f);
        Effect("GATE_gate_confetti","confetti","#FFFFFF",16,.5f,.9f,.10f);
        Effect("FX_wealth_aura","spark","#FFC928",6,.4f,.15f,.08f,true);
        Effect("FX_wealth_legendary","spark","#FFC928",8,.5f,.6f,.10f);
    }
    static string[] Animations()
    {
        string[] names={"press","panel","tab","currency","progress","reward","unlock","milestone","toast","wait"};
        foreach(string name in names){
            var clip=new AnimationClip();clip.name="UIA_"+name;clip.frameRate=60;
            if(name=="press" || name=="currency"){
                float peak=name=="press"?.96f:1.08f;
                foreach(string axis in new[]{"x","y","z"})clip.SetCurve("",typeof(Transform),"m_LocalScale."+axis,new AnimationCurve(new Keyframe(0,1),new Keyframe(.08f,peak),new Keyframe(.20f,1)));
            }else if(name=="progress"){
                var imageType=Type.GetType("UnityEngine.UI.Image, UnityEngine.UI");
                if(imageType==null)throw new Exception("uGUI Image missing");
                clip.SetCurve("",imageType,"m_FillAmount",AnimationCurve.EaseInOut(0,0,.35f,1));
            }else if(name=="tab"){
                clip.SetCurve("Marker",typeof(RectTransform),"m_AnchoredPosition.x",AnimationCurve.EaseInOut(0,0,.15f,48));
            }else if(name=="unlock"){
                clip.SetCurve("Lock",typeof(CanvasGroup),"m_Alpha",AnimationCurve.EaseInOut(0,1,.3f,0));
                clip.SetCurve("Check",typeof(CanvasGroup),"m_Alpha",AnimationCurve.EaseInOut(0,0,.3f,1));
            }else if(name=="wait"){
                clip.SetCurve("",typeof(Transform),"localEulerAnglesRaw.z",AnimationCurve.Linear(0,0,1,-360));
                var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);
            }else {
                float duration=name=="unlock"||name=="milestone"?.3f:name=="progress"?.35f:name=="reward"?.25f:.18f;
                clip.SetCurve("",typeof(CanvasGroup),"m_Alpha",AnimationCurve.EaseInOut(0,0,duration,1));
            }
            AssetDatabase.CreateAsset(clip,Out+"/UIA_"+name+".anim");
        }
        return names.Select(n=>"UIA_"+n).ToArray();
    }
}
