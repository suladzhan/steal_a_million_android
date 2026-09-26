using System.Linq;
using UnityEngine;

namespace StealAMillion
{
    public static class FinishArt
    {
        public static bool Apply(GameObject root,string id)
        {
            var prefab=Resources.Load<GameObject>("Runner/Finish/"+id);
            if(prefab==null)return false;
            foreach(Transform child in root.transform){child.gameObject.SetActive(false);Object.Destroy(child.gameObject);}
            var model=Object.Instantiate(prefab,root.transform);model.name=id;
            var visual=root.GetComponent<VaultVisual>();
            if(visual!=null)visual.hinge=model.GetComponentsInChildren<Transform>().First(t=>t.name=="DoorHinge");
            return true;
        }
    }
}
