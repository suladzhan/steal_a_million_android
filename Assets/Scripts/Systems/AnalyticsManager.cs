using System;

namespace StealAMillion
{
    public interface IAnalytics
    {
        void Track(string eventName, string detail = "");
    }

    public sealed class AnalyticsManager : IAnalytics
    {
        // Local extension point. No identifiers, networking, or analytics SDK.
        public event Action<string, string> EventRecorded;
        public void Track(string eventName, string detail = "")
        {
            if (EventRecorded != null) EventRecorded(eventName, detail);
        }
    }
}
