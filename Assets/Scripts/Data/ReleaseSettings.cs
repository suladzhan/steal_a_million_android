using UnityEngine;

namespace StealAMillion
{
    [CreateAssetMenu(menuName="Steal A Million/Release Settings")]
    public sealed class ReleaseSettings : ScriptableObject
    {
        public string publisherName="",supportEmail="",privacyUrl="";
        public static ReleaseSettings Current=>Resources.Load<ReleaseSettings>("Runner/ReleaseSettings");
    }
}
