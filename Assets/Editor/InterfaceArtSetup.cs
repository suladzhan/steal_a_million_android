using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StealAMillion.Editor
{
    public static class InterfaceArtSetup
    {
        public static void Build()
        {
            const string ui="Assets/Resources/Runner/UI";
            Directory.CreateDirectory(ui);
            foreach(var file in Directory.GetFiles("AssetProduction/04_Exports/UI","*.png"))File.Copy(file,ui+"/"+Path.GetFileName(file).Replace("__v001",""),true);
            // A neutral beveled face: runtime tint controls semantic colors.
            var face=new Texture2D(128,128,TextureFormat.RGBA32,false);
            for(int y=0;y<128;y++)for(int x=0;x<128;x++){
                float dx=Mathf.Max(18-x-.5f,x+.5f-110),dy=Mathf.Max(18-y-.5f,y+.5f-110);
                float a=Mathf.Clamp01(18-new Vector2(Mathf.Max(0,dx),Mathf.Max(0,dy)).magnitude);
                float shade=Mathf.Lerp(.82f,1,y/127f);face.SetPixel(x,y,new Color(shade,shade,shade,a));
            }
            face.Apply();File.WriteAllBytes(ui+"/ButtonFace-v001.png",face.EncodeToPNG());UnityEngine.Object.DestroyImmediate(face);
            AssetDatabase.Refresh();
            foreach(var file in Directory.GetFiles(ui,"*.png")){
                var importer=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.maxTextureSize=file.Contains("Logo")?2048:256;
                importer.spriteBorder=file.Contains("ButtonFace")?new Vector4(20,20,20,20):file.Contains("UI_panel")?new Vector4(12,12,12,12):Vector4.zero;
                importer.SaveAndReimport();
            }
            const string source="Assets/Art/Gates";const string output="Assets/Resources/Runner/Gates";
            Directory.CreateDirectory(source);Directory.CreateDirectory(output);
            File.Copy("AssetProduction/04_Exports/GATE_frame/GATE_frame__v001__model.fbx",source+"/Frame.fbx",true);AssetDatabase.Refresh();
            var modelImporter=(ModelImporter)AssetImporter.GetAtPath(source+"/Frame.fbx");
            modelImporter.globalScale=1;modelImporter.useFileScale=true;modelImporter.bakeAxisConversion=true;modelImporter.importAnimation=false;
            modelImporter.importCameras=false;modelImporter.importLights=false;modelImporter.addCollider=false;modelImporter.isReadable=true;
            modelImporter.materialImportMode=ModelImporterMaterialImportMode.None;modelImporter.SaveAndReimport();
            var root=new GameObject("GATE_frame");var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(source+"/Frame.fbx"),root.transform);
            foreach(var filter in model.GetComponentsInChildren<MeshFilter>()){
                var mesh=filter.sharedMesh;var uv=mesh.uv;var indices=mesh.triangles;
                var tinted=new System.Collections.Generic.List<int>();var neutral=new System.Collections.Generic.List<int>();
                for(int i=0;i<indices.Length;i+=3){var v=uv[indices[i]];int cell=Mathf.FloorToInt(v.x*8)+Mathf.FloorToInt(v.y*8)*8;
                    var list=cell==1?tinted:neutral;list.Add(indices[i]);list.Add(indices[i+1]);list.Add(indices[i+2]);}
                foreach(bool color in new[]{true,false}){
                    var part=UnityEngine.Object.Instantiate(mesh);part.name=color?"TintedFrame":"WhiteTrim";
                    part.triangles=(color?tinted:neutral).ToArray();if(part.triangles.Length==0){UnityEngine.Object.DestroyImmediate(part);continue;}
                    part.RecalculateBounds();var path=source+"/"+part.name+".asset";
                    if(AssetDatabase.LoadAssetAtPath<Mesh>(path)!=null){EditorUtility.CopySerialized(part,AssetDatabase.LoadAssetAtPath<Mesh>(path));UnityEngine.Object.DestroyImmediate(part);part=AssetDatabase.LoadAssetAtPath<Mesh>(path);}else AssetDatabase.CreateAsset(part,path);
                    var go=new GameObject(color?"Banner":"Trim",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(filter.transform,false);
                    go.GetComponent<MeshFilter>().sharedMesh=part;go.GetComponent<MeshRenderer>().sharedMaterial=RunnerArt.Material(color?"#3E78FF":"#FFFFFF");
                }
                filter.GetComponent<MeshRenderer>().enabled=false;
            }
            var renderers=root.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled).ToArray();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            if(Vector3.Distance(bounds.size,new Vector3(3.5f,4.78f,.64f))>.1f||root.GetComponentsInChildren<Collider>().Length!=0||!renderers.Any(r=>r.name=="Banner"))throw new Exception("Gate art contract failed");
            PrefabUtility.SaveAsPrefabAsset(root,output+"/Frame.prefab");UnityEngine.Object.DestroyImmediate(root);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("AssetProduction/Reviews/Interface");File.WriteAllText("AssetProduction/Reviews/Interface/import.txt","UI sprites imported; gate dimensions "+bounds.size+"; tint and neutral trim separated; no imported colliders.");
            Debug.Log("INTERFACE_ART_PASS");
        }
    }
}
