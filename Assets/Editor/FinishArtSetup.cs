using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StealAMillion.Editor
{
    public static class FinishArtSetup
    {
        public static readonly string[] Ids={"PROP_cash_stack","vault_classic","vault_gold","vault_diamond","vault_neon"};
        [Serializable] class Entry { public string id; public Vector3 size; public int triangles; public Vector3 hinge; }
        [Serializable] class Report { public Entry[] models; }
        public static void Build()
        {
            const string source="Assets/Art/Finish";
            const string output="Assets/Resources/Runner/Finish";
            Directory.CreateDirectory(source);Directory.CreateDirectory(output);
            foreach(var id in Ids)File.Copy("AssetProduction/04_Exports/"+id+"/"+id+"__v001__model.fbx",source+"/"+id+".fbx",true);
            AssetDatabase.Refresh();
            var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Pickups/Palette.mat");
            if(material==null)throw new Exception("Shared pickup palette missing");
            var entries=Ids.Select(id=>{
                var path=source+"/"+id+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;importer.preserveHierarchy=true;
                importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.addCollider=false;
                importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();
                var root=new GameObject(id);var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),root.transform);
                var renderers=root.GetComponentsInChildren<MeshRenderer>();var bounds=renderers[0].bounds;
                foreach(var r in renderers){bounds.Encapsulate(r.bounds);r.sharedMaterial=material;}
                var triangles=root.GetComponentsInChildren<MeshFilter>().Sum(m=>(int)m.sharedMesh.GetIndexCount(0)/3);
                var hinge=root.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="DoorHinge");
                if(root.GetComponentsInChildren<Collider>().Length!=0||triangles>3000)throw new Exception("Geometry contract failed: "+id);
                if(id.StartsWith("vault_")){
                    if(hinge==null||Vector3.Distance(root.transform.InverseTransformPoint(hinge.position),new Vector3(-1.6f,2,-1.4f))>.01f||Vector3.Distance(bounds.size,new Vector3(4.8f,4,2.915f))>.05f)throw new Exception("Vault dimensions/hinge failed: "+id);
                }else if(bounds.size.x>.9f||bounds.size.z>.5f)throw new Exception("Cash dimensions failed");
                var entry=new Entry{id=id,size=bounds.size,triangles=triangles,hinge=hinge==null?Vector3.zero:root.transform.InverseTransformPoint(hinge.position)};
                PrefabUtility.SaveAsPrefabAsset(root,output+"/"+id+".prefab");UnityEngine.Object.DestroyImmediate(root);return entry;
            }).ToArray();
            Directory.CreateDirectory("AssetProduction/Reviews/Finish");File.WriteAllText("AssetProduction/Reviews/Finish/Unity__v001__report.json",JsonUtility.ToJson(new Report{models=entries},true));
            AssetDatabase.SaveAssets();AssetDatabase.ExportPackage(new[]{source,output,"Assets/Art/Pickups/Palette.mat","Assets/Art/Pickups/Palette.png"},"AssetProduction/04_Exports/SAM_Finish__v001.unitypackage",ExportPackageOptions.Recurse);
            Debug.Log("FINISH_ART_PASS: "+entries.Length);
        }
    }
}
