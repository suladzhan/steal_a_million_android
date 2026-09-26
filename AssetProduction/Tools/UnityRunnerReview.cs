using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class RunnerReview
{
    const string Folder="Assets/Production/CHAR_runner";
    [Serializable] public class ClipReport { public string name; public float duration,rootDrift,poseChange,minY,maxY; public bool pass; }
    [Serializable] public class Report { public string unity,status; public bool avatarValid,avatarHuman; public int triangles,bones; public Vector3 restSize; public float restBottom,forwardDot; public ClipReport[] clips; }
    static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
    static void Setup(ModelImporter importer){
        importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;
        importer.preserveHierarchy=true;
        importer.importCameras=false;importer.importLights=false;importer.addCollider=false;
        importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.isReadable=true;
        importer.animationCompression=ModelImporterAnimationCompression.Off;
    }
    public static void Run(){
        AssetDatabase.Refresh();
        string reportPath=Environment.GetEnvironmentVariable("SAM_ART_REPORT");
        string modelPath=Folder+"/CHAR_runner__model__v001.fbx";
        var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);Setup(importer);
        importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=false;importer.SaveAndReimport();
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        string[] humanNames={"Hips","LeftUpperLeg","LeftLowerLeg","LeftFoot","LeftToes","RightUpperLeg","RightLowerLeg","RightFoot","RightToes","Spine","Chest","UpperChest","Neck","Head","LeftShoulder","LeftUpperArm","LeftLowerArm","LeftHand","RightShoulder","RightUpperArm","RightLowerArm","RightHand"};
        string[] boneNames={"Hips","LeftUpLeg","LeftLeg","LeftFoot","LeftToeBase","RightUpLeg","RightLeg","RightFoot","RightToeBase","Spine02","Spine01","Spine","neck","Head","LeftShoulder","LeftArm","LeftForeArm","LeftHand","RightShoulder","RightArm","RightForeArm","RightHand"};
        var human=humanNames.Select((n,i)=>new HumanBone{humanName=n,boneName=boneNames[i],limit=new HumanLimit{useDefaultValues=true}}).ToArray();
        var reference=UnityEngine.Object.Instantiate(model);reference.name=model.name;
        var byName=reference.GetComponentsInChildren<Transform>().ToDictionary(t=>t.name);
        foreach(string side in new[]{"Left","Right"}){
            Vector3 direction=side=="Left"?Vector3.left:Vector3.right;
            Align(byName[side+"Arm"],byName[side+"ForeArm"],direction);
            Align(byName[side+"ForeArm"],byName[side+"Hand"],direction);
            Align(byName[side+"UpLeg"],byName[side+"Leg"],Vector3.down);
            Align(byName[side+"Leg"],byName[side+"Foot"],Vector3.down);
        }
        var skeleton=reference.GetComponentsInChildren<Transform>().Select(t=>new SkeletonBone{name=t.name,position=t.localPosition,rotation=t.localRotation,scale=t.localScale}).ToArray();
        UnityEngine.Object.DestroyImmediate(reference);
        importer.animationType=ModelImporterAnimationType.Human;
        importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
        importer.humanDescription=new HumanDescription{human=human,skeleton=skeleton,upperArmTwist=.5f,lowerArmTwist=.5f,upperLegTwist=.5f,lowerLegTwist=.5f,armStretch=.05f,legStretch=.05f,feetSpacing=0,hasTranslationDoF=false};
        importer.SaveAndReimport();
        model=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        var avatar=AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault();
        Require(avatar!=null && avatar.isValid && avatar.isHuman,"Humanoid avatar invalid");
        var texImporter=(TextureImporter)AssetImporter.GetAtPath(Folder+"/CHAR_runner__v001__basecolor.png");
        texImporter.maxTextureSize=2048;texImporter.mipmapEnabled=true;texImporter.sRGBTexture=true;texImporter.SaveAndReimport();
        var mat=new Material(Shader.Find("Standard")){name="Runner_Satin",mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/CHAR_runner__v001__basecolor.png")};mat.SetFloat("_Glossiness",.15f);
        AssetDatabase.CreateAsset(mat,Folder+"/Runner_Satin.mat");
        var root=new GameObject("CHAR_runner_Visual");
        var visual=UnityEngine.Object.Instantiate(model,root.transform);
        var renderer=visual.GetComponentInChildren<SkinnedMeshRenderer>();renderer.sharedMaterial=mat;renderer.updateWhenOffscreen=true;
        var animator=visual.GetComponent<Animator>();animator.avatar=avatar;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        var rest=BakedBounds(renderer);
        Render(root,Path.Combine(Path.GetDirectoryName(reportPath),"rest_front__unity.png"),new Vector3(0,1,5));
        Render(root,Path.Combine(Path.GetDirectoryName(reportPath),"rest_back__unity.png"),new Vector3(0,1,-5));
        Render(root,Path.Combine(Path.GetDirectoryName(reportPath),"rest_side__unity.png"),new Vector3(5,1,0));
        var report=new Report{unity=Application.unityVersion,avatarValid=avatar.isValid,avatarHuman=avatar.isHuman,
            triangles=renderer.sharedMesh.triangles.Length/3,bones=renderer.bones.Length,restSize=rest.size,restBottom=rest.min.y};
        // Toe direction establishes forward without relying on the FBX root rotation.
        var foot=animator.GetBoneTransform(HumanBodyBones.LeftFoot);var toe=animator.GetBoneTransform(HumanBodyBones.LeftToes);
        report.forwardDot=Vector3.Dot((toe.position-foot.position).normalized,Vector3.forward);
        var controller=AnimatorController.CreateAnimatorControllerAtPath(Folder+"/RunnerReview.controller");
        var clips=new List<AnimationClip>();
        foreach(string file in Directory.GetFiles(Folder,"ANIM_*__v001.fbx")){
            var ci=(ModelImporter)AssetImporter.GetAtPath(file);Setup(ci);ci.animationType=ModelImporterAnimationType.Human;
            ci.avatarSetup=ModelImporterAvatarSetup.CopyFromOther;ci.sourceAvatar=avatar;ci.importAnimation=true;ci.SaveAndReimport();
            var settings=ci.defaultClipAnimations;
            foreach(var c in settings){c.name=Path.GetFileNameWithoutExtension(file).Replace("__v001","");
                c.loopTime=c.name=="ANIM_run"||c.name=="ANIM_idle"||c.name=="ANIM_victory_dance"||c.name=="ANIM_preview";
                c.loopPose=c.loopTime;c.lockRootRotation=true;c.keepOriginalOrientation=true;
                c.lockRootPositionXZ=true;c.keepOriginalPositionXZ=true;c.lockRootHeightY=true;c.keepOriginalPositionY=true;
                if(c.name=="ANIM_idle"){c.keepOriginalPositionY=false;c.heightFromFeet=true;c.heightOffset=.12844f;}
                c.events=new AnimationEvent[0];}
            ci.clipAnimations=settings;ci.SaveAndReimport();
            var sourceClip=AssetDatabase.LoadAllAssetsAtPath(file).OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview__"));
            var clip=sourceClip;
            AssetDatabase.DeleteAsset(Folder+"/"+clip.name+".anim");
            clips.Add(clip);var state=controller.layers[0].stateMachine.AddState(clip.name);state.motion=clip;
        }
        animator.runtimeAnimatorController=controller;animator.Rebind();animator.Update(0);
        var results=new List<ClipReport>();
        foreach(var clip in clips){
            var cr=new ClipReport{name=clip.name,duration=clip.length,minY=float.MaxValue,maxY=float.MinValue};
            Vector3 start=visual.transform.position;Vector3[] first=null;
            for(int i=0;i<=Mathf.CeilToInt(clip.length*30);i++){
                float normalized=Mathf.Min(i/30f/clip.length,.99999f);
                animator.Play(clip.name,0,normalized);animator.Update(0);
                var baked=new Mesh();renderer.BakeMesh(baked,true);var vertices=baked.vertices;
                if(first==null)first=vertices;
                for(int v=0;v<vertices.Length;v++){
                    var world=renderer.transform.TransformPoint(vertices[v]);
                    Require(float.IsFinite(world.x)&&float.IsFinite(world.y)&&float.IsFinite(world.z),"Invalid vertex");
                    cr.minY=Mathf.Min(cr.minY,world.y);cr.maxY=Mathf.Max(cr.maxY,world.y);
                    cr.poseChange=Mathf.Max(cr.poseChange,renderer.transform.TransformVector(first[v]-vertices[v]).magnitude);
                }
                UnityEngine.Object.DestroyImmediate(baked);
                cr.rootDrift=Mathf.Max(cr.rootDrift,Vector3.Distance(start,visual.transform.position));
                if(i==Mathf.CeilToInt(clip.length*30)/2)Render(root,Path.Combine(Path.GetDirectoryName(reportPath),clip.name+"__unity.png"));
            }
            cr.pass=cr.rootDrift<.001f && cr.poseChange>.001f && Mathf.Abs(cr.minY)<.03f && cr.maxY>1.4f && cr.maxY<3.0f && clip.events.Length==0;
            results.Add(cr);
        }
        report.clips=results.ToArray();
        bool pass=Mathf.Abs(rest.size.y-1.8f)<.03f && Mathf.Abs(rest.min.y)<.03f && report.forwardDot>.4f && results.Count==7 && results.All(c=>c.pass);
        report.status=pass?"PASS isolated Humanoid import and sampled deformation; gameplay/device QA pending":"FAILED character contract";
        File.WriteAllText(reportPath,JsonUtility.ToJson(report,true));
        animator.Play("ANIM_idle",0,0);animator.Update(0);
        PrefabUtility.SaveAsPrefabAsset(root,Folder+"/CHAR_runner_Visual.prefab");
        AssetDatabase.SaveAssets();Require(pass,"Character contract failed; see report");
        AssetDatabase.ExportPackage(Folder,Environment.GetEnvironmentVariable("SAM_ART_PACKAGE"),ExportPackageOptions.Recurse);
        Debug.Log("RUNNER_REVIEW_PASS");
    }
    static void Align(Transform parent,Transform child,Vector3 direction){parent.rotation=Quaternion.FromToRotation(child.position-parent.position,direction)*parent.rotation;}
    static Bounds BakedBounds(SkinnedMeshRenderer renderer){var mesh=new Mesh();renderer.BakeMesh(mesh,true);var b=new Bounds(renderer.transform.TransformPoint(mesh.vertices[0]),Vector3.zero);foreach(var v in mesh.vertices)b.Encapsulate(renderer.transform.TransformPoint(v));UnityEngine.Object.DestroyImmediate(mesh);return b;}
    static void Render(GameObject root,string path,Vector3? position=null){
        foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=31;
        var camGo=new GameObject("ReviewCamera");var camera=camGo.AddComponent<Camera>();
        camera.cullingMask=1<<31;
        camera.transform.position=position??new Vector3(3,2.2f,5);camera.transform.LookAt(new Vector3(0,1,0));
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.13f,.16f,.21f);camera.orthographic=true;camera.orthographicSize=1.35f;
        var lightGo=new GameObject("ReviewLight");var light=lightGo.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.transform.rotation=Quaternion.Euler(40,-130,0);
        RenderSettings.ambientLight=new Color(.55f,.55f,.55f);
        var target=new RenderTexture(540,720,24);camera.targetTexture=target;camera.Render();
        var previous=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(540,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,540,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
        RenderTexture.active=previous;camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(camGo);UnityEngine.Object.DestroyImmediate(lightGo);
        foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=0;
    }
}
