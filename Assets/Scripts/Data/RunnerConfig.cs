using UnityEngine;

namespace StealAMillion
{
    [CreateAssetMenu(menuName = "Steal A Million/Runner Balance")]
    public sealed class RunnerConfig : ScriptableObject
    {
        public Core.RunnerEconomy economy = new Core.RunnerEconomy();
        public float horizontalSpeed = 13, dragSensitivity = 13;
        public float suspenseSeconds = 1.1f, powerSeconds = 9;
        public int endlessUnlockLevel = 12;
        public bool mockAds = true;
        public int targetFrameRate = 60;
    }
}
