using UnityEngine;

namespace StealAMillion
{
    [CreateAssetMenu(menuName = "Steal A Million/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [Min(1)] public int startingMoney = 100;
        [Min(1000)] public int targetMoney = 1000000;
        [Min(1)] public int secondChanceMoney = 500;
        [Range(.5f, 2f)] public float suspenseSeconds = 1.15f;
        [Range(.5f, 3f)] public float resultSeconds = 1.6f;
        [Range(.1f, 1.5f)] public float countSeconds = .65f;
        public bool mockAdsEnabled = true;
        public bool developmentControls;
    }
}
