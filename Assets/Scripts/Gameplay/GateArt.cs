using UnityEngine;
namespace StealAMillion
{
    public static class GateArt
    {
        public static string Style(string id){return id=="jackpot"?"jackpot":id=="investment"||id=="business"||id=="market"?"investment":id=="risk"||id=="riskchain"||id=="doubleornothing"||id=="subtractmoney"||id=="tax"?"risk":"safe";}
        public static void Apply(GameObject root,string gateId="safe")
        {
            var prefab=Resources.Load<GameObject>("Runner/Gates/"+Style(gateId))??Resources.Load<GameObject>("Runner/Gates/Frame");if(prefab==null)return;
            foreach(Transform child in root.transform)if(child.name=="Pillar"||child.name=="Foot"||child.name=="Banner"||child.name=="Crown"){
                child.gameObject.SetActive(false);Object.Destroy(child.gameObject);
            }
            var model=Object.Instantiate(prefab,root.transform);model.name="ImportedFrame";
        }
    }
}
