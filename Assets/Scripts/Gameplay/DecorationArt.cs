using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace StealAMillion
{
    public static class DecorationArt
    {
        static readonly Dictionary<string,GameObject> prefabs=new Dictionary<string,GameObject>();
        static GameObject Get(string path){if(!prefabs.TryGetValue(path,out var prefab)||prefab==null){prefab=Resources.Load<GameObject>("Runner/Major/"+path)??Resources.Load<GameObject>("Runner/Decor/"+path);if(prefab!=null)prefabs[path]=prefab;}return prefab;}
        public static bool Tree(Transform parent,Vector3 position,bool palm){var prefab=Get("Trees/"+(palm?"palm":"tree"));if(prefab==null)return false;var visual=Object.Instantiate(prefab,parent);visual.transform.localPosition=position;visual.name=palm?"Palm":"Tree";foreach(var r in visual.GetComponentsInChildren<Renderer>()){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}visual.AddComponent<DecorationVisibility>();return true;}
        public static bool Obstacle(GameObject root,string id){
            var prefab=Get("Obstacles/"+id);if(prefab==null)return false;
            foreach(Transform child in root.transform){child.gameObject.SetActive(false);Object.Destroy(child.gameObject);}
            var visual=Object.Instantiate(prefab,root.transform);visual.name="ObstacleVisual";return true;
        }
        public static bool Building(Transform root,string world){
            var prefab=Get("Worlds/"+world);if(prefab==null)return false;
            var visual=Object.Instantiate(prefab,root);visual.name="WorldDecor";
            foreach(var r in visual.GetComponentsInChildren<Renderer>()){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
            visual.AddComponent<DecorationVisibility>();return true;
        }
    }
    public sealed class DecorationVisibility : MonoBehaviour
    {
        Renderer[] renderers;Transform player;float timer;bool hidden;
        public bool Hidden {get{return hidden;}}
        void Start(){renderers=GetComponentsInChildren<Renderer>();var game=GameManager.Instance;player=game==null||game.World==null?null:game.World.player.transform;}
        void Update(){timer-=Time.deltaTime;if(timer>0||player==null||renderers==null)return;timer=.25f;
            float distance=transform.position.z-player.position.z;bool value=distance< -45||distance>135;
            if(value==hidden)return;hidden=value;foreach(var r in renderers)if(r!=null)r.forceRenderingOff=hidden;
        }
    }
}
