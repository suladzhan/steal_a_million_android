using StealAMillion.Core;
using UnityEngine;
using System.Collections.Generic;

namespace StealAMillion
{
    // Owns only the art. RunnerController remains the sole gameplay movement owner.
    public sealed class ImportedRunnerVisual : MonoBehaviour
    {
        private Animator animator;
        private string current;
        private Transform bag,watch,chain;
        private Transform accessory;private HumanBodyBones accessoryAnchor;private Vector3 accessoryOffset;
        private Transform facing;
        private static readonly Dictionary<string,Material> tinted=new Dictionary<string,Material>();
        public bool IsFemale;
        public static bool SupportsVictory(string style) => style=="jump"||style=="dance"||style=="money_rain"||style=="backflip"||style=="gold_pose"||style=="tornado";
        public void Tint(string color,bool original=false){foreach(var renderer in GetComponentsInChildren<SkinnedMeshRenderer>()){
            var source=renderer.sharedMaterial;string key=source.mainTexture.GetInstanceID()+":"+color+":"+original;
            if(!tinted.TryGetValue(key,out var material)||material==null){material=new Material(Resources.Load<Material>("Runner/Materials/ClothTemplate")){mainTexture=source.mainTexture};material.SetColor("_ClothTint",RunnerArt.Color(color));material.SetFloat("_TintStrength",original?0:1);tinted[key]=material;}renderer.sharedMaterial=material;}}
        public void Accessory(string shape,string color){if(accessory!=null){accessory.gameObject.SetActive(false);Destroy(accessory.gameObject);accessory=null;}
            if(shape=="hood"){accessory=RunnerArt.Part(transform,"Hood",PrimitiveType.Sphere,Vector3.zero,new Vector3(.42f,.38f,.20f),color).transform;accessoryAnchor=HumanBodyBones.Head;accessoryOffset=new Vector3(0,.04f,-.10f);}
            if(shape=="chain"&&!chain.gameObject.activeSelf){accessory=RunnerArt.Part(transform,"Cosmetic chain",PrimitiveType.Sphere,Vector3.zero,new Vector3(.26f,.08f,.06f),"#FFC928").transform;accessoryAnchor=HumanBodyBones.Chest;accessoryOffset=new Vector3(0,.08f,.16f);}
            if(shape=="tie"){accessory=RunnerArt.Part(transform,"Tie",PrimitiveType.Cube,Vector3.zero,new Vector3(.07f,.25f,.035f),"#FFC928").transform;accessoryAnchor=HumanBodyBones.Chest;accessoryOffset=new Vector3(0,0,.18f);}
            if(shape=="visor"||shape=="headband"){accessory=RunnerArt.Part(transform,"Headband",PrimitiveType.Cube,Vector3.zero,new Vector3(.4f,.07f,.035f),color).transform;accessoryAnchor=HumanBodyBones.Head;accessoryOffset=new Vector3(0,.04f,.14f);}
            if(shape=="mask"){accessory=RunnerArt.Part(transform,"Mask",PrimitiveType.Cube,Vector3.zero,new Vector3(.34f,.08f,.045f),color).transform;accessoryAnchor=HumanBodyBones.Head;accessoryOffset=new Vector3(0,.10f,.16f);}
            if(shape=="crown"){accessory=RunnerArt.Group(transform,"Crown",Vector3.zero);accessoryAnchor=HumanBodyBones.Head;accessoryOffset=new Vector3(0,.19f,0);RunnerArt.Part(accessory,"Ring",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.45f,.05f,.45f),"#FFC928");for(int i=0;i<5;i++)RunnerArt.Part(accessory,"Point",PrimitiveType.Cube,new Vector3((i-2)*.08f,.1f,.15f),new Vector3(.05f,.15f,.06f),"#FFC928");}}
        private void Awake()
        {
            animator=GetComponentInChildren<Animator>();
            facing=transform.Find("Facing");
            animator.applyRootMotion=false;
            animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            bag=RunnerArt.Part(transform,"CashBag",PrimitiveType.Sphere,Vector3.zero,new Vector3(.38f,.42f,.20f),"#35D06F").transform;
            watch=RunnerArt.Part(transform,"Watch",PrimitiveType.Cube,Vector3.zero,new Vector3(.12f,.07f,.09f),"#FFC928").transform;
            chain=RunnerArt.Part(transform,"Chain",PrimitiveType.Sphere,Vector3.zero,new Vector3(.26f,.08f,.06f),"#FFC928").transform;
        }
        public void Apply(RunnerSave save)
        {
            bag.gameObject.SetActive(save.highest>=new BigMoney(1000));
            watch.gameObject.SetActive(save.highest>=new BigMoney(100000));
            chain.gameObject.SetActive(save.highest>=new BigMoney(1000000));
        }
        public void ResetPose(){current=null;transform.localPosition=Vector3.zero;transform.localRotation=Quaternion.identity;}
        public void Tick(bool running,bool preview,float lean,float celebration,float stumble,string victory)
        {
            string visualVictory=victory=="backflip"?"jump":victory=="gold_pose"?"idle":victory=="tornado"?"dance":victory;
            string state=celebration>0 ? (running?"ANIM_risk_win":visualVictory=="idle"?"ANIM_idle":"ANIM_victory_"+visualVictory) : running?"ANIM_run":"ANIM_idle";
            if(current!=state)
            {
                if(current==null)animator.Play(state,0,0);else animator.CrossFadeInFixedTime(state,.12f,0,0);
                current=state;
            }
            float flip=celebration>0&&victory=="backflip"?Mathf.Clamp01((2-celebration)/1.25f):0;
            var rotation=Quaternion.Euler(stumble>0?Mathf.Sin(stumble*15)*20:flip*360,(preview?180:0)+(celebration>0&&victory=="tornado"?(2-celebration)*720:0),-lean*1.1f);
            transform.localRotation=rotation;transform.localPosition=flip>0?Vector3.up*(.9f+Mathf.Sin(flip*Mathf.PI)*.65f)-rotation*(Vector3.up*.9f):Vector3.zero;
        }
        private void LateUpdate()
        {
            // Keep the female rig facing the track across Humanoid clips with different source axes.
            if(IsFemale&&facing!=null&&(current=="ANIM_run"||current=="ANIM_idle")){
                var right=animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position-animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position;
                right=transform.InverseTransformDirection(right);var forward=Vector3.Cross(Vector3.ProjectOnPlane(right,Vector3.up),Vector3.up).normalized;
                if(forward.sqrMagnitude>.5f)facing.localRotation=Quaternion.AngleAxis(Vector3.SignedAngle(forward,Vector3.forward,Vector3.up),Vector3.up)*facing.localRotation;
            }
            Attach(bag,HumanBodyBones.Chest,new Vector3(0,0,-.20f));
            Attach(chain,HumanBodyBones.Chest,new Vector3(0,.08f,.16f));
            Attach(watch,HumanBodyBones.LeftHand,new Vector3(0,.04f,0));
            if(accessory!=null)Attach(accessory,accessoryAnchor,accessoryOffset);
        }
        private void Attach(Transform part,HumanBodyBones anchor,Vector3 offset)
        {
            var bone=animator.GetBoneTransform(anchor);
            // Offsets are metres in visual-root space, independent of FBX bone scale.
            part.position=bone.position+transform.rotation*offset;
            part.rotation=anchor==HumanBodyBones.LeftHand?bone.rotation:transform.rotation;
        }
    }
}
