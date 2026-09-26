using StealAMillion.Core;
using UnityEngine;

namespace StealAMillion
{
    public static class PickupArt
    {
        public static string PowerId(PowerKind power)
        {
            switch(power){
                case PowerKind.Shield:return "PWR_shield";
                case PowerKind.Magnet:return "PWR_magnet";
                case PowerKind.Luck:return "PWR_luck";
                case PowerKind.DoubleCash:return "PWR_double_cash";
                case PowerKind.SlowMotion:return "PWR_slow_motion";
                default:return null;
            }
        }
        public static bool Attach(Transform parent,string id)
        {
            if(string.IsNullOrEmpty(id))return false;
            var prefab=Resources.Load<GameObject>("Runner/Pickups/"+id);
            if(prefab==null)return false;
            var art=Object.Instantiate(prefab,parent);art.name=id;return true;
        }
    }
}
