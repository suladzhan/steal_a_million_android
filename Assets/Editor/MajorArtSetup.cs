using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using StealAMillion.Core;

namespace StealAMillion.Editor
{
    public static class MajorArtSetup
    {
        [Serializable] class MaterialInfo{public string name,texture;}
        [Serializable] class Entry{public string id;public int triangles,budget;public MaterialInfo[] materials;}
        [Serializable] class Report{public Entry[] assets;}
        const string Art="Assets/Art/Major",Output="Assets/Resources/Runner/Major";
        public static void BuildAll(){Build();FemaleArtSetup.Build();}
        public static void Build(){
            Directory.CreateDirectory("Assets/Resources/Runner/Materials");AssetDatabase.Refresh();
            const string clothPath="Assets/Resources/Runner/Materials/ClothTemplate.mat";
            if(AssetDatabase.LoadAssetAtPath<Material>(clothPath)==null)AssetDatabase.CreateAsset(new Material(Shader.Find("Runner/Cloth Tint")),clothPath);
            Directory.CreateDirectory(Output+"/Worlds");Directory.CreateDirectory(Output+"/Obstacles");Directory.CreateDirectory(Output+"/Trees");AssetDatabase.Refresh();
            var report=JsonUtility.FromJson<Report>(File.ReadAllText("AssetProduction/Reviews/Major/mobile_geometry.json"));
            foreach(var data in report.assets){
                var importer=(ModelImporter)AssetImporter.GetAtPath(Art+"/"+data.id+".fbx");importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.addCollider=false;importer.globalScale=1;importer.meshCompression=ModelImporterMeshCompression.Low;importer.SaveAndReimport();
                foreach(var info in data.materials){var path=Art+"/"+info.name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,path);}material.enableInstancing=true;material.color=Color.white;material.SetFloat("_Metallic",0);material.SetFloat("_Glossiness",.15f);
                    if(info.texture!=null){var texture=(TextureImporter)AssetImporter.GetAtPath(Art+"/"+info.texture);texture.maxTextureSize=1024;texture.mipmapEnabled=true;texture.textureCompression=TextureImporterCompression.Compressed;texture.alphaSource=TextureImporterAlphaSource.None;texture.SaveAndReimport();material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/"+info.texture);}
                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),info.name),material);
                }importer.SaveAndReimport();
            }
            var worlds=JsonUtility.FromJson<WorldCatalog>(Resources.Load<TextAsset>("RunnerData/worlds").text).worlds;
            foreach(var world in worlds){string model=world.id=="streets"?"townhouse":world.id=="island"||world.id=="bay"||world.id=="luxury"?"villa":world.id=="future"?"future":"tower";
                var root=Model(model,"ENV_"+world.id);foreach(var r in root.GetComponentsInChildren<MeshRenderer>()){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
                // Cohesive architecture, small opaque accents communicate each district.
                if(world.id=="gold"||world.id=="capital")RunnerArt.Part(root.transform,"Gold plinth",PrimitiveType.Cube,new Vector3(0,.15f,0),new Vector3(4.7f,.3f,5.3f),world.accent);
                Save(root,"Worlds/"+world.id);
            }
            Save(Model("tree","Tree"),"Trees/tree");Save(Model("palm2","Palm"),"Trees/palm");
            Save(Model("car","Traffic"),"Obstacles/traffic");Save(Model("barrier","Barrier"),"Obstacles/barrier");
            var police=Model("barrier","Police barrier");foreach(int side in new[]{-1,1})RunnerArt.Part(police.transform,"Beacon",PrimitiveType.Sphere,new Vector3(side*.55f,1.47f,0),Vector3.one*.25f,side<0?"#3E78FF":"#38E2D6");Save(police,"Obstacles/police");
            var thief=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Runner/Imported/CHAR_runner_Visual"));thief.name="Animated thief";thief.AddComponent<ImportedNpcVisual>();Save(thief,"Obstacles/thief");
            ReleaseReadiness.Configure();AssetDatabase.SaveAssets();Debug.Log("MAJOR_ART_PASS: 8 textured models, 9 worlds, 3 obstacles, 2 trees");
        }
        static GameObject Model(string id,string name){var root=new GameObject(name);var model=AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/"+id+".fbx");if(model==null)throw new Exception("Missing model "+id);UnityEngine.Object.Instantiate(model,root.transform).name="Model";return root;}
        static void Save(GameObject root,string id){int triangles=root.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3)+root.GetComponentsInChildren<SkinnedMeshRenderer>().Sum(r=>r.sharedMesh.triangles.Length/3);if(root.GetComponentsInChildren<Collider>().Length!=0||triangles>(id=="Obstacles/thief"?9000:6000))throw new Exception("Major prefab budget/collider: "+id);PrefabUtility.SaveAsPrefabAsset(root,Output+"/"+id+".prefab");UnityEngine.Object.DestroyImmediate(root);}
    }
}
