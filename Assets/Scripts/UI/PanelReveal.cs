using UnityEngine;

namespace StealAMillion
{
    // Presentation uses unscaled time, so paused menus remain responsive.
    public sealed class PanelReveal : MonoBehaviour
    {
        CanvasGroup group;float time;
        void Awake(){group=gameObject.AddComponent<CanvasGroup>();group.alpha=0;}
        void Update(){time+=Time.unscaledDeltaTime;group.alpha=Mathf.SmoothStep(0,1,time/.18f);if(time>=.18f)enabled=false;}
        public void Complete(){if(group==null)group=GetComponent<CanvasGroup>();group.alpha=1;enabled=false;}
    }
}
