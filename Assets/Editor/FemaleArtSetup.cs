using System;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StealAMillion.Editor {
public static class FemaleArtSetup {
 const string Folder="Assets/Art/Major", ModelPath=Folder+"/female.fbx";
 public static void Build(){
  AssetDatabase.Refresh();var importer=(ModelImporter)AssetImporter.GetAtPath(ModelPath);
  importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;importer.preserveHierarchy=true;importer.importCameras=false;importer.importLights=false;importer.addCollider=false;importer.importAnimation=false;importer.isReadable=true;importer.materialImportMode=ModelImporterMaterialImportMode.None;
  importer.animationType=ModelImporterAnimationType.Generic;importer.SaveAndReimport();
  var model=AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);var reference=UnityEngine.Object.Instantiate(model);reference.name=model.name;
  var bones=reference.GetComponentsInChildren<Transform>().GroupBy(t=>t.name).ToDictionary(g=>g.Key,g=>g.First());
  string[] human={"Hips","LeftUpperLeg","LeftLowerLeg","LeftFoot","LeftToes","RightUpperLeg","RightLowerLeg","RightFoot","RightToes","Spine","Chest","UpperChest","Neck","Head","LeftShoulder","LeftUpperArm","LeftLowerArm","LeftHand","RightShoulder","RightUpperArm","RightLowerArm","RightHand"};
  string[] names={"Hips","LeftUpLeg","LeftLeg","LeftFoot","LeftToeBase","RightUpLeg","RightLeg","RightFoot","RightToeBase","Spine02","Spine01","Spine","neck","Head","LeftShoulder","LeftArm","LeftForeArm","LeftHand","RightShoulder","RightArm","RightForeArm","RightHand"};
  foreach(var n in names)if(!bones.ContainsKey(n))throw new Exception("Female bone missing: "+n);
  var restForward=Vector3.ProjectOnPlane(bones["LeftToeBase"].position-bones["LeftFoot"].position,Vector3.up).normalized;
  reference.transform.rotation=Quaternion.FromToRotation(restForward,Vector3.forward)*reference.transform.rotation;
  foreach(string side in new[]{"Left","Right"}){var direction=side=="Left"?Vector3.left:Vector3.right;Align(bones[side+"Arm"],bones[side+"ForeArm"],direction);Align(bones[side+"ForeArm"],bones[side+"Hand"],direction);Align(bones[side+"UpLeg"],bones[side+"Leg"],Vector3.down);Align(bones[side+"Leg"],bones[side+"Foot"],Vector3.down);Align(bones[side+"Foot"],bones[side+"ToeBase"],Vector3.forward);}
  importer.humanDescription=new HumanDescription{human=human.Select((n,i)=>new HumanBone{humanName=n,boneName=names[i],limit=new HumanLimit{useDefaultValues=true}}).ToArray(),skeleton=reference.GetComponentsInChildren<Transform>().Select(t=>new SkeletonBone{name=t.name,position=t.localPosition,rotation=t.localRotation,scale=t.localScale}).ToArray(),upperArmTwist=.5f,lowerArmTwist=.5f,upperLegTwist=.5f,lowerLegTwist=.5f,armStretch=.05f,legStretch=.05f};
  UnityEngine.Object.DestroyImmediate(reference);importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.SaveAndReimport();
  var avatar=AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Avatar>().First();if(!avatar.isValid||!avatar.isHuman)throw new Exception("Female avatar invalid");
  var texture=(TextureImporter)AssetImporter.GetAtPath(Folder+"/female_M0.png");texture.maxTextureSize=1024;texture.mipmapEnabled=true;texture.SaveAndReimport();
  const string matPath=Folder+"/female_M0.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(mat==null){mat=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(mat,matPath);}mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/female_M0.png");mat.SetFloat("_Glossiness",.15f);
  var root=new GameObject("CHAR_female_Visual");var facing=new GameObject("Facing");facing.transform.SetParent(root.transform,false);var visual=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath),facing.transform);var animator=visual.GetComponentInChildren<Animator>();animator.avatar=avatar;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.runtimeAnimatorController=Resources.Load<RuntimeAnimatorController>("Runner/Imported/RunnerReview");
  foreach(var renderer in visual.GetComponentsInChildren<SkinnedMeshRenderer>()){renderer.sharedMaterial=mat;renderer.updateWhenOffscreen=true;}
  animator.Rebind();animator.Play("ANIM_idle",0,0);animator.Update(0);
  var foot=animator.GetBoneTransform(HumanBodyBones.LeftFoot);var toe=animator.GetBoneTransform(HumanBodyBones.LeftToes);var forward=Vector3.ProjectOnPlane(toe.position-foot.position,Vector3.up).normalized;
  if(forward.sqrMagnitude<.5f)throw new Exception("Female forward axis is undefined");facing.transform.localRotation=Quaternion.FromToRotation(forward,Vector3.forward);
  PrefabUtility.SaveAsPrefabAsset(root,"Assets/Resources/Runner/Imported/CHAR_female_Visual.prefab");UnityEngine.Object.DestroyImmediate(root);AssetDatabase.SaveAssets();Debug.Log("FEMALE_HUMANOID_PASS");
 }
 static void Align(Transform parent,Transform child,Vector3 direction){parent.rotation=Quaternion.FromToRotation(child.position-parent.position,direction)*parent.rotation;}
}}
