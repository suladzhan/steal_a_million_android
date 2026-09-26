using StealAMillion.Core;
using UnityEngine;

namespace StealAMillion
{
    public sealed class CharacterVisual : MonoBehaviour
    {
        private Transform body,armL,armR,legL,legR;
        private float phase,celebrate,stumble;
        public bool Running,Preview;
        public float Lean;
        private string victory="jump";
        private ImportedRunnerVisual imported;
        public bool UsesImported => imported!=null&&imported.gameObject.activeSelf;
        private void Awake(){Bind();}
        private void Bind()
        {
            body=transform.Find("Body");if(body==null)return;
            armL=body.Find("ArmL");armR=body.Find("ArmR");legL=body.Find("LegL");legR=body.Find("LegR");
        }
        public void Cheer(float seconds=1.1f){celebrate=seconds;}
        public void ResetPose(){celebrate=stumble=phase=Lean=0;if(imported!=null)imported.ResetPose();}
        public void PreviewVictory(string style){victory=style;Cheer(2);}
        public void PreviewCloth(string color){if(imported!=null&&UsesImported)imported.Tint(color);else foreach(var r in body.GetComponentsInChildren<Renderer>())if(r.name=="Shirt"||r.name=="Sleeve")r.sharedMaterial=RunnerArt.Material(color);}
        public void UseProcedural(){if(imported!=null)imported.gameObject.SetActive(false);if(body!=null)body.gameObject.SetActive(true);}
        public void Stumble(){stumble=.6f;}
        public void Apply(RunnerCosmetic skin,RunnerSave save)
        {
            if(body==null)Bind();if(body==null||skin==null)return;
            victory=save.victory;
            bool female=skin.id=="businesswoman";bool useImported=Resources.Load<GameObject>(female?"Runner/Imported/CHAR_female_Visual":"Runner/Imported/CHAR_runner_Visual")!=null;
            if(imported!=null&&imported.IsFemale!=female){var oldImported=imported.gameObject;oldImported.SetActive(false);Destroy(oldImported);imported=null;}
            if(useImported&&imported==null)
            {
                var prefab=Resources.Load<GameObject>(female?"Runner/Imported/CHAR_female_Visual":"Runner/Imported/CHAR_runner_Visual");
                if(prefab!=null){imported=Instantiate(prefab,transform).AddComponent<ImportedRunnerVisual>();imported.IsFemale=female;}
            }
            if(imported!=null){imported.gameObject.SetActive(useImported);if(useImported){imported.Apply(save);var outfit=GameManager.Instance.Cosmetics.Find(c=>c.id==save.outfit);imported.Tint(save.outfit=="outfit_classic"?skin.color:outfit==null?skin.color:outfit.color,skin.id=="runner"&&save.outfit=="outfit_classic");imported.Accessory(skin.shape,skin.secondary);}}
            body.gameObject.SetActive(!UsesImported);
            if(UsesImported)return;
            foreach(var render in body.GetComponentsInChildren<Renderer>())
            {
                if(render.name=="Shirt"||render.name=="Sleeve")render.sharedMaterial=RunnerArt.Material(skin.color);
                if(render.name=="Pants"||render.name=="Belt")render.sharedMaterial=RunnerArt.Material(skin.secondary);
            }
            var old=body.Find("Accessory");if(old!=null){old.gameObject.SetActive(false);Destroy(old.gameObject);}
            var a=RunnerArt.Group(body,"Accessory",Vector3.zero);
            if(skin.shape=="tie")RunnerArt.Part(a,"Tie",PrimitiveType.Cube,new Vector3(0,1.18f,.235f),new Vector3(.10f,.36f,.04f),"#FFC928");
            if(skin.shape=="hood")RunnerArt.Part(a,"Hood",PrimitiveType.Sphere,new Vector3(0,1.76f,-.15f),new Vector3(.67f,.75f,.48f),skin.color);
            if(skin.shape=="hair")RunnerArt.Part(a,"Ponytail",PrimitiveType.Capsule,new Vector3(0,1.7f,-.35f),new Vector3(.28f,.34f,.24f),"#41364D");
            if(skin.shape=="visor"||skin.shape=="headband")RunnerArt.Part(a,"Band",PrimitiveType.Cube,new Vector3(0,1.88f,.27f),new Vector3(.55f,.13f,.12f),skin.secondary);
            if(skin.shape=="crown")
            {
                RunnerArt.Part(a,"Crown",PrimitiveType.Cylinder,new Vector3(0,2.1f,0),new Vector3(.62f,.1f,.62f),"#FFC928");
                for(int i=0;i<5;i++)RunnerArt.Part(a,"Point",PrimitiveType.Cube,new Vector3((i-2)*.115f,2.26f,.2f),new Vector3(.07f,.2f,.1f),"#FFC928");
            }
            if(save.highest>=new BigMoney(1000))RunnerArt.Part(a,"CashBag",PrimitiveType.Sphere,new Vector3(0,1.02f,-.32f),new Vector3(.45f,.5f,.22f),"#35D06F");
            if(save.highest>=new BigMoney(100000))RunnerArt.Part(a,"Watch",PrimitiveType.Cube,new Vector3(.4f,.99f,.12f),new Vector3(.23f,.09f,.1f),"#FFC928");
            if(save.highest>=new BigMoney(1000000)||skin.shape=="chain")RunnerArt.Part(a,"Chain",PrimitiveType.Sphere,new Vector3(0,1.36f,.23f),new Vector3(.35f,.12f,.10f),"#FFC928");
        }
        private void Update()
        {
            if(body==null)return;
            float dt=Time.deltaTime;phase+=dt*(Running?13:2.5f);celebrate=Mathf.Max(0,celebrate-dt);stumble=Mathf.Max(0,stumble-dt);
            if(UsesImported){imported.Tick(Running,Preview,Lean,celebrate,stumble,victory);return;}
            float swing=Running?Mathf.Sin(phase)*38:Mathf.Sin(phase)*4;
            bool cheering=celebrate>0;
            armL.localRotation=Quaternion.Euler(cheering?(victory=="gold_pose"?-65:-155):-swing,0,cheering?-25:8);
            armR.localRotation=Quaternion.Euler(cheering?(victory=="gold_pose"?-65:-155):swing,0,cheering?25:-8);
            legL.localRotation=Quaternion.Euler(swing,0,0);legR.localRotation=Quaternion.Euler(-swing,0,0);
            float jump=cheering?(victory=="gold_pose"?0:Mathf.Abs(Mathf.Sin(celebrate*7))*.45f):Running?Mathf.Abs(Mathf.Sin(phase))*.07f:0;
            body.localPosition=new Vector3(0,jump,0);
            body.localRotation=Quaternion.Euler(stumble>0?Mathf.Sin(stumble*15)*20:cheering&&victory=="backflip"?celebrate*360:0,
                Preview?180+Mathf.Sin(phase*.3f)*12:cheering&&victory=="dance"?Mathf.Sin(phase)*25:0,-Lean*1.1f);
        }
    }
}
