using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StealAMillion.Editor
{
    public static class PickupArtSetup
    {
        public static readonly string[] Ids={"PROP_key","PROP_gold_bar","PROP_coin","PWR_shield","PWR_magnet","PWR_luck","PWR_double_cash","PWR_slow_motion"};
        const string Source="Assets/Art/Pickups";
        const string Output="Assets/Resources/Runner/Pickups";
        [Serializable] class Entry { public string id; public Vector3 size; public int triangles; public bool pass; }
        [Serializable] class Report { public string unity; public Entry[] models; }
        public static void Build()
        {
            Directory.CreateDirectory(Source);Directory.CreateDirectory(Output);
            foreach(string id in Ids)File.Copy("AssetProduction/04_Exports/"+id+"/"+id+"__v001__model.fbx",Source+"/"+id+".fbx",true);
            File.Copy("AssetProduction/04_Exports/Shared/SAM_palette__v001.png",Source+"/Palette.png",true);
            AssetDatabase.Refresh();
            var texture=(TextureImporter)AssetImporter.GetAtPath(Source+"/Palette.png");
            texture.sRGBTexture=true;texture.mipmapEnabled=false;texture.filterMode=FilterMode.Point;texture.textureCompression=TextureImporterCompression.Uncompressed;texture.SaveAndReimport();
            var material=AssetDatabase.LoadAssetAtPath<Material>(Source+"/Palette.mat");
            if(material==null){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,Source+"/Palette.mat");}
            material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"/Palette.png");material.SetFloat("_Glossiness",.22f);material.enableInstancing=true;EditorUtility.SetDirty(material);
            var entries=Ids.Select(id=>{
                string path=Source+"/"+id+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;
                importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.addCollider=false;importer.isReadable=false;
                importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();
                var root=new GameObject(id);var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),root.transform);
                var renderers=model.GetComponentsInChildren<MeshRenderer>();var bounds=renderers[0].bounds;
                foreach(var r in renderers){bounds.Encapsulate(r.bounds);r.sharedMaterial=material;}
                model.transform.localPosition=-bounds.center;
                int triangles=model.GetComponentsInChildren<MeshFilter>().Sum(m=>(int)m.sharedMesh.GetIndexCount(0)/3);
                var entry=new Entry{id=id,size=bounds.size,triangles=triangles,pass=renderers.Length==1&&triangles<=2500&&bounds.size.magnitude>.2f&&Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z)<.91f&&root.GetComponentsInChildren<Collider>().Length==0};
                PrefabUtility.SaveAsPrefabAsset(root,Output+"/"+id+".prefab");UnityEngine.Object.DestroyImmediate(root);return entry;
            }).ToArray();
            Directory.CreateDirectory("AssetProduction/Reviews/Pickups");
            File.WriteAllText("AssetProduction/Reviews/Pickups/Unity__v001__report.json",JsonUtility.ToJson(new Report{unity=Application.unityVersion,models=entries},true));
            AssetDatabase.SaveAssets();if(entries.Any(e=>!e.pass))throw new Exception("Pickup art contract failed; see report.");
            AssetDatabase.ExportPackage(new[]{Source,Output},"AssetProduction/04_Exports/SAM_Pickups__v001.unitypackage",ExportPackageOptions.Recurse);
            Debug.Log("PICKUP_ART_PASS: "+entries.Length+" models");
        }
    }
}
