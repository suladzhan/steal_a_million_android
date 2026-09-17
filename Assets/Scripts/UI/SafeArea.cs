using UnityEngine;

namespace StealAMillion
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        private Rect previous;
        private Vector2 screen;
        private void OnEnable() { Apply(); }
        private void Update()
        {
            if (previous != Screen.safeArea || screen.x != Screen.width || screen.y != Screen.height) Apply();
        }
        private void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            previous = Screen.safeArea;
            screen = new Vector2(Screen.width, Screen.height);
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(previous.xMin / screen.x, previous.yMin / screen.y);
            rect.anchorMax = new Vector2(previous.xMax / screen.x, previous.yMax / screen.y);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
