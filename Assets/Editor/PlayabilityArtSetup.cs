using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using StealAMillion.Core;

namespace StealAMillion.Editor
{
    public static class PlayabilityArtSetup
    {
        const string Art="Assets/Art/Playability";
        const string Output="Assets/Resources/Runner/Decor";
        public static readonly string[] Obstacles={"tax","police","thief","barrier","spinner","safe","traffic","wall","sign","closing"};
        [Serializable] class Entry {public string id;public int triangles,renderers;public Vector3 size;}
        [Serializable] class Report {public Entry[] assets;}
        static readonly List<Entry> entries=new List<Entry>();
        static Mesh beveledCube;
        public static void Build(){
            Directory.CreateDirectory(Art);Directory.CreateDirectory(Output+"/Obstacles");Directory.CreateDirectory(Output+"/Worlds");AssetDatabase.Refresh();entries.Clear();
            foreach(string id in Obstacles){var root=new GameObject("OBS_"+id);Obstacle(root.transform,id);Save(root,"Obstacles/"+id);}
            var worlds=JsonUtility.FromJson<WorldCatalog>(Resources.Load<TextAsset>("RunnerData/worlds").text).worlds;
            foreach(var world in worlds){var root=new GameObject("ENV_"+world.id);Building(root.transform,world);Save(root,"Worlds/"+world.id);}
            // Merge only static gate decorations; retain separate crest nodes for QA.
            foreach(string style in GateStylesSetup.Styles){var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Runner/Gates/"+style));root.name="Gate_"+style;
                var crest=root.transform.Find("Crest");Combine(root.transform,"Gate_"+style,t=>crest!=null&&t.IsChildOf(crest));
                PrefabUtility.SaveAsPrefabAsset(root,"Assets/Resources/Runner/Gates/"+style+".prefab");UnityEngine.Object.DestroyImmediate(root);}
            Particles();Directory.CreateDirectory("AssetProduction/Reviews/Playability");File.WriteAllText("AssetProduction/Reviews/Playability/import.json",JsonUtility.ToJson(new Report{assets=entries.ToArray()},true));
            AssetDatabase.SaveAssets();Debug.Log("PLAYABILITY_ART_PASS: "+entries.Count+" models, merged gate decoration and eight particle materials");
        }
        static Material Mat(string hex){string path=Art+"/M"+hex.TrimStart('#')+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Standard")){color=RunnerArt.Color(hex),enableInstancing=true};m.SetFloat("_Glossiness",.2f);AssetDatabase.CreateAsset(m,path);}return m;}
        static GameObject P(Transform root,string name,Vector3 pos,Vector3 size,string color,PrimitiveType shape=PrimitiveType.Cube){var go=RunnerArt.Part(root,name,shape,pos,size,color);go.GetComponent<Renderer>().sharedMaterial=Mat(color);if(shape==PrimitiveType.Cube){if(beveledCube==null){var path=Art+"/BeveledCube.asset";beveledCube=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(beveledCube==null){beveledCube=BeveledPrimitive.Create();AssetDatabase.CreateAsset(beveledCube,path);}}go.GetComponent<MeshFilter>().sharedMesh=beveledCube;}return go;}
        static void Obstacle(Transform p,string id){
            const string red="#FF4D5A",white="#FFFFFF",dark="#253A70",gold="#FFC928",metal="#70899F";
            if(id=="traffic"){
                P(p,"Body",new Vector3(0,.57f,0),new Vector3(1.5f,.56f,2.45f),red);P(p,"Cabin",new Vector3(0,1.01f,.1f),new Vector3(1.23f,.58f,1.35f),dark);
                P(p,"Windshield",new Vector3(0,1.08f,-.59f),new Vector3(1.08f,.36f,.045f),"#D5F3FF");P(p,"Roof",new Vector3(0,1.33f,.1f),new Vector3(1.27f,.07f,1.4f),red);
                foreach(int side in new[]{-1,1}){P(p,"SideWindow",new Vector3(side*.635f,1.08f,.1f),new Vector3(.035f,.34f,.95f),"#D5F3FF");P(p,"Headlight",new Vector3(side*.47f,.63f,-1.24f),new Vector3(.3f,.18f,.04f),"#FFF5B1");
                    foreach(int end in new[]{-1,1}){var wheel=P(p,"Wheel",new Vector3(side*.72f,.28f,end*.79f),new Vector3(.5f,.11f,.5f),dark,PrimitiveType.Cylinder);wheel.transform.localRotation=Quaternion.Euler(0,0,90);}}
                P(p,"Bumper",new Vector3(0,.35f,-1.25f),new Vector3(1.38f,.13f,.10f),white);
            }else if(id=="closing"){
                foreach(int side in new[]{-1,1}){var door=RunnerArt.Group(p,side<0?"DoorL":"DoorR",new Vector3(side*.45f,1.2f,0));P(door,"Panel",Vector3.zero,new Vector3(.8f,2.4f,.3f),red);P(door,"Window",new Vector3(0,.34f,-.17f),new Vector3(.6f,.54f,.025f),dark);P(door,"SafetyBand",new Vector3(0,-.35f,-.17f),new Vector3(.8f,.18f,.03f),white);}
                P(p,"Lintel",new Vector3(0,2.53f,0),new Vector3(2.2f,.18f,.4f),metal);
            }else if(id=="spinner"){
                P(p,"Base",new Vector3(0,.13f,0),new Vector3(.65f,.13f,.65f),dark,PrimitiveType.Cylinder);P(p,"Hub",new Vector3(0,.65f,0),new Vector3(.38f,.55f,.38f),gold,PrimitiveType.Cylinder);
                P(p,"Arm",new Vector3(0,1.1f,0),new Vector3(2.35f,.36f,.35f),red);foreach(int side in new[]{-1,1})P(p,"PaddedTip",new Vector3(side*1.08f,1.1f,0),Vector3.one*.4f,white,PrimitiveType.Sphere);
            }else if(id=="safe"){
                P(p,"Body",new Vector3(0,.7f,0),new Vector3(1.25f,1.3f,1.25f),metal);P(p,"Door",new Vector3(0,.72f,-.645f),new Vector3(1.08f,1.1f,.08f),dark);
                var wheel=P(p,"Lock",new Vector3(0,.75f,-.71f),new Vector3(.5f,.055f,.5f),gold,PrimitiveType.Cylinder);wheel.transform.localRotation=Quaternion.Euler(90,0,0);
                for(int i=0;i<3;i++){var handle=P(p,"Handle",new Vector3(0,.75f,-.78f),new Vector3(.63f,.08f,.07f),white);handle.transform.localRotation=Quaternion.Euler(0,0,i*60);}
            }else if(id=="wall"){
                P(p,"ReceiptWall",new Vector3(0,1.3f,0),new Vector3(1.75f,2.6f,.35f),red);for(int i=0;i<5;i++)P(p,"ReceiptLine",new Vector3(0,.5f+i*.38f,-.19f),new Vector3(i%2==0?1.22f:.85f,.13f,.025f),white);
            }else if(id=="sign"){
                P(p,"Foot",new Vector3(0,.1f,0),new Vector3(.55f,.15f,.55f),metal);P(p,"Post",new Vector3(0,.82f,0),new Vector3(.12f,1.65f,.12f),metal);
                var sign=P(p,"WarningSign",new Vector3(0,1.7f,0),new Vector3(1.1f,1.1f,.15f),gold);sign.transform.localRotation=Quaternion.Euler(0,0,45);
                P(p,"Exclamation",new Vector3(0,1.78f,-.1f),new Vector3(.12f,.46f,.03f),dark);P(p,"Point",new Vector3(0,1.44f,-.1f),new Vector3(.14f,.14f,.03f),dark);
            }else if(id=="thief"){
                RunnerArt.Character(p);foreach(var r in p.GetComponentsInChildren<MeshRenderer>())r.sharedMaterial=Mat(r.name=="Shirt"||r.name=="Sleeve"?dark:"#"+ColorUtility.ToHtmlStringRGB(r.sharedMaterial.color));
                p.Find("Body").localRotation=Quaternion.Euler(0,180,0);P(p,"Mask",new Vector3(0,1.8f,-.29f),new Vector3(.5f,.15f,.055f),dark);P(p,"LootBag",new Vector3(.5f,.85f,0),new Vector3(.4f,.53f,.35f),"#916044");
            }else{
                string c=id=="police"?"#3E78FF":red;
                P(p,"Barrier",new Vector3(0,.86f,0),new Vector3(1.7f,1.1f,.45f),c);
                foreach(int side in new[]{-1,1})P(p,"Foot",new Vector3(side*.59f,.15f,0),new Vector3(.23f,.3f,.7f),metal);
                if(id=="tax"){
                    P(p,"Receipt",new Vector3(0,.9f,-.25f),new Vector3(.62f,.8f,.04f),white);for(int i=0;i<3;i++)P(p,"ReceiptLine",new Vector3(0,.65f+i*.2f,-.28f),new Vector3(.4f,.06f,.03f),c);
                }else for(int i=0;i<4;i++){var stripe=P(p,"Stripe",new Vector3(-.65f+i*.43f,.86f,-.25f),new Vector3(.15f,.9f,.04f),white);stripe.transform.localRotation=Quaternion.Euler(0,0,-25);}
                if(id=="police")foreach(int side in new[]{-1,1}){P(p,"BeaconBase",new Vector3(side*.5f,1.47f,0),new Vector3(.3f,.12f,.3f),dark);P(p,"Beacon",new Vector3(side*.5f,1.62f,0),Vector3.one*.25f,side<0?red:"#38E2D6",PrimitiveType.Sphere);}
            }
        }
        static void Building(Transform p,WorldData w){
            float height=w.id=="streets"?4:w.id=="island"?3.4f:w.id=="bay"?5:w.id=="luxury"?6:w.id=="downtown"?9:12;
            bool coast=w.id=="island"||w.id=="bay";
            P(p,"Building",new Vector3(0,height/2,0),new Vector3(3.6f,height,4.4f),w.building);
            P(p,"Foundation",new Vector3(0,.18f,0),new Vector3(3.95f,.35f,4.75f),"#FFFFFF");
            for(int floor=1;floor<height;floor+=2){
                foreach(int side in new[]{-1,1})for(int window=0;window<3;window++)P(p,"Window",new Vector3(side*1.825f,floor,-1.35f+window*1.35f),new Vector3(.05f,.75f,.85f),"#D5F3FF");
                for(int x=-1;x<=1;x++)P(p,"FrontWindow",new Vector3(x*1.1f,floor,-2.23f),new Vector3(.7f,.75f,.045f),"#D5F3FF");
                if(w.id=="business"||w.id=="downtown"||w.id=="capital")P(p,"FloorBand",new Vector3(0,floor+.6f,0),new Vector3(3.7f,.10f,4.5f),w.accent);
            }
            if(coast){
                P(p,"WideRoof",new Vector3(0,height+.15f,0),new Vector3(4.5f,.24f,5.15f),w.accent);
                foreach(int side in new[]{-1,1})P(p,"TerracePost",new Vector3(side*1.6f,.85f,-2.8f),new Vector3(.16f,1.7f,.16f),"#FFFFFF");
                P(p,"TerraceCanopy",new Vector3(0,1.8f,-2.9f),new Vector3(3.6f,.18f,1.3f),w.accent);
            }else if(w.id=="future"){
                P(p,"TopTower",new Vector3(0,height+1.1f,0),new Vector3(2.7f,2.2f,3.5f),"#FFFFFF");
                foreach(int side in new[]{-1,1})P(p,"LuminousColumn",new Vector3(side*1.86f,height*.5f,-1.8f),new Vector3(.13f,height,.15f),w.accent);
                P(p,"Spire",new Vector3(0,height+3,0),new Vector3(.22f,2.1f,.22f),"#38E2D6");
            }else if(w.id=="gold"||w.id=="capital"){
                P(p,"Crown",new Vector3(0,height+.3f,0),new Vector3(4,.6f,4.8f),w.accent);
                foreach(int side in new[]{-1,1})P(p,"Column",new Vector3(side*1.72f,height*.5f,-2.3f),new Vector3(.25f,height,.32f),"#FFF5B1",PrimitiveType.Cylinder);
                P(p,"RoofDome",new Vector3(0,height+.8f,0),new Vector3(2.5f,1.5f,2.5f),w.accent,PrimitiveType.Sphere);
            }else if(w.id=="luxury"){
                for(int i=0;i<3;i++)P(p,"RoofTerrace",new Vector3(0,height+.18f+i*.3f,0),new Vector3(4-i*.65f,.3f,4.8f-i*.65f),w.accent);
                P(p,"Door",new Vector3(0,.7f,-2.26f),new Vector3(.8f,1.4f,.05f),"#916044");
            }else{
                P(p,"Roof",new Vector3(0,height+.13f,0),new Vector3(4,.26f,4.8f),w.accent);
                P(p,"Storefront",new Vector3(0,.67f,-2.25f),new Vector3(2.8f,1.3f,.05f),"#253A70");
                P(p,"Awning",new Vector3(0,1.55f,-2.55f),new Vector3(3.1f,.18f,.75f),w.accent);
            }
        }
        static void Combine(Transform root,string id,Func<Transform,bool> skip=null){
            var filters=root.GetComponentsInChildren<MeshFilter>().Where(f=>f.GetComponent<MeshRenderer>().enabled&&(skip==null||!skip(f.transform))).ToArray();
            foreach(var group in filters.GroupBy(f=>(f.name=="Banner"||f.name=="Pillar"?"Banner_":"Decor_")+ColorUtility.ToHtmlStringRGBA(f.GetComponent<Renderer>().sharedMaterial.color))){
                var first=group.First();var combines=group.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=root.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray();
                var mesh=new Mesh{indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(combines,true,true);string name=group.Key.StartsWith("Banner")?"Banner":"Decor";
                string path=Art+"/"+id+"_"+group.Key+".asset";
                var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing!=null){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
                var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=first.GetComponent<Renderer>().sharedMaterial;
                foreach(var f in group)UnityEngine.Object.DestroyImmediate(f.gameObject);
            }
        }
        static void Save(GameObject root,string id){
            if(id=="Obstacles/closing")foreach(string side in new[]{"DoorL","DoorR"})Combine(root.transform.Find(side),"OBS_closing_"+side);
            Combine(root.transform,id.Replace('/','_'),t=>id=="Obstacles/closing"&&(t.IsChildOf(root.transform.Find("DoorL"))||t.IsChildOf(root.transform.Find("DoorR"))));
            var renderers=root.GetComponentsInChildren<MeshRenderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            int triangles=root.GetComponentsInChildren<MeshFilter>().Sum(f=>(int)f.sharedMesh.GetIndexCount(0)/3);
            if(root.GetComponentsInChildren<Collider>().Length!=0||triangles>20000||renderers.Length>12)throw new Exception("Art budget/collider check failed: "+id);
            entries.Add(new Entry{id=id,triangles=triangles,renderers=renderers.Length,size=bounds.size});PrefabUtility.SaveAsPrefabAsset(root,Output+"/"+id+".prefab");UnityEngine.Object.DestroyImmediate(root);
        }
        static void Particles(){
            const string folder="Assets/Resources/Runner/Particles";Directory.CreateDirectory(folder);
            foreach(string name in new[]{"soft","spark","note","coin","confetti","ring","dust","streak"})File.Copy("AssetProduction/04_Exports/Particles/PT_"+name+"__v001.png",folder+"/"+name+".png",true);
            AssetDatabase.Refresh();foreach(string name in new[]{"soft","spark","note","coin","confetti","ring","dust","streak"}){
                var importer=(TextureImporter)AssetImporter.GetAtPath(folder+"/"+name+".png");importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=128;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
                var path=folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Particles/Standard Unlit"));AssetDatabase.CreateAsset(m,path);}
                m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/"+name+".png");m.SetFloat("_Mode",2);m.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);m.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);m.SetInt("_ZWrite",0);m.EnableKeyword("_ALPHABLEND_ON");m.renderQueue=3000;EditorUtility.SetDirty(m);
            }
        }
    }
}
