using System;
using System.Collections;
using UnityEngine;

namespace StealAMillion
{
    public interface IRewardedAds
    {
        bool IsRewardedAdReady();
        IEnumerator ShowRewardedAd(Action<bool> completed);
    }

    public sealed class MockRewardedAds : IRewardedAds
    {
        private bool busy;
        private readonly bool enabled;
        public MockRewardedAds(bool isEnabled) { enabled = isEnabled; }
        public bool IsRewardedAdReady() { return enabled && !busy; }
        public IEnumerator ShowRewardedAd(Action<bool> completed)
        {
            if (!IsRewardedAdReady()) { completed(false); yield break; }
            busy = true;
            yield return new WaitForSeconds(1.5f);
            busy = false;
            completed(true);
        }
    }
}
