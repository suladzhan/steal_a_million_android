using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace StealAMillion
{
    public static class RunnerArt
    {
        private static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        public static Color Color(string hex) { UnityEngine.Color color; return ColorUtility.TryParseHtmlString(hex, out color) ? color : UnityEngine.Color.white; }
        public static Material Material(string hex)
        {
            Material result;
            if (materials.TryGetValue(hex, out result) && result != null) return result;
            result = Resources.Load<Material>("Runner/Materials/M" + hex.TrimStart('#'));
            if (result == null)
            {
                result = new Material(Shader.Find("Standard")) { color = Color(hex), name = "M" + hex.TrimStart('#'), enableInstancing = true };
                result.SetFloat("_Glossiness", .24f);
            }
            materials[hex] = result; return result;
        }
        public static GameObject Part(Transform parent, string name, PrimitiveType shape, Vector3 pos, Vector3 size, string color)
        {
            var part = GameObject.CreatePrimitive(shape); part.name = name;
            part.transform.SetParent(parent, false); part.transform.localPosition = pos; part.transform.localScale = size;
            part.GetComponent<Renderer>().sharedMaterial = Material(color);
            var collider = part.GetComponent<Collider>(); if (collider != null) { collider.enabled = false; if (Application.isPlaying) Object.Destroy(collider); else Object.DestroyImmediate(collider); }
            return part;
        }
        public static Transform Group(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position; return go.transform;
        }
        public static TextMeshPro Text(Transform parent, string name, string value, Vector3 position, float size, string color, Vector2 bounds)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position;
            var text = go.AddComponent<TextMeshPro>();
            text.font = Resources.Load<TMP_FontAsset>("Runner/Fonts/Primary") ?? TMP_Settings.defaultFontAsset;
            text.text = value; text.fontSize = size; text.color = Color(color); text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold; text.enableAutoSizing = true; text.fontSizeMax = size; text.fontSizeMin = size * .6f;
            text.rectTransform.sizeDelta = bounds; text.overflowMode = TextOverflowModes.Overflow;
            text.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return text;
        }
        public static void Character(Transform root)
        {
            var body = Group(root, "Body", Vector3.zero);
            Part(body,"Shirt",PrimitiveType.Capsule,new Vector3(0,1.05f,0),new Vector3(.64f,.45f,.42f),"#3E78FF");
            Part(body,"Belt",PrimitiveType.Cube,new Vector3(0,.78f,0),new Vector3(.5f,.14f,.36f),"#253A70");
            Part(body,"Head",PrimitiveType.Sphere,new Vector3(0,1.73f,.02f),new Vector3(.58f,.63f,.56f),"#EFB995");
            Part(body,"Hair",PrimitiveType.Sphere,new Vector3(0,1.99f,-.04f),new Vector3(.61f,.23f,.56f),"#41364D");
            Part(body,"Nose",PrimitiveType.Sphere,new Vector3(0,1.72f,.29f),new Vector3(.10f,.10f,.12f),"#E8A782");
            foreach (int side in new[] {-1,1})
            {
                Part(body,"Eye",PrimitiveType.Sphere,new Vector3(side*.12f,1.8f,.268f),new Vector3(.055f,.075f,.04f),"#16202A");
                Part(body,"Ear",PrimitiveType.Sphere,new Vector3(side*.29f,1.75f,0),new Vector3(.12f,.18f,.12f),"#EFB995");
                var arm = Group(body,side<0?"ArmL":"ArmR",new Vector3(side*.4f,1.34f,0));
                Part(arm,"Sleeve",PrimitiveType.Capsule,new Vector3(0,-.13f,0),new Vector3(.23f,.20f,.25f),"#3E78FF");
                Part(arm,"Hand",PrimitiveType.Sphere,new Vector3(0,-.42f,0),new Vector3(.23f,.27f,.23f),"#EFB995");
                var leg = Group(body,side<0?"LegL":"LegR",new Vector3(side*.17f,.77f,0));
                Part(leg,"Pants",PrimitiveType.Capsule,new Vector3(0,-.27f,0),new Vector3(.25f,.28f,.26f),"#253A70");
                Part(leg,"Shoe",PrimitiveType.Cube,new Vector3(0,-.62f,.08f),new Vector3(.30f,.18f,.46f),"#FFFFFF");
                Part(leg,"Sole",PrimitiveType.Cube,new Vector3(0,-.7f,.09f),new Vector3(.31f,.06f,.47f),"#BDD3E8");
            }
            var accessory = Group(body,"Accessory",Vector3.zero);
            var shadow = Part(root,"Shadow",PrimitiveType.Cylinder,new Vector3(0,.015f,0),new Vector3(.95f,.003f,.65f),"#BEC8D3");
            shadow.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        public static void Gate(Transform root, string color)
        {
            foreach(int side in new[]{-1,1})
            {
                Part(root,"Pillar",PrimitiveType.Capsule,new Vector3(side*1.52f,1.5f,0),new Vector3(.28f,1.5f,.32f),color);
                Part(root,"Foot",PrimitiveType.Cube,new Vector3(side*1.52f,.13f,0),new Vector3(.52f,.26f,.64f),"#FFFFFF");
            }
            Part(root,"Banner",PrimitiveType.Cube,new Vector3(0,3.5f,0),new Vector3(3.36f,2.35f,.24f),color);
            Part(root,"Crown",PrimitiveType.Cube,new Vector3(0,4.72f,0),new Vector3(3.5f,.12f,.38f),"#FFFFFF");
            Text(root,"Title","",new Vector3(0,4.3f,-.15f),4.8f,"#FFFFFF",new Vector2(3.1f,.5f));
            Text(root,"Value","",new Vector3(0,3.62f,-.15f),5.8f,"#FFFFFF",new Vector2(3.1f,.7f));
            Text(root,"Odds","",new Vector3(0,2.91f,-.15f),4.8f,"#FFE76A",new Vector2(3.1f,.55f));
            Text(root,"Loss","",new Vector3(0,2.45f,-.15f),2.5f,"#FFFFFF",new Vector2(3.1f,.3f));
            var trigger=root.gameObject.AddComponent<BoxCollider>();trigger.isTrigger=true;trigger.center=new Vector3(0,1.2f,0);trigger.size=new Vector3(3.85f,2.4f,.8f);
        }
        public static void Cash(Transform root)
        {
            for(int i=0;i<3;i++)Part(root,"Bill",PrimitiveType.Cube,new Vector3(0,.1f+i*.08f,0),new Vector3(.84f,.065f,.42f),i==2?"#A9FFBE":"#35D06F");
            Part(root,"Band",PrimitiveType.Cube,new Vector3(0,.245f,0),new Vector3(.18f,.12f,.44f),"#FFFFFF");
            Text(root,"Dollar","$",new Vector3(.25f,.31f,-.22f),2,"#159447",new Vector2(.3f,.25f));
            var trigger=root.gameObject.AddComponent<BoxCollider>();trigger.isTrigger=true;trigger.size=new Vector3(.95f,1.4f,.9f);trigger.center=new Vector3(0,.4f,0);
        }
        public static void Track(Transform root, bool split = false)
        {
            if(split)
                foreach(int side in new[]{-1,1})Part(root,"Bridge",PrimitiveType.Cube,new Vector3(side*2.6f,-.26f,9),new Vector3(3.05f,.5f,18),"#F4F6F8");
            else Part(root,"Road",PrimitiveType.Cube,new Vector3(0,-.26f,9),new Vector3(8.4f,.5f,18),"#F4F6F8");
            foreach(int side in new[]{-1,1})
            {
                Part(root,"Edge",PrimitiveType.Cube,new Vector3(side*4.22f,-.12f,9),new Vector3(.22f,.24f,18),"#D9E1EA");
                for(int z=2;z<18;z+=4)Part(root,"RoadStud",PrimitiveType.Cube,new Vector3(side*4.1f,.025f,z),new Vector3(.12f,.025f,1),"#FFC928");
            }
            var col=root.gameObject.AddComponent<BoxCollider>();col.center=new Vector3(0,-.3f,9);col.size=new Vector3(8.6f,.5f,18);
        }
        public static void Vault(Transform root)
        {
            Part(root,"VaultBody",PrimitiveType.Cube,new Vector3(0,2,0),new Vector3(4.8f,4,2),"#566575");
            Part(root,"VaultFrame",PrimitiveType.Cube,new Vector3(0,2,-1.05f),new Vector3(4.4f,3.6f,.2f),"#AAB7C4");
            var inside=Part(root,"Interior",PrimitiveType.Cylinder,new Vector3(0,2,-1.2f),new Vector3(3.3f,.05f,3.3f),"#16202A");inside.transform.localRotation=Quaternion.Euler(90,0,0);
            for(int i=0;i<3;i++)Part(root,"VaultCash",PrimitiveType.Cube,new Vector3((i-1)*.8f,1.25f,-1.29f),new Vector3(.7f,.45f,.12f),"#35D06F");
            var hinge=Group(root,"DoorHinge",new Vector3(-1.6f,2,-1.4f));
            var door=Part(hinge,"Door",PrimitiveType.Cylinder,new Vector3(1.6f,0,0),new Vector3(3.2f,.15f,3.2f),"#70899F");door.transform.localRotation=Quaternion.Euler(90,0,0);
            var wheel=Part(hinge,"Wheel",PrimitiveType.Cylinder,new Vector3(1.6f,0,-.25f),new Vector3(1,.13f,1),"#FFC928");wheel.transform.localRotation=Quaternion.Euler(90,0,0);
            for(int i=0;i<3;i++){var bar=Part(hinge,"Handle",PrimitiveType.Cube,new Vector3(1.6f,0,-.4f),new Vector3(1.55f,.13f,.12f),"#FFF5B1");bar.transform.localRotation=Quaternion.Euler(0,0,i*60);}
            root.gameObject.AddComponent<VaultVisual>().hinge=hinge;
        }
    }
}
