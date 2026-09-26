using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Android;
using UnityEngine;

namespace StealAMillion.Editor
{
    public static class GateStylesSetup
    {
        public static readonly string[] Styles={"safe","risk","jackpot","investment"};
        public static void Build()
        {
            AssetDatabase.Refresh();
            foreach(var style in Styles){
                string path="Assets/Art/GateCrests/"+style+".fbx";
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;
                importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.addCollider=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Runner/Gates/Frame"));root.name="GATE_"+style;
                string color=style=="safe"?"#34D17B":style=="risk"?"#FF4D5A":style=="jackpot"?"#FFC928":"#3E78FF";
                var crest=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),root.transform);crest.name="Crest";crest.transform.localPosition=new Vector3(0,5.27f,-.02f);
                foreach(var r in crest.GetComponentsInChildren<MeshRenderer>())r.sharedMaterial=Mat(style=="jackpot"?"#FFC928":"#FFFFFF");
                int triangles=crest.GetComponentsInChildren<MeshFilter>().Sum(f=>(int)f.sharedMesh.GetIndexCount(0)/3);
                var bounds=crest.GetComponentInChildren<Renderer>().bounds;
                if(triangles>1800||bounds.size.y>1.1f||bounds.size.x>1.2f||bounds.size.z>.3f)throw new Exception("Crest contract failed: "+style+" "+bounds.size);
                foreach(int side in new[]{-1,1}){
                    float x=side*1.53f;
                    Part(root.transform,"Pillar",PrimitiveType.Cube,new Vector3(x,1.48f,.08f),new Vector3(.36f,2.9f,.4f),color);
                    Part(root.transform,"ColumnBase",PrimitiveType.Cube,new Vector3(x,.16f,.03f),new Vector3(.62f,.32f,.7f),"#FFFFFF");
                    if(style=="safe"){
                        for(int i=0;i<3;i++)Part(root.transform,"SafeBands",PrimitiveType.Cube,new Vector3(x,.7f+i*.65f,-.14f),new Vector3(.42f,.11f,.09f),"#FFF5B1");
                        Part(root.transform,"RoundedCap",PrimitiveType.Sphere,new Vector3(x,4.78f,0),new Vector3(.48f,.48f,.48f),color);
                    }else if(style=="risk"){
                        for(int i=0;i<4;i++){var p=Part(root.transform,"RiskStripes",PrimitiveType.Cube,new Vector3(x,.6f+i*.52f,-.16f),new Vector3(.43f,.12f,.07f),"#FFF5B1");p.transform.localRotation=Quaternion.Euler(0,0,side*25);}
                        var fin=Part(root.transform,"AngularCap",PrimitiveType.Cube,new Vector3(x,4.86f,0),new Vector3(.46f,.46f,.38f),color);fin.transform.localRotation=Quaternion.Euler(0,0,45);
                    }else if(style=="jackpot"){
                        for(int i=0;i<3;i++)Part(root.transform,"GoldColumn",PrimitiveType.Cylinder,new Vector3(x,.7f+i*.76f,0),new Vector3(.48f,.16f,.48f),"#FFF5B1");
                        Part(root.transform,"GoldOrb",PrimitiveType.Sphere,new Vector3(x,4.9f,0),Vector3.one*.42f,"#FFC928");
                    }else{
                        for(int i=0;i<3;i++)Part(root.transform,"GrowthSteps",PrimitiveType.Cube,new Vector3(x,4.77f+i*.11f,0),new Vector3(.5f-i*.12f,.12f,.42f),"#FFFFFF");
                        Part(root.transform,"BlueLight",PrimitiveType.Cube,new Vector3(x,1.5f,-.17f),new Vector3(.10f,2.5f,.06f),"#38E2D6");
                    }
                }
                foreach(var r in root.GetComponentsInChildren<MeshRenderer>())if(r.name=="Banner"||r.name=="Pillar")r.sharedMaterial=Mat(color);
                if(root.GetComponentsInChildren<Collider>().Length!=0)throw new Exception("Decoration collider detected");
                PrefabUtility.SaveAsPrefabAsset(root,"Assets/Resources/Runner/Gates/"+style+".prefab");UnityEngine.Object.DestroyImmediate(root);
                Debug.Log("GATE_STYLE_PASS: "+style+" crest triangles="+triangles);
            }
            ConfigureIcons();AssetDatabase.SaveAssets();
        }
        static Material Mat(string color){var path="Assets/Art/GateCrests/M"+color.Substring(1)+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Standard")){color=RunnerArt.Color(color),enableInstancing=true};m.SetFloat("_Glossiness",.24f);AssetDatabase.CreateAsset(m,path);}return m;}
        static GameObject Part(Transform p,string n,PrimitiveType t,Vector3 pos,Vector3 size,string c){var go=RunnerArt.Part(p,n,t,pos,size,c);go.GetComponent<Renderer>().sharedMaterial=Mat(c);return go;}
        static Texture2D LoadIcon(string path){var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.isReadable=true;importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);}
        static void ConfigureIcons(){
            var source=LoadIcon("Assets/Art/Brand/AppIcon-v001.png");
            var bg=new Texture2D(512,512,TextureFormat.RGBA32,false);var fg=new Texture2D(512,512,TextureFormat.RGBA32,false);var combined=new Texture2D(512,512,TextureFormat.RGBA32,false);
            for(int y=0;y<512;y++)for(int x=0;x<512;x++){
                var back=RunnerArt.Color("#06335D");bg.SetPixel(x,y,back);
                float u=(x/511f-.16f)/.68f,v=(y/511f-.16f)/.68f;
                var front=u>=0&&u<=1&&v>=0&&v<=1?source.GetPixelBilinear(u,v):Color.clear;
                fg.SetPixel(x,y,front);combined.SetPixel(x,y,Color.Lerp(back,front,front.a));
            }
            foreach(var pair in new[]{("IconBackground",bg),("IconForeground",fg),("IconLegacy",combined)}){pair.Item2.Apply();File.WriteAllBytes("Assets/Art/Brand/"+pair.Item1+".png",pair.Item2.EncodeToPNG());UnityEngine.Object.DestroyImmediate(pair.Item2);}
            AssetDatabase.Refresh();var background=LoadIcon("Assets/Art/Brand/IconBackground.png");var foreground=LoadIcon("Assets/Art/Brand/IconForeground.png");var legacy=LoadIcon("Assets/Art/Brand/IconLegacy.png");
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown,new[]{legacy},IconKind.Any);
            foreach(var kind in new[]{AndroidPlatformIconKind.Adaptive,AndroidPlatformIconKind.Round,AndroidPlatformIconKind.Legacy}){
                var icons=PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android,kind);
                foreach(var icon in icons){var layers=new Texture2D[icon.maxLayerCount];for(int i=0;i<layers.Length;i++)layers[i]=layers.Length>1?(i==0?background:foreground):legacy;icon.SetTextures(layers);}
                PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android,kind,icons);
            }
            Debug.Log("ANDROID_LOGO_ICON_PASS");
        }
    }
}
