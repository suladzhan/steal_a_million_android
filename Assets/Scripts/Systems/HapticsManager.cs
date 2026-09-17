using StealAMillion.Core;
using UnityEngine;

namespace StealAMillion
{
    public sealed class HapticsManager
    {
        private readonly SaveData settings;
        public HapticsManager(SaveData data) { settings = data; }

        public void Pulse(bool strong = false)
        {
            if (!settings.vibrationEnabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var window = activity.Call<AndroidJavaObject>("getWindow"))
                using (var view = window.Call<AndroidJavaObject>("getDecorView"))
                    view.Call<bool>("performHapticFeedback", strong ? 0 : 3);
            }
            catch (AndroidJavaException) { /* Haptics are optional on unsupported devices. */ }
#endif
        }
    }
}
