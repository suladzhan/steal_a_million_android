using UnityEngine;

namespace StealAMillion
{
    public sealed class StageLayout : MonoBehaviour
    {
        private void OnRectTransformDimensionsChange()
        {
            var rect = (RectTransform)transform;
            var parent = transform.parent as RectTransform;
            if (parent == null) return;
            var size = new Vector2(Mathf.Min(540, Mathf.Max(1, parent.rect.width - 36)), Mathf.Max(1, parent.rect.height - 24));
            if (rect.sizeDelta != size) rect.sizeDelta = size;
        }
        private void OnEnable() { OnRectTransformDimensionsChange(); }
        private void LateUpdate() { OnRectTransformDimensionsChange(); }
    }
}
